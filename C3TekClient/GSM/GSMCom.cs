using C3TekClient.C3Tek;
using C3TekClient.Core.JobScheduler;
using C3TekClient.Helper;
using C3TekClient.MyMobifone;
using C3TekClient.MyViettel;
using C3TekClient.MyVNMB;
using C3TekClient.MyVNPT;
using GsmComm.PduConverter;
using GsmComm.PduConverter.SmartMessaging;
using SMSPDULib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using WSafeSerialPort.Serial;

namespace C3TekClient.GSM
{
    public class TaskConsumeDataResult
    {
        public int CurrentIdx { get; set; }
        public string Result { get; set; }
    }
    public class GSMCom
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string SIMCOM_SIM5320E = "SIMCOM_SIM5320E";
        public static string EC20F = "EC20F";
        public static string MC55 = "MC55";
        public bool is_khmer_change_imei = false;
        public static bool is_set_date = false;
        public bool is_reset_port = false;
        public List<string> imeiList = new List<string>();
        public bool AutoAssignCarrier { get; set; }
        public SIMCarrier CarrierDefault { get; set; }
        private string _DisplayName { get; set; }
        public string DisplayName

        {
            get { return _DisplayName; }
            set
            {
                _DisplayName = value;
                var manual = GlobalVar.UserSetting.ManualPortNames.FirstOrDefault(m => m.WindowsPortName == PortName);
                if (manual != null)
                {
                    manual.UserPortName = _DisplayName;
                }
                else
                {
                    GlobalVar.UserSetting.ManualPortNames.Add(new ManualPortName() { UserPortName = _DisplayName, WindowsPortName = PortName });
                }
            }

        }
        public SMSMemory[] SMSMemory = new SMSMemory[]
        {
            new SMSMemory(){ Name = "SM" },
            new SMSMemory(){ Name = "ME" },
            new SMSMemory(){ Name = "MT" },
        };
        private const string ENTER = @"
";
        private const string Ctrl_Z = "";
        public bool Stop = false;
        public PortState PortState { get; set; }
        public TTTB TTTB { get; set; }
        public bool DoNotConnect { get; set; }
        public bool Log { get; set; }
        public string PortName { get; private set; }
        public string Serial { get; set; }
        public string ImeiDevice { get; set; }
        public string ModemName { get; set; }
        public string PhoneNumber { get; set; }
       
        public bool IsPortConnected { get; set; }
        public bool IsSIMConnected { get; set; }
        private bool ComStarted = false;
        public int MainBalance { get; set; }
        public string Expire { get; set; }
        public string LastUSSDCommand { get; set; }
        public string LastUSSDResult { get; set; }
        public bool IsCalling { get; set; }
        public RingStatus RingStatus { get; set; }
        public Action<GSMCom, RingStatus> RingStatusChanged = (com, ringStatus) => { };
        public SIMCarrier SIMCarrier { get; set; }
        private Thread PortConnectionHandler { get; set; }
        private Thread SIMConnectionHandler { get; set; }
        private Thread GSMMessageHandler { get; set; }
        private Thread AlertSMSHandler { get; set; }
        public MyRegisterState MyRegisterState { get; set; }
        public string MyProcessMessage { get; set; }
        public int SignalInt { get; set; }

        public int LimitMess=0;
        public string SignalStr
        {
            get
            {
                if (0 <= SignalInt && SignalInt <= 9)
                {
                    return "Yếu";
                }
                else if (10 >= SignalInt && SignalInt <= 14)
                {
                    return "Tạm ổn";
                }
                else if (15 >= SignalInt && SignalInt <= 19)
                {
                    return "Tốt";
                }
                else if (SignalInt >= 20 && SignalInt <= 31)
                {
                    return "Rất tốt";
                }
                else
                {
                    return "Không xác định";
                }
            }
        }
        public string MyRegisterStateText
        {
            get
            {
                return MyRegisterState == MyRegisterState.None ? (SIMCarrier == SIMCarrier.NO_SIM_CARD ? string.Empty : "")
                   : MyRegisterState == MyRegisterState.Processing ? "ĐANG ĐĂNG KÝ"
                   : MyRegisterState == MyRegisterState.NoOTP ? "KHÔNG VỀ OTP"
                   : MyRegisterState == MyRegisterState.Failed ? "THẤT BẠI"
                   : MyRegisterState == MyRegisterState.Succeed ? "THÀNH CÔNG"
                   : "KHÔNG XÁC ĐỊNH";
            }
        }

        private Thread AlertCallHandler { get; set; }

        private object RequestLocker = new object();
        public object RequestLockerMessage = new object();

        //default constructor
        public Core.JobScheduler.JobWorker ussdScheduler;
        public Core.JobScheduler.SocketJobWorker socketJobWorker;
        public GSMCom()
        {
            AutoAssignCarrier = true;
            CarrierDefault = SIMCarrier.NO_SIM_CARD;
            imeiList = imeiList.Concat(getAllTypeTalco("Apple")).Concat(getAllTypeTalco("Samsung")).Concat(getAllTypeTalco("HTC")).Concat(getAllTypeTalco("Motorola")).ToList();
            ussdScheduler = new JobWorker(this);
            socketJobWorker = new SocketJobWorker(this);
        }
        public void GetDeviceFirmware()
        {
            lock (RequestLocker)
            {
                Port.WriteLine("ATI");
                String response = WaitResultOrTimeout("Revision", 1000, true);
                Regex r = new Regex("Revision: (.*)\\W", RegexOptions.IgnoreCase);

                Match m = r.Match(response);

                if(m.Success)
                {
                    this.MyProcessMessage = m.Value;
                }
                else
                {
                    this.MyProcessMessage = "Không nhận dạng được firmware" + response;
                }
            }
        }
        public SocketJobWorker GetSocketJobWorker()
        {
            return this.socketJobWorker;
        }
        public JobWorker GetUSSDJobWorker()
        {
            return ussdScheduler;
        }
        private void LogResponseCommand(string response)
        {
            if (!string.IsNullOrEmpty(response) && Log)
            {
                GlobalEvent.ONATCommandResponse($"====[ {PortName} ]====\n\n{response}=============\n");
            }
            //if (SIMCarrier == SIMCarrier.VietnamMobile && !string.IsNullOrEmpty(response) && response.Contains("TK Chinh:"))
            //{
            //    try
            //    {
            //        CultureInfo provider = CultureInfo.InvariantCulture;
            //        string accountInfo = Regex.Match(response, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))").Value;

            //        var matchBalance = Regex.Match(accountInfo, "(TK Chinh: (.*?)d,)");
            //        var matchExpire = Regex.Match(accountInfo, "(\\d{2}\\/\\d{2}\\/\\d{4})");
            //        if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
            //        {
            //            MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK Chinh: ", "").Replace("d,", ""));
            //        }
            //        if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
            //        {
            //            Expire = matchExpire.Value;
            //        }
            //    }
            //    catch { }
            //}
        }
        DateTime LastRing = DateTime.Now;
        private void RingDetector(string response)
        {
            if (response.Contains("RING"))
            {
                RingStatus = RingStatus.Ringing;
                LastRing = DateTime.Now;
                RingStatusChanged(this, RingStatus);
                //string info = ExecuteCommand(GSMCommand.GETCALLERID);
                GlobalEvent.OnGlobalMessaging($"{this.PhoneNumber} -> Ring Ring");
                if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT))
                {
                    ExecuteCommand(GSMCommand.ANSWER_CALL, null);
                }
            }
            else
            {
                if (RingStatus == RingStatus.Ringing)
                {
                    if ((DateTime.Now - LastRing).TotalSeconds > 5)
                    {
                        RingStatus = RingStatus.Idle;
                        RingStatusChanged(this, RingStatus);
                        GlobalEvent.OnGlobalMessaging($"{this.PhoneNumber} -> Missed");
                    }
                }
            }
        }
        private Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void testReadWav()
        {
            lock (RequestLocker)
            {
                Port.WriteLine("AT+QFDWL=\"demo.wav\";");
                Thread.Sleep(1000);
                read_func:
                //Thread.Sleep(200);
                string audioString = "";


                byte[] buffer = Port.ReadBytes(5000, 5000);
                int bytesRead = buffer.Length;
                if (bytesRead == 0)
                {
                    return;
                }
                //int bytesRead = 0;
                //try
                //{
                //    bytesRead = Port.Read(buffer, 0, buffer.Length);
                //    if (bytesRead == 0)
                //        break;
                //}
                //catch
                //{
                //}
                using (var stream = System.IO.File.Open("C:\\Users\\snowd\\Desktop\\test.amr", System.IO.FileMode.Append))
                {
                    stream.Write(buffer, 0, bytesRead);
                }

                audioString = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                string pattern_connect = "\r\nCONNECT\r\n";
                int ix = audioString.IndexOf(pattern_connect);
                if (ix != -1)
                {
                    audioString = audioString.Substring(ix + pattern_connect.Length);
                    byte[] newArray = new byte[buffer.Length - 11];
                    Buffer.BlockCopy(buffer, 11, newArray, 0, newArray.Length);
                    buffer = newArray;
                    // do something here
                }


                if (!string.IsNullOrEmpty(audioString) && !audioString.Contains("+QFLDS") && !audioString.Contains("+CPMS") && !audioString.Contains("CMT")
                    && !audioString.Contains("ERROR"))
                {
                    if (audioString.Contains("RIFF"))
                    {
                        //Restart record
                        RIFFTimeline.Clear();
                        RIFFTimeline.Add(buffer.Skip(38).Take(bytesRead - 38).ToArray(), string.Empty);
                        goto read_func;
                    }
                    else
                    {
                        if (audioString.Contains("+QFDWL"))
                        {
                            RIFFTimeline.Add(buffer.Take(bytesRead - 29).ToArray(), string.Empty);
                            List<byte[]> recordValue = new List<byte[]>();
                            foreach (var item in RIFFTimeline)
                                recordValue.Add(item.Key);
                            byte[] record = recordValue.SelectMany(s => s).ToArray();

                            // voiceContent = new C3TekPortal().VoiceRecognitionToText(record);

                            //var temp = Path.GetTempFileName();
                            //File.WriteAllBytes(temp, record);
                            //string voiceContent = new SpeechToTextHelp
                            //er().SpeechToText(temp);


                            //lock (GSMControlCenter.LockGSMMessages)
                            //{
                            //    GSMControlCenter.GSMMessages.Add(message);
                            //    GSMControlCenter.OnNewMessage(message);
                            //    NotifyAlert();
                            //}
                            string temp_file = $"P__{((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString()}_c.amr";
                            System.IO.File.WriteAllBytes(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/voice_data/" + temp_file, record);
                            int Subchunk1Size = BitConverter.ToInt32(record, 0);
                            return;
                        }
                        RIFFTimeline.Add(buffer.Take(bytesRead).ToArray(), string.Empty);
                        goto read_func;
                    }
                }
            }
        }
        public List<string> getAllTypeTalco(string brand_name)
        {
            List<string> talcos = new List<string>();
            try
            {
                string file = "imeidb.csv";
                using (StreamReader sr = new StreamReader(file))
                {
                    while (!sr.EndOfStream)
                    {
                        string row = sr.ReadLine();
                        string[] column = row.Split(',');
                        string talco = column[0];
                        string brand = column[1];
                        if (brand.ToUpper().Contains(brand_name.ToUpper()))
                        {
                            talcos.Add(talco);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("file" + ex.Message);
            }
            catch (Exception er)
            {
                Console.WriteLine("er" + er.Message);
            }
            return talcos;
        }
        public int sumOfDigit(int n)
        {
            int res = 0;
            while (n != 0)
            {
                res += n % 10;
                n /= 10;
            }
            return res;
        }
        public int calculateDoubleDigitTalco(string talco)
        {
            int res = 0;
            for (int i = 1; i < 8; i += 2)
            {
                res += sumOfDigit(int.Parse(talco[i].ToString()) * 2);
            }
            return res;
        }
        public int calculateSingleDigitTalco(string talco)
        {
            int res = 0;
            for (int i = 0; i < 8; i += 2)
            {
                res += int.Parse(talco[i].ToString());
            }
            return res;
        }
        public int calculateSumFromCheckDigit(int checkDigit)
        {
            if (checkDigit > 9 || checkDigit < 0)
            {
                return -1;
            }
            int val = 10 - checkDigit;

            int res = val + 50;

            return res;
        }

        public int calculateRemainingSum(string talco, int checkDigit)
        {
            int double_digit = calculateDoubleDigitTalco(talco);
            int single_digit = calculateSingleDigitTalco(talco);
            int sum_both = double_digit + single_digit;
            int origin_sum = calculateSumFromCheckDigit(checkDigit);
            int res = 0;
            if (origin_sum != -1)
            {
                res = origin_sum - sum_both;
            }
            return res;
        }

        public string generateSequenceNumber(int remaining)
        {
            string sqn = "";
            try
            {
                for (int x1 = 0; x1 < 10; x1++)
                {
                    for (int x2 = 0; x2 < 10; x2++)
                    {
                        for (int x3 = 0; x3 < 10; x3++)
                        {
                            for (int x4 = 0; x4 < 10; x4++)
                            {
                                for (int x5 = 0; x5 < 10; x5++)
                                {
                                    for (int x6 = 0; x6 < 10; x6++)
                                    {
                                        int sum = x1 + sumOfDigit(2 * x2) + x3 + sumOfDigit(2 * x4) + x5 + sumOfDigit(2 * x6);
                                        if (sum == remaining)
                                        {
                                            sqn = "" + x1 + x2 + x3 + x4 + x5 + x6;
                                            return sqn;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return sqn;
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }
            return sqn;
        }
        public string generatePhoneImeiNumber(string talco, int check_digit)
        {

            string imei = "";
            try
            {
                int remaining = calculateRemainingSum(talco, check_digit);
                string sqn = generateSequenceNumber(remaining);
                imei = talco + sqn + check_digit.ToString();
                return imei;
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);
            }
            return imei;
        }

        public void activation(string number, string dtmf, int delaytimeKH)
        {
            try
            {
                if (this.ModemName == "MC55")
                {
                    Port.WriteLine("AT^SM20=0");
                    WaitResultOrTimeout("", 1000);
                }

                Port.Write("ATH\r\n");
                WaitResultOrTimeout("", 2000);

                Thread.Sleep(1000);


                Port.WriteLine("AT+SPEAKER=1");
                WaitResultOrTimeout("", 300);
                //resetPort();

                string command = "";
                if (this.ModemName == "MC55")
                {
                    command = "ATDT" + number + ";";
                }

                else
                {
                    command = "ATD" + number + ";";
                }

                Port.WriteLine(command);
                WaitResultOrTimeout("", 10000);

                Thread.Sleep((delaytimeKH * 1000) + 100);
                //Send DTMF
                //string alcc = "";
                //while (true)
                //{
                //    alcc = ExecCommand("AT+CLCC", 5000);
                //    if (alcc.Contains("1,0,0,0"))
                //    {
                //        break;
                //    }
                //    Thread.Sleep(1000);
                //    //this.port.Write("ATH" + char.ConvertFromUtf32(13) + "\r");
                //}
                command = "AT+VTD=6;+VTS=" + dtmf;

                Port.WriteLine(command);
                WaitResultOrTimeout("", 4000);
                //this.port.Write("AT+VTD=6;+VTS=1");
                Thread.Sleep(3000);
                //command = "AT+VTD=6;+VTS=1";// +dtmf;
                //ExecCommand(command, 4000);
                ////this.port.Write("AT+VTD=6;+VTS=1");
                //Thread.Sleep(3000);
                //command = "AT+VTD=6;+VTS=1";// +dtmf;
                //ExecCommand(command, 4000);
                ////this.port.Write("AT+VTD=6;+VTS=1");
                //Thread.Sleep(3000);
                //command = "AT+VTD=6;+VTS=1";// +dtmf;
                //ExecCommand(command, 4000);
                ////this.port.Write("AT+VTD=6;+VTS=1");
                //Thread.Sleep(3000);
                Port.WriteLine("ATH");
                Thread.Sleep(1000);
                /*
                string command = "ATDT +84907811703;";
                ExecCommand(command, 10000);
                Thread.Sleep(7000);
                ExecCommand("ATH", 300);
                */
            }
            catch (Exception ex)
            {
                //ErrorLog(ex.ToString());
            }
        }
        public void activation(string number, string dtmf1, string dtmf2, int delaytimeKH)
        {
            try
            {
                if (this.ModemName == "MC55")
                {
                    Port.WriteLine("AT^SM20=0");
                    WaitResultOrTimeout("", 1000);
                }


                Port.Write("ATH\r\n");
                WaitResultOrTimeout("", 2000);

                /*
                string command = "ATDT +84907811703;";
                ExecCommand(command, 10000);
                Thread.Sleep(7000);
                ExecCommand("ATH", 300);
                */
                Thread.Sleep(1000);

                Port.WriteLine("AT+SPEAKER=1");
                WaitResultOrTimeout("", 300);

                string command = "";
                if (this.ModemName == "MC55")
                {
                 command = "ATDT" + number + ";";
                }

                else
                {
                    command= "ATD" + number + ";";
                }

                Port.WriteLine(command);
                WaitResultOrTimeout(command, 2000);
                Thread.Sleep((delaytimeKH * 1000) + 100);
                //Send DTMF

                command = "AT+VTD=6;+VTS=" + dtmf1;
                Port.WriteLine(command);
                WaitResultOrTimeout("", 2000);
                Thread.Sleep(10000);
                command = "AT+VTD=6;+VTS=" + dtmf2;
                Port.WriteLine(command);
                WaitResultOrTimeout("", 2000);
                Thread.Sleep(1000);
                command = "AT+VTD=6;+VTS=" + dtmf2;
                Port.WriteLine(command);

                WaitResultOrTimeout("", 2000);
                //Thread.Sleep(5000);
                //Hang up
                Port.Write("ATH\r\n");
                WaitResultOrTimeout("", 2000);

                /*
                string command = "ATDT +84907811703;";
                ExecCommand(command, 10000);
                Thread.Sleep(7000);
                ExecCommand("ATH", 300);
                */
                Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{PortName} : {ex.Message} when use activation");
                //ErrorLog(ex.ToString());
            }
        }
        //send.amr
        void playAudio(string filename, int number_replay)
        {

            Port.WriteLine("AT+QPSND=1,\"RAM:" + filename + "\"," + number_replay.ToString() + ",7,7,0");
            string response = WaitResultOrTimeout("", 1000);
            Console.WriteLine("[Play Audio]" + response);
        }
        public void safeDownloadAudioToDevice(string filename, string local_path)
        {
            lock (RequestLocker)
            {
                downloadAudioToDevice(filename, local_path);
            }
        }
        void downloadAudioToDevice(string filename, string local_path)
        {
            try
            {
                MyProcessMessage = "Đang đọc dữ liệu audio";
                byte[] data = File.ReadAllBytes(local_path);
                string response = "";
                string addr = "";
                this.ENABLE_LOOP_BACK_SERIAL = true;
                //Port.StartLoggerWrite("writer.log", true);
                if (ModemName == "M26")
                {
                    Port.WriteLine("AT+QFDEL=\"RAM:*\"");
                    Thread.Sleep(1000);
                    response = Port.ReadExisting();
                    Port.WriteLine("AT+QFLST=\"RAM:*\"");
                    response = WaitResultOrTimeout("QFLST", 2000);
                    if (response.Contains("ERROR"))
                    {
                        MyProcessMessage = "Có lỗi xảy ra khi truyền file, vui lòng khởi động lại modem và upload lại";
                        return;
                    }
                    //if (!response.Contains(filename))
                    {
                        //tham so thu 3 dung cho RAM
                        Port.WriteLine($"AT+QFOPEN=\"RAM:{filename}\",0,{(data.Length + 1000).ToString()}");
                        Thread.Sleep(3000);
                        response = WaitResultOrTimeout("QFOPEN", 3000);
                        addr = StringHelper.ParseDigitString(response) ?? "";
                        MyProcessMessage = "Đang ghi dữ liệu audio";

                        if (addr == "")
                        {
                            MyProcessMessage = "Có lỗi xảy ra khi truyền file, vui lòng khởi động lại modem và upload lại";
                            return;
                        }
                        Port.WriteLine($"AT+QFWRITE={addr},{data.Length.ToString()},20");
                        Thread.Sleep(100);
                        response = WaitResultOrTimeout("CONNECT", 1000);
                        WaitEmptyResponse();
                        foreach (byte e in data)
                        {
                            Port.Write(e);

                        }
                        response = WaitResultWithCommand("QFWRITE", 20000);
                        Port.WriteLine($"AT+QFCLOSE={addr}");
                        response = WaitResultOrTimeout("", 1000);
                        if (response.Contains("ERROR"))
                        {
                            MyProcessMessage = "Có lỗi xảy ra khi truyền file, vui lòng khởi động lại modem và upload lại";
                            return;
                        }
                        //var match_len_str = Regex.Match(result, "(\\" + cmd_expected + ": (.*)\r)");
                        //if (match_len_str.Value != null && match_len_str.Value.Length > 10)
                        //{
                        //    return (!string.IsNullOrEmpty(match_len_str.Value) ? match_len_str.Value : "");
                        //}
                        MyProcessMessage = "Tải audio lên thiết bị thành công";
                       

                    }
                    //else
                    //{
                    //    MyProcessMessage = "Audio đã tồn tại";

                    //}

                    //Port.EndLoggerWrite();


                }
                this.ENABLE_LOOP_BACK_SERIAL = false;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Download err");
            }
        }



        public string MakeCallAndPlay(string phone_number, int duration, bool is_record_play, string record_fn, int record_number_play)
        {
            string response = "";
            lock (RequestLocker)
            {
                try
                {
                    Port.WriteLine("AT+COLP=1");
                    response = WaitResultOrTimeout("", 1000);
                    IsCalling = true;

                    Port.WriteLine("ATH");
                    Thread.Sleep(100);
                    LogResponseCommand(Port.ReadExisting());

                    Port.WriteLine("ATD" + phone_number + ";");
                    Thread.Sleep(1000);
                    response = Port.ReadExisting();
                    LogResponseCommand(response);


                    if (ModemName == "M26")
                    {
                    //m26
                    loop:
                        Port.WriteLine("AT+CPAS");
                        response = WaitResultOrTimeout("CPAS", 1000);
                        if (response.Contains("CPAS: 4"))
                        {
                            if (is_record_play)
                            {
                                playAudio(record_fn, record_number_play);
                            }
                            Thread.Sleep(duration * 1000);
                            Port.WriteLine("ATH");
                            Thread.Sleep(100);
                            //LogResponseCommand(Port.ReadExisting());
                            response = "CALL_OK";
                        }
                        else
                        {
                            if (response.Contains("BUSY"))
                            {
                                response = "BUSY";
                            }
                            else if (response.Contains("CPAS: 3"))
                            {
                                goto loop;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    IsCalling = false;

                }
                finally
                {
                    IsCalling = false;
                    Port.WriteLine("ATH");
                    Thread.Sleep(100);
                }
            }
            return response;
        }

        public string ActivateSIM(string number, string dtmf1, string dtmf2, int delaytimeKH = 20)
        {
            string response = "";
            bool flag = true;
            string blance = "";
            if (number=="1529")
            {
                blance = ExecuteSingleUSSD("*097#");
            }
            else
            {
                blance=ExecuteSingleUSSD("*101#");
            }
            //if (string.IsNullOrEmpty(checkBalance))
            //{
            //    this.LastUSSDResult = "Có lỗi xảy ra khi kiểm tra tài khoản";
            //    response = "FALSE";
            //}

            if (string.IsNullOrEmpty(blance) || blance.Contains("Your number is inactive") || blance.Contains("Yeu cau cua quy khach khong the thuc hien") || blance.Contains("chua kich hoat") || blance.Contains("kich hoat") || blance.Contains("Quy khach can") || blance.Contains("goi 900") || blance.Contains("Nghẽn mạng.") || blance.Contains("Kết nối thẻ SIM chưa sẵn sàng") || blance.Contains("ERROR"))
            {
                this.LastUSSDResult = "Đang kích hoạt sim";
                if (string.IsNullOrEmpty(dtmf2))
                {
                    this.activation(number, dtmf1, delaytimeKH);
                }
                else
                {
                    this.activation(number, dtmf1, dtmf2, delaytimeKH);
                }
                this.LastUSSDResult = "kích hoạt sim hoàn tất";
                Thread.Sleep(2000);
                this.LastUSSDResult = "Kiểm tra kết quả kích hoạt....";
                string balance = "";
                if (number == "1529")
                {
                    balance = ExecuteSingleUSSD("*097#");
                }
                else
                {
                    balance = ExecuteSingleUSSD("*101#");
                }
                if (string.IsNullOrEmpty(balance) || balance.Contains("Yeu cau cua quy khach khong the thuc hien") || balance.Contains("Nghẽn mạng.") || balance.Contains("Kết nối thẻ SIM chưa sẵn sàng") || balance.Trim() == "" || balance.Contains("chua kich hoat") || balance.Contains("inactive") || balance.Contains("kich hoat") || balance.Contains("Quy khach can") || balance.Contains("goi 900") || balance.Contains("ERROR"))
                {
                    this.LastUSSDResult = "Kích hoạt không thành công, vui lòng kiểm tra lại sim và thử lại....";
                }
                else
                {
                    this.LastUSSDResult = "Thành công, " + balance;
                }
            }
            return response;
        }
        public string ExecuteCommand(GSMCommand command, object data = null)
        {

            lock (RequestLocker)
            {
                string response = string.Empty;
                try
                {
                    if (Stop)
                        return string.Empty;

                    if (command != GSMCommand.CHECKRING)
                    {
                        string temp_response = Port.ReadExisting();
                        if (temp_response.Contains("RING"))
                        {
                            this.RingDetector(temp_response);
                            //command = GSMCommand.ANSWER_CALL;
                        }
                    }

                    if (IsPortConnected)
                    {
                        switch (command)
                        {
                            case GSMCommand.ACTIVATE_SIM:
                                break;
                            case GSMCommand.GET_SIGNAL:
                                Port.WriteLine("AT+CSQ");
                                response = WaitResultWithCommand("+CSQ", 1000);
                                break;
                            case GSMCommand.INIT_COMMAND:
                                Port.Write("" + (char)27);
                                Port.WriteLine("ATE0");
                                response = WaitResultOrTimeout("", 1000);
                                break;
                            case GSMCommand.GET_MODEM_NAME:
                                {
                                    Port.ReadExisting();
                                    Port.WriteLine("AT+CGMM");
                                    Thread.Sleep(500);
                                    response = Port.ReadExisting();
                                    break;
                                }
                            case GSMCommand.CPIN:
                               {
                                    Port.WriteLine("AT+CPIN?");
                                    Thread.Sleep(1000);
                                    response = Port.ReadExisting();
                                    if (ModemName == "WaveCom")
                                    {
                                        response += "OK";
                                    }
                                    LogResponseCommand(response);
                                    break;
                               }
                            case GSMCommand.GET_IMEI:
                                {   
                                    Port.Write("AT+CGSN\r");
                                    Thread.Sleep(100);
                                    response = WaitResultOrTimeout("OK", 5000);
                                    if(!string.IsNullOrEmpty(response) && (AppModeSetting.Locale.Equals("km-KH") || AppModeSetting.Locale.Equals("en-US")))
                                    {  
                                        if(is_khmer_change_imei)
                                        {
                                            is_khmer_change_imei = false;
                                            this.ChangeRandomIMEI();
                                        }
                                    }
                                    break;
                                }
                            case GSMCommand.CHANGE_EMEI:
                                {   
                                    string imei = (string)data;
                                    Port.Write($"AT+EGMR=1,7,\"{imei}\"\r\n");
                                    Thread.Sleep(1000);
                                    response=WaitResultOrTimeout("OK", 5000);
                                    this.ImeiDevice = string.Empty;
                                    break;
                                }

                            case GSMCommand.SWITCH_TEXT_MODE:
                                {
                                    Port.WriteLine("AT+CMGF=1");
                                    //Port.WriteLine("AT+CLIP=1");
                                    WaitResultOrTimeout("OK", 1000);

                                    Port.WriteLine("AT+CSCS=\"GSM\"");
                                    //Port.WriteLine("AT+CLIP=1");
                                    WaitResultOrTimeout("OK", 1000);

                                    WaitResultOrTimeout("OK", 2000);
                                   
                                    //Port.WriteLine("AT+CPMS=\"MT\"");
                                    //WaitResultOrTimeout("CPMS", 500);
                                   
                                    //response = Port.ReadExisting();
                                    break;
                                }
                            case GSMCommand.GET_CARRIER:
                                {
                                
                                    Port.WriteLine("AT+COPS?");
                                    //  response = Port.ReadExisting();
                                    response = WaitResultOrTimeout("COPS", 1000);
                                    Port.WriteLine("AT+CNUM");
                                    string cnum_response = WaitResultOrTimeout("+CNUM", 2000);
                                    if(cnum_response.Contains("+66"))
                                    {
                                        response = "+COPS: 0,0,\"DTAC\"";
                                    }
                                   
                                    LogResponseCommand(response);
                                    break;
                                }
                            case GSMCommand.GET_ICCID:
                                {
                                    if (ModemName == "MC55")
                                    {
                                        //AT+CIMI

                                        Port.WriteLine("AT^SCID");
                                        Thread.Sleep(1000);
                                        response = WaitResultOrTimeout("", 500);
                                    }
                                    else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                    {
                                        Port.WriteLine("AT+CICCID");
                                        response = WaitResultOrTimeout("ICCID", 1000);
                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = StringHelper.ParseDigitString(response);
                                        }
                                    }
                                    else if (ModemName.Contains("WaveCom") || ModemName.Contains("Wavecom"))
                                    {
                                        Port.WriteLine("AT+CCID");
                                        response = WaitResultOrTimeout("+CCID", 1000);
                                        if (!string.IsNullOrEmpty(response))
                                        {
                                            response = StringHelper.ParseDigitString(response);
                                        }
                                    }
                                    else
                                    {
                                        Port.WriteLine("AT+ICCID?");
                                        response = WaitResultOrTimeout("ICCID", 500);
                                        response = StringHelper.ParseDigitString(response);

                                        if (string.IsNullOrEmpty(response))
                                        {
                                            Port.WriteLine("AT+CCID");
                                            response = WaitResultOrTimeout("CCID", 500);
                                            response = StringHelper.ParseDigitString(response);
                                        }
                                        if (string.IsNullOrEmpty(response))
                                        {
                                            Port.WriteLine("AT+CCID");
                                            response = WaitResultOrTimeout("CCID", 500);
                                            response = StringHelper.ParseDigitString(response);
                                        }

                                        LogResponseCommand(response);
                                    }
                                    break;
                                }

                            case GSMCommand.CONSUME_DATA:
                                {
                                    string package_size_download = "5";
                                    if (data != null)
                                    {
                                        package_size_download = data.ToString();
                                    }
                                    string size_5mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/5mb.txt";
                                    string size_1mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/1mb.txt";
                                    string size_3mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/3mb.txt";
                                    string size_10mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/10mb.txt";
                                    string size_25mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/25mb.txt";
                                    string size_50mb = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/50mb.txt";
                                    List<string> list_size = new List<string> { size_1mb, size_3mb, size_5mb, size_10mb, size_25mb, size_50mb };
                                    string sample = "";
                                    foreach (string size in list_size)
                                    {
                                        Regex reg = new Regex("[^0-9]");
                                        string val= reg.Replace(size, "");
                                        if(val==package_size_download)
                                        {
                                            sample = size;
                                        }
                                    }
                  

                                    var task2watch = new Stopwatch();
                                    task2watch.Start();
                                 
                                    if (ModemName == "UC20" || ModemName == "EC20" || ModemName == "EC20F")
                                    {

                                        Port.WriteLine("AT+COPS=0,0,6");
                                        Thread.Sleep(2000);

                                        response = Port.ReadExisting();
                                        GlobalEvent.ONATCommandResponse("COPS state : => " + response);


                                        Port.WriteLine("AT+QHTTPCFG=\"contextid\",1");
                                        response = WaitResultOrTimeout("", 1000); //timeout 20s

                                        Port.WriteLine("AT+QHTTPCFG=\"responseheader\",1");
                                        switch (SIMCarrier)
                                        {
                                            case SIMCarrier.NO_SIM_CARD: { break; }
                                            case SIMCarrier.VietnamMobile:
                                                {
                                                    Port.WriteLine("AT+QICSGP=1,1,\"internet\",\"\",\"\",1");
                                                    break;
                                                }
                                            case SIMCarrier.Viettel:
                                                {
                                                    Port.WriteLine("AT+QICSGP=1,1,\"v-internet\",\"\",\"\",1");
                                                    break;
                                                }
                                            case SIMCarrier.Vinaphone:
                                                {
                                                    Port.WriteLine("AT+QICSGP=1,1,\"m3-world\",\"mms\",\"mms\",1");
                                                    break;
                                                }
                                            case SIMCarrier.Mobifone:
                                                {
                                                    Port.WriteLine("AT+QICSGP=1,1,\"m-wap\",\"mms\",\"mms\",1");
                                                    break;
                                                }
                                        }
                                        Port.WriteLine("AT+QIACT=1");
                                        response = WaitResultErrorOrTimeout("", 5000);
                                       //timeout 20s
                                        Thread.Sleep(100);
                                       
                                        int size_package = 3 * 1024 * 1024;
                                        string url = "https://www.cika.com/soporte/Information/GSMmodules/Quectel/EC20/Quectel_EC20_AT_Commands_Manual.pdf";
                                        //  url = "https://";
                                        // url = "https://github.com/jamesward/play-load-tests/raw/master/public/10mb.txt";
                                        // url = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/10mb.txt"; 
                                        url = GSMPayloadSchemasInstance._payloadInstance.payloads[$"U_{package_size_download}MB"].URL.Replace("\r","").Replace("\r\n","").Replace("\n","").Trim();
                                        int timeoutInSecond = GSMPayloadSchemasInstance._payloadInstance.payloads[$"U_{package_size_download}MB"].TimeWaitTimeOutInSecond;
                                        string cmd_internet = $"AT+QHTTPURL={sample.Length},80";
                                        Port.WriteLine(cmd_internet);
                                        response = WaitResultOrTimeout("CONNECT", 5000);
                                     //timeout 20s
                                        Thread.Sleep(100);
                                        //http://dulieu.tailieuhoctap.vn/books/giao-duc-dai-cuong/toan-roi-rac/file_goc_768449.pdf
                                        Port.WriteLine(sample);
                                        GlobalEvent.ONATCommandResponse(url);
                                        Thread.Sleep(1000);
                                        Port.WriteLine("AT+QHTTPGET=80");
                                        response = WaitResultOrTimeout("QHTTPGET", 5000);
                                        GlobalEvent.ONATCommandResponse(response);
                                        //Port.WriteLine("AT+QHTTPPOST="+size.ToString()+",80,80");
                                        //response = WaitResultOrTimeout("QHTTPPOST", 2000); //timeout 20s
                                        //string _dataBody = RandomString(size);
                                        //Port.WriteLine("http://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpbin.org/posthttp://httpb");
                                        //Port.WriteLine(_dataBody);
                                        Port.WriteLine($"AT+QHTTPREAD=80");
                                        response = WaitResultOrTimeout("QHTTPREAD", 1000 * 300 * 12); //timeout 20s
                                    }
                                    else
                                    {

                                        Port.WriteLine("AT+CGDCONT=1,\"IP\",\"m3-world\",\"0.0.0.0\"");
                                        WaitResultOrTimeout("", 100);
                                        Port.WriteLine("AT+CGSOCKCONT=1,\"IP\",\"m3-world\"");
                                        WaitResultOrTimeout("", 100);
                                        Port.WriteLine("AT+CSOCKSETPN=1");
                                        WaitResultOrTimeout("", 100);

                                        int size_need_http = Int32.Parse(package_size_download);



                                        string host_1mb_http = "speedtest.tele2.net";
                                        string path_1mb_http = "/" + package_size_download + "MB.zip";
                                        int timeout_http = 60 * 1000 * size_need_http;
                                        string temp_rslt = "";
                                        Port.WriteLine($"AT+CHTTPACT=\"{host_1mb_http}\",80");
                                        response = WaitResultOrTimeout("REQUEST", 5000, false);
                                        Port.WriteLine($"GET {path_1mb_http} HTTP/1.1");
                                        Thread.Sleep(100);
                                        Port.WriteLine($"Host:{host_1mb_http}\r\n\r\n\r\n");
                                        Thread.Sleep(100);
                                        Port.Write(char.ConvertFromUtf32(26));
                                        Thread.Sleep(100);
                                        temp_rslt = WaitResultOrTimeout("+CHTTPACT: 0", 60 * 1000 * 2 * size_need_http, false);
                                        if (!string.IsNullOrEmpty(temp_rslt))
                                        {
                                            response += temp_rslt;
                                        }
                                        else
                                        {
                                            response = "";
                                            break;
                                        }
                                        Thread.Sleep(3000);

                                        //for (int i = 0; i < size_need_http; i++)
                                        //{
                                        //    Port.WriteLine($"AT+CHTTPACT=\"{host_1mb_http}\",80");
                                        //    response = WaitResultOrTimeout("REQUEST", 5000, false);

                                        //    Port.WriteLine($"GET {path_1mb_http} HTTP/1.1");
                                        //    Thread.Sleep(100);
                                        //    Port.WriteLine($"Host:{host_1mb_http}\r\n\r\n\r\n");
                                        //    Thread.Sleep(100);
                                        //    Port.Write(char.ConvertFromUtf32(26));
                                        //    Thread.Sleep(100);
                                        //    temp_rslt = WaitResultOrTimeout("+CHTTPACT: 0", 60 * 1000, false);
                                        //    if (!string.IsNullOrEmpty(temp_rslt)){
                                        //        response += temp_rslt;
                                        //    }
                                        //    else
                                        //    {
                                        //        response = "";
                                        //        break;
                                        //    }
                                        //    Thread.Sleep(3000);
                                        //}

                                        if (response.Contains("ERROR") || response.Contains("227"))
                                        {
                                            response = "";
                                        }
                                    }
                                    task2watch.Stop();
                                    Console.WriteLine($"Response length: {response.Length} with time : {task2watch.ElapsedMilliseconds / 1000.00 }s");
                                    //Thread.Sleep(2000);
                                    if (ModemName == "EC20" || ModemName == "EC20F")
                                    {
                                        Port.WriteLine("AT+QIDEACT=1");

                                        Thread.Sleep(1000);
                                    }
                                    LogResponseCommand(response);
                                    break;
                                }
                            case GSMCommand.GET_PHONE_NUMBER:
                                {
                                    switch (SIMCarrier)
                                    {
                                        case SIMCarrier.NO_SIM_CARD: { break; }
                                        case SIMCarrier.VietnamMobile:
                                            {
                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);
                                                }
                                                else if (ModemName != "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }


                                                //Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                ////Thread.Sleep(5000);
                                                ////response = Port.ReadExisting();
                                                ////response = WaitResultOrTimeout("CUSD", 15000);
                                                //response = Port.ReadStringUpToEndChars("\", 15", 15000);
                                                //Port.Write("" + (char)26);
                                                LogResponseCommand(response);
                                                //Port.Write("AT+CUSD=2\r\n");
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.Vinaphone:
                                            {
                                                if (ModemName == "MC55")
                                                {


                                                    Port.WriteLine("ATDT*111#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.Write("" + (char)27);
                                                    Port.Write("" + (char)27);
                                                    Thread.Sleep(100);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*111#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);

                                                    Port.WriteLine("AT+CUSD=1,\"*111#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);
                                                }
                                                else
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);


                                                    Port.WriteLine("AT+CUSD=1,\"*111#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);
                                                    if (response.Length < 10)
                                                    {
                                                        //itel *101#
                                                        Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                        //response = WaitResultOrTimeout("CUSD", 20000);]
                                                        response = Port.ReadStringUpToEndChars("\",15", 10000);
                                                        Port.Write("" + (char)27);

                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("", 1000);
                                                    }
                                                }

                                                LogResponseCommand(response);
                                                break;
                                            }
                                        case SIMCarrier.Viettel:
                                            {
                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 3000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 3000);
                                                    //+CUSD: 0,"Yeu cau cua Quy khach khong duoc dap ung tai thoi diem nay",15
                                                    if (response.Contains("Yeu cau cua Quy khach khong duoc dap ung"))
                                                    {
                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 3000);
                                                        Port.Write("" + (char)27);

                                                        Port.WriteLine("ATDT*098#;");
                                                        response = Port.ReadStringUpToEndChars(",15", 10000);

                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 3000);
                                                        isPrepaid = false;
                                                    }

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);]
                                                    response = Port.ReadStringUpToEndChars("\",15", 10000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("", 1000);

                                                    if (response.Contains("Yeu cau cua Quy khach khong duoc dap ung"))
                                                    {
                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 1000);

                                                        Port.WriteLine("AT+CUSD=1,\"*098#\",15");
                                                        //response = WaitResultOrTimeout("CUSD", 20000);
                                                        response = Port.ReadStringUpToEndChars(",15", 10000);
                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 1000);
                                                        isPrepaid = false;

                                                    }

                                                }
                                                else if (ModemName != "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    if (response.Contains("Yeu cau cua Quy khach khong duoc dap ung"))
                                                    {
                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 1000);

                                                        Port.WriteLine("AT+CUSD=1,\"*098#\",15");
                                                        //response = WaitResultOrTimeout("CUSD", 20000);
                                                        response = Port.ReadStringUpToEndChars(",15", 10000);
                                                        Port.WriteLine("AT+CUSD=2");
                                                        WaitResultOrTimeout("CUSD", 1000);
                                                        isPrepaid = false;

                                                    }
                                                }


                                                //Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 20000);
                                                ////LogResponseCommand(response);
                                                //Port.Write("AT+CUSD=2\r");
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                LogResponseCommand(response);
                                                break;
                                            }
                                        case SIMCarrier.Mobifone:
                                            {

                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*0#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*0#\",15");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=1,\"*0#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*0#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }


                                                //Port.Write("AT+CUSD=1,\"*0#\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 15000);
                                                ////Thread.Sleep(5000);
                                                ////response = Port.ReadExisting();
                                                LogResponseCommand(response);
                                                break;
                                            }

                                        case SIMCarrier.DTAC:
                                            {
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                Port.WriteLine("AT+CUSD=1,\"*102*9#\",15");
                                                //response = WaitResultOrTimeout("CUSD", 20000);
                                                response = Port.ReadStringUpToEndChars(",15", 10000);
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);

                                                LogResponseCommand(response);
                                                break;
                                            }
                                        case SIMCarrier.Metfone:
                                            {
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                Port.WriteLine("AT+CUSD=1,\"*99#\",15");
                                                response = Port.ReadStringUpToEndChars(",15", 10000);
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                LogResponseCommand(response);
                                                break;    
                                            }
                                        default: { break; }
                                    }
                                    break;
                                }
                            case GSMCommand.CHECKBALANCE:
                                {
                                    switch (SIMCarrier)
                                    {
                                        case SIMCarrier.Vinaphone:
                                            {
                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }

                                                break;

                                                //Port.Write("AT+CUSD=2\r");
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                //Port.Write("AT+CUSD=1,\"*111#\",15\r");
                                                //WaitResultOrTimeout("CUSD", 20000);
                                                //Port.Write("AT+CUSD=1,\"1\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 15000);
                                                //Port.Write("AT+CUSD=2\r");
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                //break;
                                            }
                                        case SIMCarrier.Mobifone:
                                            {

                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }

                                                //Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 15000);
                                                ////LogResponseCommand(response);
                                                //Port.Write("AT+CUSD=2\r");
                                                //WaitResultOrTimeout("CUSD", 3000);


                                                break;
                                            }
                                        case SIMCarrier.Viettel:
                                            {

                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                MessageBox.Show(response);
                                                //Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 15000);
                                                //LogResponseCommand(response);
                                                //Port.Write("AT+CUSD=2\r");
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }
                                        case SIMCarrier.VietnamMobile:
                                            {

                                                if (ModemName == "MC55")
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                    Port.Write("" + (char)27);

                                                    Port.WriteLine("ATDT*101#;");
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);

                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);


                                                }
                                                else if (ModemName.Contains("SIMCOM_SIM5320E"))
                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }
                                                else

                                                {
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);

                                                    Port.WriteLine("AT+CUSD=1,\"*101#\",15");
                                                    //response = WaitResultOrTimeout("CUSD", 20000);
                                                    response = Port.ReadStringUpToEndChars(",15", 10000);
                                                    Port.WriteLine("AT+CUSD=2");
                                                    WaitResultOrTimeout("CUSD", 1000);
                                                }

                                                //Port.Write("AT+CUSD=1,\"*101#\",15\r");
                                                //response = WaitResultOrTimeout("CUSD", 15000);
                                                ////LogResponseCommand(response);
                                                //WaitResultOrTimeout("CUSD", 3000);
                                                break;
                                            }

                                        case SIMCarrier.DTAC:
                                            {

                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);

                                                Port.WriteLine("AT+CUSD=1,\"*101*9#\",15");
                                                //response = WaitResultOrTimeout("CUSD", 20000);
                                                response = Port.ReadStringUpToEndChars(",15", 10000);
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                LogResponseCommand(response);
                                                break;
                                            }
                                        case SIMCarrier.Metfone:
                                            {
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                Port.WriteLine("AT+CUSD=1,\"*097#\",15");
                                                response = Port.ReadStringUpToEndChars(",15", 10000);
                                                Port.WriteLine("AT+CUSD=2");
                                                WaitResultOrTimeout("CUSD", 1000);
                                                LogResponseCommand(response);
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case GSMCommand.RESET_INBOX:
                                {
                                    //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    Port.WriteLine("AT+CMGD=1,4\r");
                                    Thread.Sleep(1000);
                                    LogResponseCommand(Port.ReadExisting());
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    //Port.WriteLine("AT+CMGD=1,3\r");
                                    //Thread.Sleep(1000);
                                    //GlobalEvent.ONATCommandResponse(Port.ReadExisting());

                                    response = "RESETED";
                                    break;
                                }
                            case GSMCommand.GET_NEW_MSG:
                                {
                                    if (IsSIMConnected)
                                    {


                                        //response = string.Empty;
                                        //Port.WriteLine("AT+CPMS=\"ME\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        //response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        //Port.WriteLine("AT+CMGD=1,3\r");
                                        //WaitResultOrTimeout("CMGD", 500);

                                        //Port.WriteLine("AT+CPMS=\"SM\"");
                                        //WaitResultOrTimeout("CPMS", 500);


                                        // Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        // response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        // Port.Write("AT+CMGF=1\r");

                                        Port.WriteLine("AT+CMGF=1");
                                        WaitResultOrTimeout("OK", 1000);


                                        if (ModemName != "MC55")
                                        {
                                            Port.WriteLine("AT+CMGF=1");
                                            WaitResultOrTimeout("OK", 1000);


                                            if (ModemName.Contains("SIMCOM") || ModemName.Contains("EC20F"))
                                            {

                                                Port.WriteLine("AT+CPMS=\"SM\"");
                                                WaitResultOrTimeout("CPMS", 1000);
                                                response = string.Empty;
                                                Port.WriteLine("AT+CMGL=\"ALL\"");
                                                Thread.Sleep(1000);
                                            }
                                            else 
                                            {

                                                Port.WriteLine("AT+CPMS=\"MT\"");
                                                WaitResultOrTimeout("CPMS", 1000, true);

                                                response = string.Empty;
                                                Port.WriteLine("AT+CMGL=\"ALL\"");

                                            }
                                            response += WaitSlowResultOrTimeout("+CMGL", 15000);
                                            if (string.IsNullOrEmpty(response))
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                if (response.Contains("+CMGL"))
                                                {
                                                    //if no contain => no remove
                                                    //Port.WriteLine("AT+CMGD=1,3");
                                                    //WaitResultOrTimeout("OK", 3000);
                                                    Port.WriteLine("AT+CMGD=0,3");
                                                    WaitResultOrTimeout("OK", 3000);
                                                }

                                            }
                                        }
                                        else if (ModemName == "MC55")
                                        {
                                            //change to pdu mode
                                            Port.WriteLine("AT+CPMS=\"MT\"");
                                            WaitResultOrTimeout("CPMS", 1000);


                                            Port.WriteLine("AT+CMGF=0");
                                            WaitResultOrTimeout("", 1000);

                                            Port.WriteLine("AT+CMGL=4");
                                            response = WaitSlowResultOrTimeout("+CMGL", 3000);
                                            Console.WriteLine(response);
                                        }
                                        //Port.WriteLine("AT+CPMS=\"MT\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        //Port.Write("AT+CMGL=\"REC UNREAD\"\r");
                                        //response += "" + WaitResultOrTimeout("CMGL", 1000);
                                        //Port.WriteLine("AT+CMGD=1,3\r");
                                        //WaitResultOrTimeout("CMGD", 500);

                                        //Port.WriteLine("AT+CPMS=\"SM\"");
                                        //WaitResultOrTimeout("CPMS", 500);
                                        break;
                                    }
                                    return string.Empty;
                                }
                            case GSMCommand.DIAL:
                                {
                                    try
                                    {
                                        if (IsSIMConnected)
                                        {
                                            if (ModemName != "UC20" && ModemName != "EC20F")
                                            {
                                                Port.WriteLine("AT+COLP=1");
                                                response = WaitResultOrTimeout("OK", 1000);
                                            }
                                            else
                                            {
                                                Port.WriteLine("AT+COLP=0");
                                                response = WaitResultOrTimeout("OK", 1000);
                                            }

                                            IsCalling = true;
                                            DialInfo dialInfo = (DialInfo)data;
                                            Port.WriteLine("ATH");
                                            Thread.Sleep(100);
                                            LogResponseCommand(Port.ReadExisting());
                                            Port.WriteLine("ATD" + dialInfo.DialNo + ";");
                                            Thread.Sleep(1000);
                                            //response = Port.ReadExisting();
                                            //LogResponseCommand(response);
                                            Console.WriteLine($"[Dialing] {PortName} => {dialInfo.DialNo} ");

                                            logger.Info($"[+] Dial {PortName} => {dialInfo.DialNo} ");

                                            if (ModemName == "EC20F")
                                            {
                                                int counter_ec20f = 0;
                                            loop_connect_ec20f:
                                                Port.WriteLine("AT+CLCC");
                                                Thread.Sleep(1000);
                                                response = Port.ReadExisting();
                                                //AT+CLCC 1,0,0,0,0 -> nghe 1,0,3,0,0 dang ket noi
                                                //+CLCC: 2,0,2,0,0,"0846889911",129 dang chuan bi
                                                //+CLCC: 2,0,3,0,0,"0846889911",129 dang ket noi ring
                                                //+CLCC: 2,0,0,0,0,"0846889911",129 dang nghe

                                                if (response.Contains("0,0,0,0,"))
                                                {
                                                    int timeDelayTATH = (dialInfo.DurationLimit > 3)
                                                        ? dialInfo.DurationLimit - 2
                                                        : dialInfo.DurationLimit;

                                                    Thread.Sleep(timeDelayTATH * 1000);
                                                    Port.WriteLine("ATH");
                                                    Thread.Sleep(100);
                                                    LogResponseCommand(Port.ReadExisting());
                                                    response = "CALL_OK";
                                                    logger.Info($"[+] Contain CPAS:4 {response}");
                                                }
                                                else
                                                {
                                                    if (response.Contains("BUSY"))
                                                    {
                                                        response = "BUSY";
                                                        logger.Info($"[+] Contain BUSY {response}");
                                                        return response;
                                                    }
                                                    else if (response.Contains("0,3,0,0,") || response.Contains("0,2,0,0,"))
                                                    {
                                                        Thread.Sleep(1000);
                                                        goto loop_connect_ec20f;
                                                    }
                                                    counter_ec20f++;
                                                    if (counter_ec20f > 10)
                                                    {
                                                        Port.WriteLine("ATH");
                                                        Thread.Sleep(100);
                                                        LogResponseCommand(Port.ReadExisting());

                                                        return "ERROR";
                                                    }
                                                    //logger.Info($"[+] Dial {PortName} => {dialInfo.DialNo} CLCC " + response);
                                                    goto loop_connect_ec20f;
                                                }


                                            }

                                            else if (ModemName == "WaveCom" || ModemName == "EC20F")
                                            {
                                                while (true)
                                                {

                                                    response += Port.ReadExisting();
                                                    logger.Info($"[Seq] => {response}");
                                                    if (response.Contains("BUSY") || response.Contains("NO CARRIER"))
                                                    {
                                                        response = "BUSY|NO CARRIER";
                                                        IsCalling = false;
                                                        break;
                                                    }
                                                    else if (response.Contains("OK"))
                                                    {
                                                        Thread.Sleep(dialInfo.DurationLimit * 1000);

                                                        Port.WriteLine("ATH");
                                                        Thread.Sleep(100);
                                                        response = "CALL_OK";
                                                        IsCalling = false;
                                                        break;
                                                    }
                                                    //+WIND: 9
                                                    //OK => connecte
                                                    //BUSY  => ban
                                                    //NO CARRIER
                                                    Thread.Sleep(100);
                                                }
                                            }
                                            else if (ModemName == "MC55")
                                            {
                                                if (string.IsNullOrEmpty(response))
                                                {
                                                    Thread.Sleep(dialInfo.DurationLimit * 1000);

                                                    Port.WriteLine("ATH");
                                                    Thread.Sleep(100);
                                                    LogResponseCommand(Port.ReadExisting());
                                                    response = "CALL_OK";
                                                    IsCalling = false;
                                                }
                                            }
                                            else if (ModemName == SIMCOM_SIM5320E)
                                            {
                                                response += WaitResultOrTimeout("COLP", 60 * 1000, true);
                                                //Thread.Sleep(1500);
                                                //response += Port.ReadExisting();

                                                if (response.Contains("BEGIN"))
                                                {
                                                    Thread.Sleep(dialInfo.DurationLimit * 1000);
                                                    Port.WriteLine("AT+CVHU=0");
                                                    Thread.Sleep(100);
                                                    Port.WriteLine("AT+CVHU=0");
                                                    Thread.Sleep(100);
                                                    Port.WriteLine("ATH");
                                                    Thread.Sleep(100);
                                                    LogResponseCommand(Port.ReadExisting());
                                                    response = "CALL_OK";
                                                    IsCalling = false;
                                                }
                                                else if (response.Contains("VOICE CALL: END: 000000"))
                                                {
                                                    response = "BUSY";
                                                    IsCalling = false;
                                                }
                                                else
                                                {
                                                    response = "ERROR";
                                                    IsCalling = false;
                                                }
                                            }
                                            else if (ModemName == "UC20")
                                            {
                                                int counter_uc20 = 0;
                                            loop_connect_uc20:
                                                Port.WriteLine("AT+CLCC");
                                                Thread.Sleep(1000);
                                                response = Port.ReadExisting();
                                                //AT+CLCC 1,0,0,0,0 -> nghe 1,0,3,0,0 dang ket noi
                                                if (response.Contains("1,0,0,0,0"))
                                                {
                                                    Thread.Sleep(dialInfo.DurationLimit * 1000);
                                                    Port.WriteLine("ATH");
                                                    Thread.Sleep(100);
                                                    LogResponseCommand(Port.ReadExisting());
                                                    response = "CALL_OK";
                                                    logger.Info($"[+] Contain CPAS:4 {response}");
                                                }
                                                else
                                                {
                                                    if (response.Contains("BUSY"))
                                                    {
                                                        response = "BUSY";
                                                        logger.Info($"[+] Contain BUSY {response}");
                                                        return response;
                                                    }
                                                    else if (response.Contains("1,0,3,0,0"))
                                                    {
                                                        Thread.Sleep(1000);
                                                        goto loop_connect_uc20;
                                                    }
                                                    counter_uc20++;
                                                    if (counter_uc20 > 10)
                                                    {
                                                        Port.WriteLine("ATH");
                                                        Thread.Sleep(100);
                                                        LogResponseCommand(Port.ReadExisting());

                                                        return "ERROR";
                                                    }
                                                    //logger.Info($"[+] Dial {PortName} => {dialInfo.DialNo} CLCC " + response);
                                                    goto loop_connect_uc20;
                                                }



                                            }
                                            else if (ModemName != "MC55")
                                            {
                                            loop:
                                                Port.WriteLine("AT+CPAS");
                                                Thread.Sleep(100);
                                                response = Port.ReadExisting();
                                                if (response.Contains("CPAS: 4"))
                                                {
                                                    Thread.Sleep(dialInfo.DurationLimit * 1000);
                                                    Port.WriteLine("ATH");
                                                    Thread.Sleep(100);
                                                    LogResponseCommand(Port.ReadExisting());
                                                    response = "CALL_OK";
                                                    logger.Info($"[+] Contain CPAS:4 {response}");

                                                }
                                                else
                                                {
                                                    if (response.Contains("BUSY"))
                                                    {
                                                        response = "BUSY";
                                                        logger.Info($"[+] Contain BUSY {response}");

                                                    }
                                                    else if (response.Contains("CPAS: 3"))
                                                    {
                                                        goto loop;
                                                    }
                                                }
                                            }
                                            IsCalling = false;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        IsCalling = false;
                                        Console.WriteLine($"Exception at Dial {PortName} => {ex.Message}");

                                    }
                                    finally
                                    {
                                        IsCalling = false;
                                        Port.WriteLine("ATH");
                                        Thread.Sleep(100);

                                    }
                                    break;
                                }
                            case GSMCommand.DROPCALL:
                                {
                                    if (IsSIMConnected)
                                    {
                                        Port.WriteLine("ATH");
                                        Thread.Sleep(100);
                                        response = Port.ReadExisting();
                                        LogResponseCommand(response);
                                    }
                                    break;
                                }

                            case GSMCommand.ANSWER_CALL:
                                {
                                    this.Log = true;
                                    if (RingStatus == RingStatus.Ringing)
                                    {
                                        try
                                        {
                                            if (AppModeSetting.AutoRecordCall)
                                            {

                                                if (AppModeSetting.AutoAnswerCall)
                                                {
                                                    RIFFTimeline = new Dictionary<byte[], string>();

                                                    Port.WriteLine("AT+QFDEL=\"RAM:*\"");
                                                    WaitResultOrTimeout("", 500);
                                                    Port.WriteLine("AT+QAUDRD=0;\r");
                                                    //Port.WriteLine("AT+QFDEL=\"RAM:*\"");
                                                    Thread.Sleep(1000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);


                                                    Port.WriteLine("AT+CLIP=1");
                                                    Thread.Sleep(1000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    Thread.Sleep(2000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    string callerID = Regex.Match(response, "(CLIP: \"(.*?)\")").Value
                                                        .Replace("CLIP: \"", string.Empty).Replace("\"", string.Empty);
                                                    this.MyProcessMessage = "Ring... => " + callerID;
                                                    DateTime answerDate = DateTime.Now;
                                                    Port.WriteLine("ATA");
                                                    Thread.Sleep(100);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    RingStatus = RingStatus.Idle;
                                                    RingStatusChanged(this, RingStatus);

                                                    DateTime startTime = DateTime.Now;




                                                    //if (AppModeSetting.AutoPlayAnswerAudio == true && AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT_RECORD))
                                                    //{

                                                    //    //Port.StartLoggerWrite("playAudio", true);
                                                    //    playAudio("send.amr", 3);
                                                    //    //Port.EndLoggerWrite();
                                                    //}

                                                    //                                                startTime = DateTime.Now;


                                                    //while (!Stop && (DateTime.Now - startTime).TotalSeconds < 10 && IsPortConnected && IsSIMConnected)
                                                    //{
                                                    //    //recording
                                                    //    response = Port.ReadExisting();
                                                    //    LogResponseCommand(response);
                                                    //    if (response.Contains("QAUDRIND") || response.Contains("NO CARRIER"))
                                                    //    {
                                                    //        break;
                                                    //    }
                                                    //}


                                                    //Port.WriteLine("AT+QPSND=0");
                                                    //Thread.Sleep(100);




                                                    Port.WriteLine("AT+QAUDRD=1,\"RAM:c.amr\",3");
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);


                                                    while (!Stop &&
                                                           (DateTime.Now - startTime).TotalSeconds <
                                                           AppModeSetting.TimeToEndAnswer && IsPortConnected &&
                                                           IsSIMConnected)
                                                    {
                                                        //recording
                                                        response = Port.ReadExisting();
                                                        LogResponseCommand(response);
                                                        if (response.Contains("QAUDRIND") ||
                                                            response.Contains("NO CARRIER"))
                                                        {
                                                            break;
                                                        }
                                                    }

                                                    Port.WriteLine("AT+QAUDRD=0;\r");
                                                    Thread.Sleep(500);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    Port.WriteLine("ATH");
                                                    response = WaitResultOrTimeout("", 1000);

                                                    Port.WriteLine("AT+QFDWL=\"RAM:c.amr\";");
                                                    Thread.Sleep(1000);
                                                read_func:
                                                    //Thread.Sleep(200);
                                                    string audioString = "";


                                                    byte[] buffer = Port.ReadBytes(5000,8000);
                                                    int bytesRead = buffer.Length;
                                                    if (bytesRead == 0)
                                                    {
                                                        break;
                                                    }
                                                    //int bytesRead = 0;
                                                    //try
                                                    //{
                                                    //    bytesRead = Port.Read(buffer, 0, buffer.Length);
                                                    //    if (bytesRead == 0)
                                                    //        break;
                                                    //}
                                                    //catch
                                                    //{
                                                    //}
                                                    //using (var stream = System.IO.File.Open("C:\\Users\\snowd\\Desktop\\test.amr", System.IO.FileMode.Append))
                                                    //{
                                                    //    stream.Write(buffer, 0 ,bytesRead);
                                                    //}

                                                    audioString = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                                    MessageBox.Show(audioString);
                                                    string pattern_connect = "\r\nCONNECT\r\n";
                                                    int ix = audioString.IndexOf(pattern_connect);
                                                    if (ix != -1)
                                                    {
                                                        audioString =
                                                            audioString.Substring(ix + pattern_connect.Length);
                                                        byte[] newArray = new byte[buffer.Length - 11];
                                                        Buffer.BlockCopy(buffer, 11, newArray, 0, newArray.Length);
                                                        buffer = newArray;
                                                        // do something here
                                                    }


                                                    if (!string.IsNullOrEmpty(audioString) &&
                                                        !audioString.Contains("+QFLDS") &&
                                                        !audioString.Contains("+CPMS") && !audioString.Contains("CMT")
                                                        && !audioString.Contains("ERROR"))
                                                    {
                                                        if (audioString.Contains("RIFF"))
                                                        {
                                                            //Restart record
                                                            RIFFTimeline.Clear();
                                                            RIFFTimeline.Add(
                                                                buffer.Skip(38).Take(bytesRead - 38).ToArray(),
                                                                string.Empty);
                                                            goto read_func;
                                                        }
                                                        else
                                                        {
                                                            if (audioString.Contains("+QFDWL"))
                                                            {
                                                                RIFFTimeline.Add(buffer.Take(bytesRead - 29).ToArray(),
                                                                    string.Empty);
                                                                List<byte[]> recordValue = new List<byte[]>();
                                                                foreach (var item in RIFFTimeline)
                                                                    recordValue.Add(item.Key);
                                                                byte[] record = recordValue.SelectMany(s => s)
                                                                    .ToArray();
                                                                MessageBox.Show(record.Length.ToString());
                                                                // voiceContent = new C3TekPortal().VoiceRecognitionToText(record);

                                                                //var temp = Path.GetTempFileName();
                                                                //File.WriteAllBytes(temp, record);
                                                                //string voiceContent = new SpeechToTextHelp
                                                                //er().SpeechToText(temp);


                                                                //lock (GSMControlCenter.LockGSMMessages)
                                                                //{
                                                                //    GSMControlCenter.GSMMessages.Add(message);
                                                                //    GSMControlCenter.OnNewMessage(message);
                                                                //    NotifyAlert();
                                                                //}
                                                                string file_name_amr =
                                                                    $"{PhoneNumber}__{((Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString()}_c.amr";
                                                                this.MyProcessMessage = "Cuộc gọi đến đã ghi âm > " +
                                                                    file_name_amr;

                                                                if (!Directory.Exists(System.IO.Path.GetDirectoryName(
                                                                        Assembly.GetEntryAssembly()?.Location) +
                                                                    "/voice_data"))
                                                                {
                                                                    Directory.CreateDirectory(Path.GetDirectoryName(
                                                                            Assembly.GetEntryAssembly()?.Location) +
                                                                        "/voice_data");
                                                                }

                                                                System.IO.File.WriteAllBytes(
                                                                    System.IO.Path.GetDirectoryName(
                                                                        Assembly.GetEntryAssembly()?.Location) +
                                                                    "/voice_data/" + file_name_amr, record);
                                                                int Subchunk1Size = BitConverter.ToInt32(record, 0);
                                                                break;
                                                            }

                                                            RIFFTimeline.Add(buffer.Take(bytesRead).ToArray(),
                                                                string.Empty);
                                                           
                                                            goto read_func;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (GlobalVar.AutoAnswerIncomingCall)
                                                    {
                                                        Port.WriteLine("ATA");
                                                        Thread.Sleep(1000);
                                                        response = Port.ReadExisting();
                                                        LogResponseCommand(response);
                                                        RingStatus = RingStatus.Idle;
                                                        RingStatusChanged(this, RingStatus);


                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //No AutoRecord
                                                if (AppModeSetting.AutoAnswerCall)
                                                {
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);


                                                    Port.WriteLine("AT+CLIP=1");
                                                    Thread.Sleep(1000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    Thread.Sleep(2000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);

                                                    string callerID = Regex.Match(response, "(CLIP: \"(.*?)\")").Value
                                                        .Replace("CLIP: \"", string.Empty).Replace("\"", string.Empty);
                                                    this.MyProcessMessage = "Ring... => " + callerID;




                                                    Port.WriteLine("ATA");

                                                    Thread.Sleep(100);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    RingStatus = RingStatus.Idle;
                                                    RingStatusChanged(this, RingStatus);

                                                    DateTime startTime = DateTime.Now;

                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);


                                                    while (!Stop &&
                                                           (DateTime.Now - startTime).TotalSeconds <
                                                           AppModeSetting.TimeToEndAnswer && IsPortConnected &&
                                                           IsSIMConnected)
                                                    {
                                                        //recording
                                                        response = Port.ReadExisting();
                                                        LogResponseCommand(response);
                                                        if (response.Contains("QAUDRIND") ||
                                                            response.Contains("NO CARRIER"))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    Port.WriteLine("ATH");
                                                    response = WaitResultOrTimeout("OK", 1000);


                                                }
                                                else
                                                {
                                                    Port.WriteLine("ATA");
                                                    Thread.Sleep(1000);
                                                    response = Port.ReadExisting();
                                                    LogResponseCommand(response);
                                                    RingStatus = RingStatus.Idle;
                                                    RingStatusChanged(this, RingStatus);
                                                }
                                            }

                                        }
                                        catch (Exception ex)
                                        { 
                                            Console.WriteLine("Exception when answer with no recording", ex.Message);
                                        }
                                        finally
                                        {
                                            Port.WriteLine("ATH");
                                            WaitResultOrTimeout("", 500);
                                        }

                                    }
                                    break;
                                }
                            case GSMCommand.CHECKRING:
                                {
                                    if (IsSIMConnected)
                                    {
                                        response = Port.ReadExisting();
                                        LogResponseCommand(response);

                                    }
                                    break;
                                }
                            case GSMCommand.GETCALLERID:
                                {
                                    Port.WriteLine("AT+CLIP=1");
                                    Thread.Sleep(1000);
                                    response = Port.ReadExisting();
                                    LogResponseCommand(response);

                                    break;
                                }
                            case GSMCommand.SEND_MESSAGE:
                                {
                                    if (IsSIMConnected)
                                    {
                                        SendMessageData _dataParam = (SendMessageData)data;

                                        string num = _dataParam.Receiver;
                                        //string pduString = createFlash(num, _dataParam.Content);

                                        bool unicode = HasNonASCIIChars(_dataParam.Content);
                                        if (_dataParam.Content.Contains("_") || _dataParam.Content.Contains("?"))
                                        {
                                            unicode = true;
                                        }

                                        if (AppModeSetting.Locale.Equals("vi-VN"))
                                        {
                                            if (num.Length == 9)
                                                num = "0" + num;
                                            if (num.StartsWith("0"))
                                                num = "84" + num.Remove(0, 1);
                                            if (num.StartsWith("840"))
                                                num = "+84" + num.Remove(0, 3);
                                            if (!num.StartsWith("840"))
                                                num = "+" + num;
                                        }
                                        //if (ModemName == "WaveCom")
                                        string result_msg = "";
                                        Port.WriteLine("AT+CMGF=1");
                                        response = WaitResultOrTimeout("", 300);
                                        //Port.WriteLine("AT+CNMI=2,2,0,1,0");
                                        //response = WaitResultOrTimeout("", 300);
                                        //Port.WriteLine("AT+CSMP=17,167,0,0");
                                        unicode = CheckIsUTF8(_dataParam.Content);
                                        if (unicode || _dataParam.Content.Contains("_"))
                                        {

                                            //pdu
                                            response = WaitResultOrTimeout("", 300);
                                            Port.WriteLine("AT+CMGF=0");
                                            response = WaitResultOrTimeout("", 300);
                                            Port.WriteLine("AT+CMMS=2");
                                            response = WaitResultOrTimeout("", 300);

                                            SmsSubmitPdu[] pdu = SmartMessageFactory.CreateConcatTextMessage(_dataParam.Content, unicode, num);
                                            foreach (SmsSubmitPdu pduItem in pdu)
                                            {
                                                Port.WriteLine("AT+CMGS=" + pduItem.ActualLength);
                                                response = WaitResultOrTimeout("", 500);
                                                Port.Write(pduItem.ToString() + char.ConvertFromUtf32(26));
                                                result_msg = WaitResultOrTimeout("", 5000);
                                            }
                                            Port.Write("AT+CMGF=1\r\n");
                                            response = WaitResultOrTimeout("", 100);
                                            Port.Write("AT+CSCS=\"GSM\"\r");
                                            response = WaitResultOrTimeout("", 500);
                                        }
                                        else
                                        {
                                            Port.Write("AT+CMGF=1\r\n");
                                            response = WaitResultOrTimeout("", 100);
                                            Port.Write("AT+CSCS=\"GSM\"\r");
                                            response = WaitResultOrTimeout("", 500);

                                            Port.WriteLine($"AT+CMGS=\"{num}\"");
                                            response = WaitResultOrTimeout("", 500);
                                            Port.Write(_dataParam.Content.Trim() + char.ConvertFromUtf32(26));
                                            result_msg = WaitResultErrorOrTimeout("", 5000);
                                        }


                                        if (result_msg.Contains("ERROR") || result_msg.Contains("CME ERR"))
                                        {
                                            response = "ERROR";
                                        }
                                        else if (result_msg.Contains("OK"))
                                        {
                                            response = "OK";
                                        }
                                        else
                                        {
                                            response = "ERROR";
                                        }

                                        //Port.Write("AT+CMGF=1\r\n");
                                        //WaitResultOrTimeout("", 100);
                                        //Port.Write("AT+CSCS=\"GSM\"\r");
                                        //response = WaitResultOrTimeout("", 500);

                                        //if (unicode)
                                        //{
                                        //    //USE PDU mode to send SMS and wait re
                                        //    Port.Write("AT+CMGF=0\r");
                                        //    response = WaitResultOrTimeout("", 300);
                                        //    Port.Write("AT+CMMS=2\r");
                                        //    WaitResultOrTimeout("", 300);

                                        //    //Port.Write("AT+CSCS=\"UCS2\"\r");
                                        //    response = WaitResultOrTimeout("", 500);
                                        //    ////enable status of sms
                                        //    //Port.Write("AT+CNMI=2,1,0,1,0\r");
                                        //    ////
                                        //    //WaitResultOrTimeout("", 500);
                                        //    //Port.Write("AT+CSMP=49,167,0,0\r");
                                        //    //WaitResultOrTimeout("", 500);


                                        //    SmsSubmitPdu[] pdu = SmartMessageFactory.CreateConcatTextMessage(_dataParam.Content, unicode, num);
                                        //    if(pdu.Length == 1)
                                        //    {
                                        //        SmsSubmitPdu newpdu = new SmsSubmitPdu(_dataParam.Content, _dataParam.Receiver, GsmComm.PduConverter.DataCodingScheme.NoClass_16Bit);
                                        //        Port.Write("AT+CMGS=" + newpdu.ActualLength + "\r");
                                        //        result_msg = WaitResultOrTimeout(">", 1000);
                                        //        Port.Write(newpdu.ToString());
                                        //        Port.Write("" + (char)26);

                                        //        //result_msg = WaitResultWithCommand("+CDS", 10000);

                                        //        //contains ok
                                        //        // string cds_response = WaitResultWithCommand("+CDS", 5000);//5s for status
                                        //        result_msg = WaitResultOrTimeout("", 5000);


                                        //        string chk_result_error = Port.ReadExisting();

                                        //        if (result_msg.Contains("ERROR"))
                                        //        {
                                        //            response = "ERROR";
                                        //        }
                                        //        else if (string.IsNullOrEmpty(result_msg))
                                        //        {
                                        //            response = "OK_NOT_STATUS";
                                        //        }
                                        //        else
                                        //        {
                                        //            response = "OK";
                                        //        }

                                        //    }
                                        //    else {  
                                        //        foreach (SmsSubmitPdu pduItem in pdu)
                                        //    {
                                        //        //pduItem.SetSmscAddress("+84920210015", AddressType.InternationalPhone);
                                        //        Port.Write("AT+CMGS=" + pduItem.ActualLength + "\r");
                                        //        result_msg = WaitResultOrTimeout(">", 1000);
                                        //        Port.Write(pduItem.ToString() + (char)26);

                                        //        //result_msg = WaitResultWithCommand("+CDS", 10000);

                                        //        //contains ok
                                        //        // string cds_response = WaitResultWithCommand("+CDS", 5000);//5s for status
                                        //        result_msg = WaitResultOrTimeout("", 5000);


                                        //        string chk_result_error = Port.ReadExisting();

                                        //        if (result_msg.Contains("ERROR"))
                                        //        {
                                        //            response = "ERROR";
                                        //        }
                                        //        else if (string.IsNullOrEmpty(result_msg))
                                        //        {
                                        //            response = "OK_NOT_STATUS";
                                        //        }
                                        //        else
                                        //        {
                                        //            response = "OK";
                                        //        }
                                        //        //string chk_result_error = Port.ReadExisting();
                                        //        //if (result_msg.Contains("ERROR") || chk_result_error.Contains("ERROR"))
                                        //        //{
                                        //        //    response = "ERROR";
                                        //        //    return response;
                                        //        //}
                                        //        //else
                                        //        //{
                                        //        //    response = "OK";
                                        //        //}
                                        //    }
                                        //    }
                                        //    //change to text mode
                                        //    Port.Write("AT+CMGF=1\r\n");
                                        //    WaitResultOrTimeout("", 100);
                                        //    Port.Write("AT+CSCS=\"GSM\"\r");
                                        //    response = WaitResultOrTimeout("", 500);
                                        //}
                                        //else
                                        //{
                                        //    //enable text mode
                                        //    Port.Write("AT+CMGF=1\r\n");
                                        //    WaitResultOrTimeout("", 1000);

                                        //    //enable status of sms
                                        //    Port.Write("AT+CNMI=2,1,0,1,0\r");
                                        //    WaitResultOrTimeout("", 500);
                                        //    Port.Write("AT+CSMP=49,167,0,0\r");
                                        //    WaitResultOrTimeout("", 500);


                                        //    Port.Write("AT+CMGS=\"" + _dataParam.Receiver + "\"\r");
                                        //    Thread.Sleep(100);

                                        //    Port.Write(_dataParam.Content + "\x1A");

                                        //    result_msg = WaitResultWithCommand("+CDS", 6000);

                                        //    //contains ok
                                        //    // string cds_response = WaitResultWithCommand("+CDS", 5000);//5s for status


                                        //    //string chk_result_error = Port.ReadExisting();

                                        //    if (result_msg.Contains("ERROR"))
                                        //    {
                                        //        response = "ERROR";
                                        //    }
                                        //    else if (string.IsNullOrEmpty(result_msg))
                                        //    {
                                        //        response = "OK_NOT_STATUS";
                                        //    }
                                        //    else
                                        //    {
                                        //        response = "OK";
                                        //    }

                                        //    //string result_msg = WaitResultOrTimeout("CMGS", 10000);
                                        //    //Port.WriteLine("AT+CMGF=1");
                                        //    //Thread.Sleep(100);

                                        //    //Port.WriteLine("AT+CSCS=\"GSM\"");
                                        //    //Thread.Sleep(1000);

                                        //    ////SendMessageData _dataParam = (SendMessageData)data;
                                        //    ////if (_dataParam.Receiver.StartsWith("0"))
                                        //    ////{
                                        //    ////    _dataParam.Receiver = _dataParam.Receiver.Remove(0, 1).Insert(0, "84");
                                        //    ////}

                                        //    //string message = "AT+CMGS=\"" + ((SendMessageData)data).Receiver + "\""
                                        //    // + "\n" + ((SendMessageData)data).Content + "";

                                        //    //string result_msg = WaitResultOrTimeout("CMGS", 10000);
                                        //    //LogResponseCommand(result_msg);
                                        //}

                                    }
                                    break;
                                }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogResponseCommand(ex.Message);
                }
                RingDetector(response);
                return response;
            }
        }
        public static string GSMChar(string PlainText)
        {
            // ` is not a conversion, just a untranslatable letter
            string strGSMTable = "";
            strGSMTable += "@£$¥èéùìòÇ`Øø`Åå";
            strGSMTable += "Δ_ΦΓΛΩΠΨΣΘΞ`ÆæßÉ";
            strGSMTable += " !\"#¤%&'()*=,-./";
            strGSMTable += "0123456789:;<=>?";
            strGSMTable += "¡ABCDEFGHIJKLMNO";
            strGSMTable += "PQRSTUVWXYZÄÖÑÜ`";
            strGSMTable += "¿abcdefghijklmno";
            strGSMTable += "pqrstuvwxyzäöñüà";

            string strExtendedTable = "";
            strExtendedTable += "````````````````";
            strExtendedTable += "````^```````````";
            strExtendedTable += "````````{}`````\\";
            strExtendedTable += "````````````[~]`";
            strExtendedTable += "|```````````````";
            strExtendedTable += "````````````````";
            strExtendedTable += "`````€``````````";
            strExtendedTable += "````````````````";

            string strGSMOutput = "";
            foreach (char cPlainText in PlainText.ToCharArray())
            {
                int intGSMTable = strGSMTable.IndexOf(cPlainText);
                if (intGSMTable != -1)
                {
                    strGSMOutput += intGSMTable.ToString("X2");
                    continue;
                }
                int intExtendedTable = strExtendedTable.IndexOf(cPlainText);
                if (intExtendedTable != -1)
                {
                    strGSMOutput += (27).ToString("X2");
                    strGSMOutput += intExtendedTable.ToString("X2");
                }
            }
            return strGSMOutput;
        }
        private string pduMobileTD3(string mobile)
        {
            if (mobile.Length == 3)
            {
                mobile += "F";

            };
            if (mobile.Length == 4)
            {
                char[] arrayM = mobile.ToCharArray();
                return Convert.ToString(arrayM[1]) + Convert.ToString(arrayM[0]) +
                     Convert.ToString(arrayM[3]) + Convert.ToString(arrayM[2]);
            }
            char[] array = mobile.ToCharArray();
            return Convert.ToString(array[1]) + Convert.ToString(array[0]) +
                Convert.ToString(array[3]) + Convert.ToString(array[2]) +
                Convert.ToString(array[5]) + Convert.ToString(array[4])
                + Convert.ToString(array[7]) + Convert.ToString(array[6])
                + Convert.ToString(array[9]) + Convert.ToString(array[8]);
        }
        public string createFlash(string mobile, string sms)
        {
            string retval = string.Empty;
            retval += "001100";
            retval += "0" + mobile.Length.ToString("X");
            //if (mobile.StartsWith("84"))
            //{
            retval += "91";
            //}
            //else
            //{
            //    retval += "92";
            //}
            //MessageBox.Show(""+ pduMobileTD3(mobile));
            if (mobile.Trim().Length > 10)
            {
                retval += pduMobile(mobile.Trim());
            }
            else
            {
                retval += pduMobileTD3(mobile.Trim());
            }
            //else if (mobile.Trim() == "900")
            //{
            //    retval += "09F0";
            //}
            //else if (mobile.Trim() == "170")
            //{
            //    retval += "71F0";
            //}
            //else if (mobile.Trim() == "109")
            //{
            //    retval += "01F9";
            //}
            retval += "0000AA";
            if ((sms.Length) < 16) retval += "0";
            //int octet = 33 + sms.Length;
            retval += sms.Length.ToString("X");
            //retval += (sms.Length * 2).ToString("X");

            string a = GSMChar(sms);
            //string a = GSMConverter.StringToGSMHexString("12345678");
            byte[] unpackedBytes = PduBitPacker.ConvertHexToBytes(a);
            byte[] packedBytes = PduBitPacker.PackBytes(unpackedBytes);
            //byte[] unpackedBytes = PduBitPacker.UnpackBytes(packedBytes);

            retval += PduBitPacker.ConvertBytesToHex(packedBytes);

            //ASCIIEncoding.ASCII.GetString();

            return retval;
        }
        private string pduMobile(string mobile)
        {
            if (mobile.Length == 11) mobile += "F";
            char[] array = mobile.ToCharArray();
            return Convert.ToString(array[1]) + Convert.ToString(array[0]) +
                Convert.ToString(array[3]) + Convert.ToString(array[2]) +
                Convert.ToString(array[5]) + Convert.ToString(array[4]) +
                Convert.ToString(array[7]) + Convert.ToString(array[6]) +
                Convert.ToString(array[9]) + Convert.ToString(array[8]) +
                Convert.ToString(array[11]) + Convert.ToString(array[10]);
        }
        bool HasNonASCIIChars(string str)
        {
            return (System.Text.Encoding.UTF8.GetByteCount(str) != str.Length);
        }
        bool CheckIsUTF8(string input)
        {
            byte[] charactor = Encoding.ASCII.GetBytes(input);
            foreach (byte i in charactor)
            {
                if (i == 63)
                    return true;
            }

            return false;
        }
        private SafeSerialPort Port { get; set; }
        public List<GSMMessage> ParseNewMessageMC55()
        {
            List<GSMMessage> messages = new List<GSMMessage>();

            try
            {
                if (IsPortConnected && IsSIMConnected && !string.IsNullOrEmpty(PhoneNumber))
                {
                    try
                    {
                        ExecuteCommand(GSMCommand.SWITCH_TEXT_MODE);
                        string[] responsees = ExecuteCommand(GSMCommand.GET_NEW_MSG).Split(new char[] { '' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var res in responsees)
                        {
                            string response = res.Replace("\n", "");
                           
                            var match = Regex.Match(response, "(\\+CMGL: (.*)OK)");
                            if (match.Success && !string.IsNullOrEmpty(match.Value))
                            {
                                string[] msgs = match.Value.Replace("OK", "\r\nOK\r\n").Split(new string[] { "+CMGL" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var msg in msgs)
                                {
                                    try
                                    {
                                        string msgContent = msg.Substring(msg.IndexOf("\r") + 1, msg.Length - msg.IndexOf("\r") - 1);
                                        string msgIdx = new string(msg.SkipWhile(c => !char.IsDigit(c))
                                             .TakeWhile(c => char.IsDigit(c))
                                             .ToArray());
                                        SmsDeliverPdu incomingSmsPdu = (SmsDeliverPdu)IncomingSmsPdu.Decode(msgContent, true, msgContent.Length);
                                        string sender = incomingSmsPdu.OriginatingAddress;
                                        string receiveDate = incomingSmsPdu.GetTimestamp().ToString();
                                        string otpDetector = string.Empty;
                                        bool _isUnicode = incomingSmsPdu.UserDataText.Length != incomingSmsPdu.UserDataLength;
                                        bool _isCSMS = incomingSmsPdu.UserDataHeaderPresent;
                                        string content = "";
                                        content = incomingSmsPdu.UserDataText;
                                        bool isMore = incomingSmsPdu.MoreMessages;
                                        //Console.WriteLine($"From: {sender}\r\nUser Content: {content}\r\n Date : {receiveDate} \r\n isMore: { (isMore ? "More" : "single bogy")}\r\n");

                                        List<string> ListNumberArray = new List<string>();
                                        string NumberArray = string.Empty;
                                        string AllNumbers = string.Empty;
                                        foreach (var character in content)
                                        {
                                            if (Char.IsDigit(character))
                                            {
                                                NumberArray += Convert.ToString(character);
                                                AllNumbers += Convert.ToString(character);
                                            }
                                            else
                                            {
                                                ListNumberArray.Add(NumberArray);
                                                NumberArray = string.Empty;
                                            }
                                        }
                                        ListNumberArray.Add(NumberArray);

                                        string otp = string.Empty;
                                        if (string.IsNullOrEmpty(otp))
                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
                                        if (string.IsNullOrEmpty(otp))
                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
                                        if (string.IsNullOrEmpty(otp))
                                            otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
                                        if (string.IsNullOrEmpty(otp))
                                            if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
                                                otp = AllNumbers;

                                        messages.Add(new GSMMessage()
                                        {
                                            Receiver = PhoneNumber,
                                            DisplayCOMName = DisplayName,
                                            COM = PortName,
                                            Sender = sender,
                                            Content = content,
                                            Date = receiveDate,
                                            OTP = otp,
                                            Carrier = GlobalVar.MapCarrier[this.SIMCarrier]
                                        });
                                        //remove message here
                                        lock (RequestLocker)
                                        {
                                            Port.WriteLine($"AT+CMGD={msgIdx}");
                                        }
                                        WaitResultOrTimeout("", 500);
                                        ThreadPool.QueueUserWorkItem(delegate
                                        {
                                            new C3TekPortal().LogAllSMS(sender, PhoneNumber, content, GlobalVar.MapCarrier[this.SIMCarrier], receiveDate);

                                            //safe request
                                            //if ((!MVTGlobalVar.RegisterVar.Stop || !MVNPTGlobalVar.RegisterVar.Stop || !MVNMBGlobalVar.RegisterVar.Stop || !MMFGlobalVar.RegisterVar.Stop) && !string.IsNullOrEmpty(otp) && (sender.Trim() == "MyViettel" || sender.Trim() == "900" || sender.Trim() == "MyMobiFone" || sender.Trim() == "Bima"))
                                            //{
                                            //    //tong dai my* => khong log 2 lan 
                                            //}
                                            //else
                                            //{
                                            //    new C3TekPortal().LogAllSMS(sender, PhoneNumber, content, GlobalVar.MapCarrier[this.SIMCarrier], receiveDate);
                                            //}
                                        }, null);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                    // var user_data_header = incomingSmsPdu.GetUserDataHeader();
                                    //                    var user_content = incomingSmsPdu.GetUserDataTextWithoutHeader();

                                    //     Console.WriteLine($"\r\nFrom: {incomingSmsPdu.}");
                                }

                            }

                            /*
                            if (match.Success && !string.IsNullOrEmpty(match.Value))
                            {
                                string[] msgs = match.Value.Split(new string[] { "+CMGL" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var msg in msgs)
                                {
                                    //PDU MODE
                                    string msgContent = msg.Substring(msg.LastIndexOf("\""), msg.Length - msg.LastIndexOf("\"")).Replace("\"", string.Empty).Replace("\r", string.Empty);
                                    string msgHeader = msg.Substring(0, msg.LastIndexOf("\""));
                                    string[] headerAtts = msgHeader.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    string sender = headerAtts[2].Replace("\"", string.Empty);
                                    string receiveDate = headerAtts[4].Replace("\"", string.Empty);
                                    string otpDetector = string.Empty;


                                    if (msgContent.StartsWith(" "))
                                        msgContent = msgContent.Remove(0, 1);

                                    if (msgContent.EndsWith(" OK"))
                                        msgContent = msgContent.Remove(msgContent.Length - 3, 3);
                                    try
                                    {
                                        if (Regex.IsMatch(msgContent.Trim(), "(?:0[xX])?[0-9a-fA-F]+") && msgContent.Length > 3)
                                        {
                                            string hex = msgContent.Trim();
                                            byte[] bytes = Enumerable.Range(0, hex.Length)
                                                     .Where(x => x % 2 == 0)
                                                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                                     .ToArray();
                                            UTF8Encoding utf8 = new UTF8Encoding();
                                            string content = Encoding.GetEncoding("utf-16BE").GetString(bytes);
                                            if (!string.IsNullOrEmpty(content))
                                                msgContent = content;
                                        }
                                    }
                                    catch { }

                                    List<string> ListNumberArray = new List<string>();
                                    string NumberArray = string.Empty;
                                    string AllNumbers = string.Empty;
                                    foreach (var character in msgContent)
                                    {
                                        if (Char.IsDigit(character))
                                        {
                                            NumberArray += Convert.ToString(character);
                                            AllNumbers += Convert.ToString(character);
                                        }
                                        else
                                        {
                                            ListNumberArray.Add(NumberArray);
                                            NumberArray = string.Empty;
                                        }
                                    }
                                    ListNumberArray.Add(NumberArray);

                                    string otp = string.Empty;
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
                                    if (string.IsNullOrEmpty(otp))
                                        if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
                                            otp = AllNumbers;
                                    messages.Add(new GSMMessage()
                                    {
                                        Receiver = PhoneNumber,
                                        DisplayCOMName = DisplayName,
                                        COM = PortName,
                                        Sender = sender,
                                        Content = msgContent,
                                        Date = receiveDate,
                                        OTP = otp
                                    });
                                    ThreadPool.QueueUserWorkItem(delegate
                                    {
                                        new C3TekPortal().LogAllSMS(sender, PhoneNumber, msgContent, receiveDate);
                                    }, null);

                                }
                            }
                            */
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Console.WriteLine("MC55 Message Parse" + ex.Message); }

            return messages;
        }
        //find message
        public List<GSMMessage> GetNewMessage()
        { 
            List<GSMMessage> messages = new List<GSMMessage>();
            try
            {
                if (IsPortConnected && IsSIMConnected && GlobalVar.AutoDashboardMode)
                {
                    try
                    {
                        ExecuteCommand(GSMCommand.SWITCH_TEXT_MODE);
                        string[] responsees = ExecuteCommand(GSMCommand.GET_NEW_MSG)
                            .Split(new char[] { '' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var res in responsees)
                        {
                            string response = res.Replace("\n", " ").Replace("\r", "");
                            var match = Regex.Match(response, "(\\+CMGL: (.*)OK)");
                            if (match.Success && !string.IsNullOrEmpty(match.Value))
                            {
                                string[] msgs = match.Value.Split(new string[] { "+CMGL" },
                                    StringSplitOptions.RemoveEmptyEntries);
                                foreach (var msg in msgs)
                                {
                                    bool skip_msg = false;
                                    string msgContent =
                                                 msg.Substring(msg.LastIndexOf("\""), msg.Length - msg.LastIndexOf("\""))
                                            .Replace("\"", string.Empty).Replace("\r", string.Empty);
                                    if (msgContent.Contains(","))
                                    {
                                        var matchContent = Regex.Match(response, @"((\d+)?,(\d+) )(.*)(?!OK)");
                                        if (matchContent.Success)
                                        {
                                            msgContent = matchContent.Groups[4].Value.Replace("OK", "") ?? "";
                                            msgContent = msgContent.Trim();
                                        }
                                    }
                                    string msgHeader = msg.Substring(0, msg.LastIndexOf("\""));
                                    string[] headerAtts = msgHeader.Split(new char[] { ',' },
                                        StringSplitOptions.RemoveEmptyEntries);
                                    string sender = headerAtts[2].Replace("\"", string.Empty);
                                    string receiveDate = headerAtts[4].Replace("\"", string.Empty);
                                    string otpDetector = string.Empty;
                                    if (msgContent.StartsWith(" "))
                                        msgContent = msgContent.Remove(0, 1);
                                    

                                    if (msgContent.EndsWith(" OK"))
                                        msgContent = msgContent.Remove(msgContent.Length - 3, 3);
                                    msgContent = msgContent.Trim();
                                    try
                                    {
                                        if (Regex.IsMatch(msgContent.Trim(), "(?:0[xX])?[0-9a-fA-F]+") &&
                                            msgContent.Length > 3)
                                        {
                                            string hex = msgContent.Trim();
                                            byte[] bytes = Enumerable.Range(0, hex.Length)
                                                .Where(x => x % 2 == 0)
                                                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                                .ToArray();
                                            UTF8Encoding utf8 = new UTF8Encoding();
                                            string content = Encoding.GetEncoding("utf-16BE").GetString(bytes);
                                            if (!string.IsNullOrEmpty(content))
                                                msgContent = content;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(" Exception when parese Message", ex.Message);
                                    }

                                    List<string> ListNumberArray = new List<string>();
                                    string NumberArray = string.Empty;
                                    string AllNumbers = string.Empty;
                                    foreach (var character in msgContent)
                                    {
                                        if (Char.IsDigit(character))
                                        {
                                            NumberArray += Convert.ToString(character);
                                            AllNumbers += Convert.ToString(character);
                                        }
                                        else
                                        {
                                            ListNumberArray.Add(NumberArray);
                                            NumberArray = string.Empty;
                                        }
                                    }

                                    ListNumberArray.Add(NumberArray);

                                    string otp = string.Empty;
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
                                    if (string.IsNullOrEmpty(otp))
                                        otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
                                    if (string.IsNullOrEmpty(otp))
                                        if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
                                            otp = AllNumbers;
                                    string[] at_msg_filer = { "+CMGL", "+CMGR", "+CUSD", "+CMGS", "+CMGD" };
                                    foreach(string t in at_msg_filer)
                                    {
                                        if(msgContent.ToUpper().Contains(t))
                                        {
                                            skip_msg = true;
                                        }
                                    }
                                    var match_balance = Regex.Match(msgContent, @"\d+(?:\.\d{1,2})?");
                                    var match_expire = Regex.Match(msgContent, @"\b\d{1,2}([./-])\d{1,2}\1\d{2,4}\b");
                                    msgContent = msgContent.Replace("\n", "").Replace("\r", "");
                                  
                                    if (msgContent.Contains("USD"))
                                    {
                                        if (string.IsNullOrEmpty(Expire))
                                        {
                                            if (match_balance.Success && !string.IsNullOrEmpty(match_balance.Value))
                                        { 
                                              MainBalance = (int)Double.Parse(match_balance.Value);
                                         }
                                            if (match_expire.Success && !string.IsNullOrEmpty(match_expire.Value))
                                            {
                                                Expire = match_expire.Value;
                                            }
                                        }
                                    }
                                    if (skip_msg == false)
                                    {
                                        messages.Add(new GSMMessage()
                                        {
                                            Receiver = PhoneNumber,
                                            DisplayCOMName = DisplayName,
                                            COM = PortName,
                                            Sender = sender,
                                            Content = msgContent,
                                            Date = receiveDate,
                                            OTP = otp,
                                            Carrier = GlobalVar.MapCarrier[this.SIMCarrier]
                                        });
                                        
                                        ThreadPool.QueueUserWorkItem(
                                            delegate
                                            {
                                                new C3TekPortal().LogAllSMS(sender, PhoneNumber, msgContent,
                                                    GlobalVar.MapCarrier[this.SIMCarrier], receiveDate);
                                            }, null);
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GlobalEvent.OnGlobalMessaging($"{PortName} : handle message exception" + ex.Message);
                    }
                }
            }
            catch { }
            return messages;
        }
        void ThreadProc(object data)
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("Thread callback: " + data);
                Thread.Sleep(500);
            }
        }

        public string Call(string dialNumber, int duration = 0)
        {
            return ExecuteCommand(GSMCommand.DIAL, new DialInfo() { DialNo = dialNumber, DurationLimit = (duration <= 0 ? 60 * 1000 * 2 : duration) });
            //    return ExecuteCommand(GSMCommand.DIAL, new DialInfo() { DialNo = dialNumber, DurationLimit = (duration > 60  * 1000 * 20 ? 60 * 1000 * 20 : duration) });
            //}
        }
        public string ExecuteConsumeData(string size_mb)
        {
            return ExecuteCommand(GSMCommand.CONSUME_DATA, size_mb);
        }

        public string ParseResponseBalanceExpireByPattern(string response, string pattern)
        {
            var matchExpire = Regex.Match(response, pattern);

            if (!string.IsNullOrEmpty(matchExpire.Value))
            {
                string expire_date = matchExpire.Value;
                return expire_date;
            }

            return "";
        }

        public string GetExpireMobiStr(string response)
        {
            List<string> listExpirePattern = new List<string>()
            {
                @"([0-9]{2}[-|\/]{1}[0-9]{2}[-|\/]{1}[0-9]{4})",
                @"(\\d{2}\\/\\d{2}\\/\\d{4})"
            };

            string expiredResult = "";
            foreach (string pattern in listExpirePattern)
            {
                expiredResult = ParseResponseBalanceExpireByPattern(response, pattern);
                if (!string.IsNullOrEmpty(expiredResult)) break;
            }
            return expiredResult;
        }

        public string GetBalanceMobiStr(string response)
        {
            List<string> listExpirePattern = new List<string>()
            {
                @"(TKC (.*?) d)",
                @"(TKC:(.*?) d)"
            };

            string expiredResult = "";
            foreach (string pattern in listExpirePattern)
            {
                expiredResult = ParseResponseBalanceExpireByPattern(response, pattern);
                if (!string.IsNullOrEmpty(expiredResult)) break;
            }

            if (expiredResult != null)
                return expiredResult.Replace("TKC", "")
                    .Replace(" ", "")
                    .Replace(":", "")
                    .Replace(",", "")
                    .Replace(".", "")
                    .Replace("d", "").Trim();
            return "";
        }

        public int CheckBalance(string referenceData = "")
        {
            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> START_GET_BALANCE : ");

            int main_balance = -1;
            if (Port != null && Port.IsOpen && IsSIMConnected)
            {
                string response = string.Empty;
                if (!string.IsNullOrEmpty(referenceData))
                    response = referenceData;
                else
                    response = ExecuteCommand(GSMCommand.CHECKBALANCE);


                switch (SIMCarrier)
                {
                    case SIMCarrier.Vinaphone:
                        {
                            var matchBalance = Regex.Match(response, "(TK chinh=(.*?)VND)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK chinh=", "").Replace("VND", ""));
                            }
                            else
                            {
                                //itel
                                matchBalance = Regex.Match(response, "(Tai khoan chinh:?\\s?([0-9]*)?\\sVND)");
                                if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                                {
                                    MainBalance = Convert.ToInt32(matchBalance.Value.Replace("Tai khoan chinh", "")
                                        .Replace("VND", "")
                                        .Replace(",", "")
                                        .Replace(" ", "")
                                        .Replace(":", "")
                                    );
                                }
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                    case SIMCarrier.Mobifone:
                        {
                            MainBalance = Convert.ToInt32(GetBalanceMobiStr(response));
                            Expire = GetExpireMobiStr(response).Replace("-", "/");
                            /* Regex khong het
                            var matchBalance = Regex.Match(response, "(TKC:(.*?) d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKC:", "").Replace(" d", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            */
                            break;
                        }
                    case SIMCarrier.Viettel:
                        {

                            var matchBalance = Regex.Match(response, "(TKG:(.*?)d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKG:", "").Replace("d", "").Replace(".", ""));
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                Expire = matchExpire.Value;
                            }
                            break;
                        }
                    case SIMCarrier.VietnamMobile:
                        {
                            //TKG
                            var matchBalance = Regex.Match(response, "(:(.*?)d)");
                            var matchExpire = Regex.Match(response, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                // MainBalance = Convert.ToInt32(matchBalance.Value.Replace(":", "").Replace("d", ""));
                                //Fix issue get balacne
                                string balance_str = new String(matchBalance.Value.Where(Char.IsDigit).ToArray());
                          
                                MainBalance = Convert.ToInt32(balance_str);
                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                MessageBox.Show(matchExpire.Value);
                                Expire = matchExpire.Value;
                            }
                            break;
                        }

                    case SIMCarrier.DTAC:
                        {
                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> GET_BALANCE : " + response);

                            var matchBalance = Regex.Match(response, "(is (.*?) Baht)");
                            var matchExpire = Regex.Match(response, @"(\d{2}\/\d{2}\/\d{2})");

                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Match value : {matchBalance} {matchExpire}");

                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                            {
                                MainBalance = (int)Convert.ToDouble(matchBalance.Value.Replace("is", "").Replace("Baht", "").Replace(" ", ""));

                            }
                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                            {
                                if (matchExpire.Value.Length > 6)
                                {
                                    Expire = matchExpire.Value.Insert(6, "20");
                                }
                            }
                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Procesed : {MainBalance} {Expire}");

                            break;
                        }
                    case SIMCarrier.Metfone:
                        {
                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> GET_BALANCE : " + response);
                           if(response.ToUpper().Contains("INACTIVE"))
                            {
                                break;
                            }
                            var match_balance = Regex.Match(response, @"(\d+(\.\d+)?|\.\d+)");
                            if(match_balance.Success && !string.IsNullOrEmpty(match_balance.Value))
                            {
                                MainBalance = (int)Convert.ToDouble(match_balance.Value);
                            }
                            var match_expire = Regex.Match(response, @"(\d{2}\/\d{2}\/\d{4})");
                            if(match_expire.Success && !string.IsNullOrEmpty(match_expire.Value))
                            {
                                Expire = match_expire.Value;
                            }
                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Procesed : {MainBalance} {Expire}");
                            break;
                        }              
                }

                main_balance = MainBalance;
                GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Ready");
            }
            return main_balance;
        }
        internal void Start(string portName)
        {
            if (!ComStarted)
            {
                PortName = portName;
                PortConnectionHandler = new Thread(new ThreadStart(PortConnectionHanding));
                SIMConnectionHandler = new Thread(new ThreadStart(SIMConnectionHanding));

                GSMMessageHandler = new Thread(new ThreadStart(GSMMessageHanding));
                //AlertSMSHandler = new Thread(new ThreadStart(AlertSMSHanding));
                //AlertCallHandler = new Thread(new ThreadStart(AlertCallHanding));


                //PortConnectionHandler.Start();
                //SIMConnectionHandler.Start();
                //GSMMessageHandler.Start();
                //  AlertSMSHandler.Start();
                //AlertCallHandler.Start();
                ComStarted = true;
            }
            if (PortConnectionHandler != null && !PortConnectionHandler.IsAlive)
            {
                PortConnectionHandler.Start();
            }
            if (SIMConnectionHandler != null && !SIMConnectionHandler.IsAlive)
            {
                SIMConnectionHandler.Start();
            }
            if (GSMMessageHandler != null && !GSMMessageHandler.IsAlive)
            {
                // GSMMessageHandler.Start();
            }
           
        }

        Dictionary<byte[], string> RIFFTimeline = new Dictionary<byte[], string>();

        private void PortConnectionHanding()
        {
            while (!Stop)
            {
                if (DoNotConnect)
                {
                    ResetInfo();
                    if (Port != null && Port.IsOpen)
                    {
                        Port.Close();
                        Port = null;
                    }
                    Thread.Sleep(1000);
                    continue;
                }
                try
                {
                    if (Port == null)
                    {
                        IsSIMConnected = false;
                        SIMCarrier = SIMCarrier.NO_SIM_CARD;
                        PortState = PortState.DISCONNECTED;
                        PhoneNumber = string.Empty;

                        Port = new SafeSerialPort();

                        Port.AutoReconnect = true;
                        Port.Port = SerialPort.StringToSerialCommPort(PortName);
                        Port.BaudRate = SerialBaudRate.Br115200;
                        Port.DataWidth = SerialDataWidth.Dw8Bits;
                        Port.ParityBits = SerialParityBits.None;
                        Port.StopBits = SerialStopBits.Sb1Bit;
                        Port.HardwareFlowControl = SerialHardwareFlowControl.None;
                        Port.NewLine = "\r\n";
                        Port.AutoReceive = false;
                       

                        //Port.Disconnected += Port_Disconnected;
                        //Port.Faulted += Port_Faulted;
                        //Port.Idle += Port_Idle;

                        Port.Error += Port_Error;
                        //Port = new SafeSerialPort()
                        //{
                        //    PortName = PortName,
                        //    BaudRate = 115200,
                        //    Parity = Parity.None,
                        //    StopBits = StopBits.One,
                        //    DataBits = 8,
                        //    Handshake = Handshake.RequestToSend,
                        //    DtrEnable = true,
                        //    RtsEnable = true,
                        //    NewLine = "\r\n",
                        //    Encoding = Encoding.UTF8,
                        //    WriteTimeout = 60000,
                        //    ReadTimeout = 60000
                        //};
                    }
                    if (!Port.IsOpen || Port.IsFaulted)
                    {
                        IsSIMConnected = false;
                        SIMCarrier = SIMCarrier.NO_SIM_CARD;
                        PhoneNumber = string.Empty;

                        //Port = new SafeSerialPort();
                        //{
                        //    PortName = PortName,
                        //    BaudRate = 115200,
                        //    Parity = Parity.None,
                        //    StopBits = StopBits.One,
                        //    DataBits = 8,
                        //    Handshake = Handshake.RequestToSend,
                        //    DtrEnable = true,
                        //    RtsEnable = true,
                        //    NewLine = "\r\n",
                        //    Encoding = Encoding.UTF8,
                        //    WriteTimeout = 60000,
                        //    ReadTimeout = 60000
                        //};
                        if (Port.Open())
                        {
                            //  Port.RtsEnable = true;
                            IsPortConnected = true;
                            PortState = PortState.CONNECTED;
                            GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Connected");
                        }
                        else
                        {
                            IsPortConnected = false;
                            ResetInfo();
                        }
                    }
                    else
                    {

                        PortState = PortState.CONNECTED;
                        IsPortConnected = true;

                    }
                }
                catch
                {
                    IsPortConnected = false;
                    ResetInfo();
                }
                Thread.Sleep(1000);
            }
            ResetInfo();
            if (Port != null && Port.IsOpen)
            {
                Port.Close();
                Port = null;
            }
        }

        private void Port_Error(object sender, WSafeSerialPort.Serial.ErrorEventArgs e)
        {

            Console.WriteLine($"Port_Idle error {PortName}  {e.Value}");
            this.ResetInfo();
        }

        private void Port_Idle(object sender, EventArgs e)
        {
            Console.WriteLine($"Port_Idle disonnceted {PortName}");
        }

        private void Port_Faulted(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"Fault disonnceted {PortName}");
        }

        private void Port_Disconnected(object sender, ConnectionEventArgs e)
        {
            Console.WriteLine($"Port disconnected {PortName}");
        }

        //private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    SerialPort sp = (SerialPort)sender;
        //    string indata = sp.ReadExisting();
        //    Console.WriteLine("Data Received:");
        //    Console.Write(indata);
        //}


        private void SIMConnectionHanding()
        {
            int reconnect = 0;
            while (!Stop)
            {
            reconnect:
                try
                {

                    if (Port != null && Port.IsOpen)
                    {

                        if (IsCalling || GlobalVar.AutoDashboardMode == false)
                        {
                            // dang goi hoac auto  = false;
                            Thread.Sleep(3500);
                            goto reconnect;
                        }

                        Port.WriteLine("" + (char)27);


                        Thread.Sleep(100);
                        string init_str = ExecuteCommand(GSMCommand.INIT_COMMAND);

                        if (init_str.Contains("RING"))
                        {
                            this.RingDetector(init_str);
                        }


                        Port.WriteLine("AT+CFUN=1");
                        Thread.Sleep(300);
                        Port.WriteLine("AT+CMEE=2");
                        Thread.Sleep(300);

                        Port.WriteLine("AT+CMGF=1");
                        Thread.Sleep(300);

                        Port.WriteLine("AT+CSDH=1");

                        WaitResultOrTimeout("OK", 1000);

                        Port.WriteLine("AT+CMGF=1");

                        WaitResultOrTimeout("OK", 1000);

                        Port.WriteLine("AT+CSCS=\"GSM\"");

                        WaitResultOrTimeout("OK", 1000);




                        //"\r\n MULTIBAND  900E  1800 \r\n\r\nOK\r\n"
                        string modem_name = ExecuteCommand(GSMCommand.GET_MODEM_NAME);
                        if (modem_name.Contains("EC20F"))
                        {
                            this.ModemName = "EC20F";
                        }
                        else if (modem_name.Contains("M26"))
                        {
                            this.ModemName = "M26";
                        }
                        //MC55i
                        else if (modem_name.Contains("MC55"))
                        {
                            this.ModemName = "MC55";
                        }
                        else if (modem_name.Contains("MULTIBAND  900E  1800"))
                        {
                            this.ModemName = "WaveCom";
                        }
                        else if (modem_name.Contains("SIMCOM_SIM5320"))
                        {
                            this.ModemName = "SIMCOM_SIM5320E";
                        }
                        else if (modem_name.Contains("UC20"))
                        {
                            this.ModemName = "UC20";
                        }
                        else
                        {
                            this.ModemName = "Không hỗ trợ";
                            ResetInfo();
                            goto reconnect;
                        }

                        if (ModemName != "MC55" && GSMMessageHandler != null && !GSMMessageHandler.IsAlive && AppModeSetting.AutoTrackingSMS)
                        {
                            GSMMessageHandler.Start();
                        }

                        string checkRing = ExecuteCommand(GSMCommand.CHECKRING);
                        if (IsSIMConnected)
                            RingDetector(checkRing);
                        int remain = 5;

                        string imei_repsonse = ExecuteCommand(GSMCommand.GET_IMEI);
                        if (!string.IsNullOrEmpty(imei_repsonse))
                        {
                            string imei_code = new String(imei_repsonse.Where(Char.IsDigit).ToArray());
                            this.ImeiDevice = imei_code;
                        }
                        
                        else
                        {
                            ResetInfo();
                        }

                    loop:
                        //Port.WriteLine("" + (char)27);
                        string cpin = ExecuteCommand(GSMCommand.CPIN);



                        if (cpin.Contains("CME ERROR: 10"))
                        {
                            Port.WriteLine("AT+CFUN=1");
                            WaitResultOrTimeout("", 1000);
                            GlobalEvent.OnGlobalMessaging($"{this.PortName} AT+CFUN=1,1 => Khong nhan dang duoc the sim");
                            goto loop;
                        }

                        if (IsSIMConnected && (!cpin.Contains("READY") || !cpin.Contains("OK")) && remain > 0)
                        {
                            remain--;
                            goto loop;
                        }

                        if (cpin.Contains("OK") && cpin.Contains("READY"))
                        {

                            if (IsSIMConnected)
                                RingDetector(checkRing);
                            string signal_val = ExecuteCommand(GSMCommand.GET_SIGNAL);
                            try
                            {
                                Match regrex_signal = Regex.Match(signal_val, "(CSQ: (.*?),)");
                                if (regrex_signal.Success)
                                {
                                    string match_val = regrex_signal.Value.Replace("CSQ: ", string.Empty).Replace(",", string.Empty);
                                    this.SignalInt = Convert.ToInt32(match_val);
                                }
                            }
                            catch (Exception ex)
                            {
                                GlobalEvent.OnGlobalMessaging($"{PortName} => Cannot get signal");
                            }
                            reconnect = 0;
                            if (!IsSIMConnected)
                            {
                                ExecuteCommand(GSMCommand.SWITCH_TEXT_MODE);

                                string carrierMsg = ExecuteCommand(GSMCommand.GET_CARRIER).ToUpper();
                                if (carrierMsg.Contains("COPS:"))
                                {

                                    if (AutoAssignCarrier)
                                    {
                                        SIMCarrier = (carrierMsg.ToUpper().Contains("VIETNAMOBILE") || StringHelper.ParseDigitString(carrierMsg).Contains("45205")) ? SIMCarrier.VietnamMobile
                                            : (carrierMsg.ToUpper().Contains("VIETTEL") || carrierMsg.ToUpper().Contains("45204")) ? SIMCarrier.Viettel
                                            : carrierMsg.ToUpper().Contains("MOBIFONE") ? SIMCarrier.Mobifone
                                            : (carrierMsg.ToUpper().Contains("VINAPHONE") || carrierMsg.ToUpper().Contains("45202")) ? SIMCarrier.Vinaphone
                                            : carrierMsg.ToUpper().Contains("DTAC") ? SIMCarrier.DTAC :carrierMsg.ToUpper().Contains("METFONE")?SIMCarrier.Metfone:SIMCarrier.NO_SIM_CARD;
                                        MyProcessMessage = "Nhận dạng nhà mạng tự động....";
                                   
                                    }
                                    else
                                    {
                                        SIMCarrier = CarrierDefault;
                                        MyProcessMessage = "Gán nhà mạng thủ công....";
                                    }

                                    if (SIMCarrier != SIMCarrier.NO_SIM_CARD)
                                    {
                                        GlobalEvent.OnGlobalMessaging($"[{PortName}] -> Carrier Detected");
                                        string iccd_res = ExecuteCommand(GSMCommand.GET_ICCID);
                                        Serial = Regex.Match(iccd_res, @"\d+").Value;
                                        //Serial = Regex.Match(iccd_res, "([A-Za-z0-9]{20,22})").Value;
                                        IsSIMConnected = true;
                                    }

                                    //else
                                    //{

                                    //    Port.WriteLine("AT+CFUN=1");
                                    //    Thread.Sleep(1000);
                                    //    Port.WriteLine("AT+COPS=?");
                                    //    string copsResponse= WaitResultOrTimeout("COPS", 10000, true);
                                    //    GlobalEvent.OnGlobalMessaging($"{PortName} -> copsResponse");
                                    //    goto reconnect;

                                    //}


                                    if (GlobalVar.CheckBalanceAndPhone)
                                    {
                                        string phoneNumberMSG = ExecuteCommand(GSMCommand.GET_PHONE_NUMBER);
                                        GlobalEvent.OnGlobalMessaging($"[{PortName}] -> GET_PHONE_NUMBER : " + phoneNumberMSG);
                                        switch (SIMCarrier)
                                        {
                                            case SIMCarrier.VietnamMobile:
                                                {
                                                    var match = Regex.Match(phoneNumberMSG, "(CUSD: 1,\"Xin chao (.*?)\n)");

                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                        string phoneNumber = match.Value.Replace("CUSD: 1,\"Xin chao ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                                                        phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                        if (phoneNumber.Length == 9)
                                                            phoneNumber = phoneNumber.Insert(0, "0");
                                                        if (phoneNumber.StartsWith("84"))
                                                            phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");

                                                        this.PhoneNumber = phoneNumber;
                                                        IsSIMConnected = true;
                                                        MVNMBGlobalVar.RegisterVar.OnSIMInjected(phoneNumber, PortName, Serial, SIMCarrier);
                                                        CheckBalance(phoneNumberMSG);
                                                        //TTTB = new TTTB();
                                                        // SendMessage("1414", "TTTB");
                                                    }
                                                    else ResetInfo();
                                                    break;
                                                }
                                            case SIMCarrier.Vinaphone:
                                                {
                                                    var match = Regex.Match(phoneNumberMSG, "(\\d{9,12})");
                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                        string phoneNumber = match.Value;

                                                        phoneNumber = StringHelper.FormatPhoneZero(phoneNumber);
                                                        string phone_wintel = phoneNumber.Substring(0, 3);
                                                        if (phone_wintel.Equals("055"))
                                                        {
                                                            var matchBalance = Regex.Match(phoneNumberMSG, "(TKC=(.*?)d)");
                                                            var matchExpire = Regex.Match(phoneNumberMSG, "(\\d{2}\\/\\d{2}\\/\\d{4})");

                                                            if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                                                            {
                                                                MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TKC=", "")
                                                                    .Replace("VND", "")
                                                                    .Replace(",", "")
                                                                    .Replace(" ", "")
                                                                    .Replace(":", "").Replace("d", "")
                                                                );
                                                            }
                                                            if (matchExpire != null && !string.IsNullOrEmpty(matchExpire.Value))
                                                            {
                                                                Expire = matchExpire.Value;
                                                            }
                                                        }
                                                        this.PhoneNumber = phoneNumber;
                                                        IsSIMConnected = true;
                                                        MVNPTGlobalVar.RegisterVar.OnSIMInjected(phoneNumber, PortName, Serial, SIMCarrier);
                                                        CheckBalance();
                                                        //TTTB = new TTTB();
                                                        //SendMessage("1414", "TTTB");
                                                    }
                                                    else ResetInfo();
                                                    break;
                                                }
                                            case SIMCarrier.Viettel:
                                                {
                                                    Match match = null;
                                                    //if (isPrepaid)
                                                    //{
                                                        match = Regex.Match(phoneNumberMSG, "(\\d{9,12})");
                                                    //}
                                                    //else
                                                    //{
                                                        //match = Regex.Match(phoneNumberMSG, "(\\d{10})");
                                                    //}
                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                        string phoneNumber = match.Value;
                                                        phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                        if (phoneNumber.Length == 9)
                                                            phoneNumber = phoneNumber.Insert(0, "0");
                                                        if (phoneNumber.StartsWith("84"))
                                                            phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");
                                                        this.PhoneNumber = phoneNumber;
                                                        
                                                        IsSIMConnected = true;
                                                        
                                                        
                                                        MVTGlobalVar.RegisterVar.OnSIMInjected(phoneNumber, PortName, Serial, SIMCarrier);
                                                        if (isPrepaid)
                                                        {
                                                            CheckBalance(phoneNumberMSG);
                                                        }
                                                        //TTTB = new TTTB();
                                                        //SendMessage("1414", "TTTB");
                                                    }
                                                    else
                                                    {
                                                        ResetInfo();
                                                    }
                                                    
                                                    break;
                                                }
                                            case SIMCarrier.Mobifone:
                                                {
                                                    var match = Regex.Match(phoneNumberMSG, "(\\d{9,12})");
                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                        string phoneNumber = match.Value;
                                                        phoneNumber = StringHelper.FormatPhoneZero(phoneNumber);

                                                        //phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                        //if (phoneNumber.Length == 9)
                                                        //    phoneNumber = phoneNumber.Insert(0, "0");
                                                        //if (phoneNumber.StartsWith("84"))
                                                        //    phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");

                                                        this.PhoneNumber = phoneNumber;
                                                        IsSIMConnected = true;
                                                        MMFGlobalVar.RegisterVar.OnSIMInjected(phoneNumber, PortName, Serial, this.SIMCarrier);
                                                        CheckBalance();
                                                        //TTTB = new TTTB();
                                                        // SendMessage("1414", "TTTB");
                                                    }
                                                    else ResetInfo();
                                                    break;
                                                }

                                            case SIMCarrier.DTAC:
                                                {
                                                    var match = Regex.Match(phoneNumberMSG, @"(\d{11}|\d{10})");
                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                        string phoneNumber = match.Value;
                                                        phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                        if (phoneNumber.Length == 9)
                                                            phoneNumber = phoneNumber.Insert(0, "0");
                                                        if (phoneNumber.StartsWith("84"))
                                                            phoneNumber = phoneNumber.Remove(0, 2).Insert(0, "0");
                                                        this.PhoneNumber = phoneNumber;
                                                        IsSIMConnected = true;
                                                        CheckBalance();
                                                        //TTTB = new TTTB();
                                                        // SendMessage("1414", "TTTB");
                                                    }
                                                    else ResetInfo();
                                                    break;
                                                }
                                            case SIMCarrier.Metfone:
                                                {
                                                 
                                                    var match = Regex.Match(phoneNumberMSG, @"(\b\d{11,12}\b)");
                                                    if (match.Success && !string.IsNullOrEmpty(match.Value))
                                                    {
                                                     
                                                        string phoneNumber = match.Value;
                                                        phoneNumber = phoneNumber.Replace("+", string.Empty);
                                                        if (phoneNumber.StartsWith("855"))
                                                            phoneNumber = phoneNumber.Remove(0, 3).Insert(0, "0");
                                                        this.PhoneNumber = phoneNumber;
                                                        IsSIMConnected = true;
                                                        CheckBalance();
                                                    }
                                                    else ResetInfo();
                                                    break;
                                                }
                                            default:
                                                {
                                                    if (!string.IsNullOrEmpty(PhoneNumber))
                                                        GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Default switch Disconnected");
                                                    this.PhoneNumber = string.Empty;
                                                    break;
                                                }
                                        }
                                        if (!string.IsNullOrEmpty(Serial))
                                        {
                                            GSMControlCenter.webSocketClient.SendUpdateGSM(new List<GSMCom>()
                                            {
                                                 this
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    this.MyProcessMessage = "Không nhận dạng được cây sóng COPS!";
                                }
                            }
                            else
                            {
                                if (ModemName == "MC55")
                                {
                                    GSMMessageHandingMC55();
                                }
                                else
                                {
                                    if (AppModeSetting.OneTimeMode)
                                    {
                                        this.MyProcessMessage = "Chế độ thủ công...";
                                        return;
                                    }
                                }


                                if (SIMCarrier == SIMCarrier.VietnamMobile && ModemName != "MC55")
                                {
                                    if (string.IsNullOrEmpty(Expire))
                                    {
                                        Thread.Sleep(5000);
                                        GlobalEvent.OnGlobalMessaging($"{PortName} => VNMobile Wait expired");
                                        if (string.IsNullOrEmpty(Expire))
                                        {
                                            GlobalEvent.OnGlobalMessaging($"{PortName} => VNMobile Still Null => Retry");
                                            goto loop;
                                        }
                                    }
                                }

                                while (this.GetSocketJobWorker().HasJobQueue())
                                {
                                    this.GetSocketJobWorker().ConsumeNextJob();

                                }

                                string remain_data = Port.ReadExisting();
                                if (remain_data.Contains("RING"))
                                {
                                    RingDetector(remain_data);

                                }
                                else
                                {
                                    RingDetector(cpin);
                                }
                            }
                        }
                        else
                        {
                            RingDetector(cpin);
                            if (IsSIMConnected)
                            {
                                if (reconnect < 1)
                                {
                                    reconnect++;
                                    goto reconnect;
                                }
                                else
                                {
                                    //GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                                }
                            }
                            ResetInfo();
                        }
                    }
                    else
                    {
                        if (IsSIMConnected)
                        {
                            if (reconnect < 1)
                            {
                                reconnect++;
                                goto reconnect;
                            }
                            else
                            {
                                GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                            }
                        }
                        ResetInfo();
                    }

                }
                catch (Exception ex)
                {
                    if (IsSIMConnected)
                    {
                        if (reconnect < 1)
                        {
                            reconnect++;
                            goto reconnect;
                        }
                        else
                        {
                            GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
                        }
                    }
                    ResetInfo();
                }


                Thread.Sleep(2000);
            }
            //if (IsSIMConnected)
            //    GlobalEvent.OnGlobalMessaging($"{PortName} -> {PhoneNumber} -> Disconnected");
            ResetInfo();
        }

        private DateTime VNMBUpdateDate = new DateTime(1900, 01, 01);

        private void GSMMessageHandingMC55()
        {

            try
            {
                if (IsPortConnected && IsSIMConnected && GlobalVar.RealtimeSMSTracking)
                {
                    var messages = ParseNewMessageMC55();
                    foreach (var message in messages)
                    {
                        lock (GSMControlCenter.LockGSMMessages)
                        {

                            GSMControlCenter.GSMMessages.Add(message);
                            GSMControlCenter.OnNewMessage(message);
                            NotifyAlert();
                        }
                        if (SIMCarrier == SIMCarrier.VietnamMobile && (message.Sender == "123" || message.Sender == "+123") && Regex.IsMatch(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))"))
                        {

                            try
                            {
                                CultureInfo provider = CultureInfo.InvariantCulture;
                                string date = message.Date.Substring(0, message.Date.IndexOf("+"));
                                //var accountDate = DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", provider);
                                //check message
                                if (true)
                                //if (accountDate > VNMBUpdateDate)
                                {
                                    string accountInfo = Regex.Match(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))").Value;
                                    var matchBalance = Regex.Match(accountInfo, "(TK Chinh: (.*?)d,)");
                                    var matchExpire = Regex.Matches(accountInfo, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                                    if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                                    {
                                        MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK Chinh: ", "").Replace("d,", "").Replace(".", string.Empty).Replace(",", string.Empty));
                                    }
                                    if (matchExpire != null && matchExpire.Count > 0)
                                    {
                                        Expire = matchExpire[0].Value;
                                    }
                                    //VNMBUpdateDate = accountDate;
                                }
                            }
                            catch { }
                        }
                        if (message.Sender == "1414")
                        {
                            try
                            {
                                TTTB.full_content += message.Content;
                                TTTB.phone = this.PhoneNumber;
                                
                                string[] attributes = TTTB.full_content.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                                for (int index = 0; index < attributes.Length; index++)
                                {
                                    if (index == 0)
                                    {
                                        string[] subAttributes = attributes[index].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (subAttributes.Any() && subAttributes.Length == 5)
                                        {
                                            string[] propertyAttributes = subAttributes[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.full_name = propertyAttributes[1];

                                            propertyAttributes = subAttributes[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.birthday = propertyAttributes[1];

                                            propertyAttributes = subAttributes[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.cmnd = propertyAttributes[1];

                                            propertyAttributes = subAttributes[3].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.address_cmnd = propertyAttributes[1];

                                            propertyAttributes = subAttributes[4].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.date_cmnd = propertyAttributes[1];
                                        }
                                    }
                                    if (index == 1)
                                    {
                                        string[] subAttributes = attributes[index].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (subAttributes.Any() && subAttributes.Length == 2)
                                        {
                                            string[] propertyAttributes = subAttributes[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (propertyAttributes.Length == 2)
                                                TTTB.type_tb = propertyAttributes[1];
                                            try
                                            {
                                                propertyAttributes = subAttributes[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.date_active = propertyAttributes[1];
                                            }
                                            catch { }
                                            TTTBHelper.RequestPush(this.TTTB);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }

            }
            catch { }

        }
        private void GSMMessageHanding()
        {
            while (!Stop && AppModeSetting.AutoTrackingSMS)
            {
                try
                {
                    if (IsPortConnected && IsSIMConnected && GlobalVar.AutoDashboardMode && GlobalVar.RealtimeSMSTracking)
                    {
                        GlobalEvent.OnGlobalMessaging("Read SMS event");
                        var messages = GetNewMessage();
                        GlobalEvent.OnGlobalMessaging($"Read SMS event count : {messages.Count} sms");

                        foreach (var message in messages)
                        {
                            lock (GSMControlCenter.LockGSMMessages)
                            {
                                GSMControlCenter.GSMMessages.Add(message);
                                GSMControlCenter.OnNewMessage(message);
                                NotifyAlert();
                            }
                            if (SIMCarrier == SIMCarrier.VietnamMobile && (message.Sender == "123" || message.Sender == "+123") && Regex.IsMatch(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))"))
                            {
                                try
                                {
                                    CultureInfo provider = CultureInfo.InvariantCulture;
                                    string date = message.Date.Substring(0, message.Date.IndexOf("+"));
                                    //var accountDate = DateTime.ParseExact(date, "yyyy/MM/dd HH:mm:ss", provider);
                                    //check message
                                    if (true)
                                    //if (accountDate > VNMBUpdateDate)
                                    {
                                        string accountInfo = Regex.Match(message.Content, "(TK Chinh: (.*?)het han (\\d{2}\\/\\d{2}\\/\\d{4}))").Value;
                                        var matchBalance = Regex.Match(accountInfo, "(TK Chinh: (.*?)d,)");
                                        var matchExpire = Regex.Matches(accountInfo, "(\\d{2}\\/\\d{2}\\/\\d{4})");
                                        if (matchBalance != null && !string.IsNullOrEmpty(matchBalance.Value))
                                        {
                                            MainBalance = Convert.ToInt32(matchBalance.Value.Replace("TK Chinh: ", "").Replace("d,", "").Replace(".", string.Empty).Replace(",", string.Empty));
                                        }
                                        if (matchExpire != null && matchExpire.Count > 0)
                                        {
                                            Expire = matchExpire[0].Value;
                                        }
                                        //VNMBUpdateDate = accountDate;
                                    }
                                }
                                catch { }
                            }
                            if (message.Sender == "1414")
                            {
                                try
                                {
                                    TTTB.full_content += message.Content;
                                    TTTB.phone = this.PhoneNumber;

                                    string[] attributes = TTTB.full_content.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                                    for (int index = 0; index < attributes.Length; index++)
                                    {
                                        if (index == 0)
                                        {
                                            string[] subAttributes = attributes[index].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (subAttributes.Any() && subAttributes.Length == 5)
                                            {
                                                string[] propertyAttributes = subAttributes[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.full_name = propertyAttributes[1];

                                                propertyAttributes = subAttributes[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.birthday = propertyAttributes[1];

                                                propertyAttributes = subAttributes[2].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.cmnd = propertyAttributes[1];

                                                propertyAttributes = subAttributes[3].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.address_cmnd = propertyAttributes[1];

                                                propertyAttributes = subAttributes[4].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.date_cmnd = propertyAttributes[1];
                                            }
                                        }
                                        if (index == 1)
                                        {
                                            string[] subAttributes = attributes[index].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                            if (subAttributes.Any() && subAttributes.Length == 2)
                                            {
                                                string[] propertyAttributes = subAttributes[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                if (propertyAttributes.Length == 2)
                                                    TTTB.type_tb = propertyAttributes[1];
                                                try
                                                {
                                                    propertyAttributes = subAttributes[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                                    if (propertyAttributes.Length == 2)
                                                        TTTB.date_active = propertyAttributes[1];
                                                }
                                                catch { }
                                                TTTBHelper.RequestPush(this.TTTB);
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Read mesage EXCEPTION", ex.Message);
                    GlobalEvent.OnGlobalMessaging($"Read SMS event error" + ex.Message);

                }
                Thread.Sleep(2000);
            }
        }
        public void GetNewTaskFromCenter()
        {

        }
        public void ChangeIMEI(string imei)
        {
            ExecuteCommand(GSMCommand.CHANGE_EMEI, imei);
        }
        public void ChangeRandomIMEI()
        {
            
                Random random = new Random();
                int index = random.Next(1, imeiList.Count);
                int checksum_digit = random.Next(1, 10);
                string imei = generatePhoneImeiNumber(imeiList[index], checksum_digit);
                this.ChangeIMEI(imei);
        }
        public void ChangeImeiManual(string imei)
        {
            this.ChangeIMEI(imei);
        }
        private string pduMobileSM(string mobile)
        {
            if (mobile.Length == 11) mobile += "F";
            char[] array = mobile.ToCharArray();
            return Convert.ToString(array[1]) + Convert.ToString(array[0]) +
                Convert.ToString(array[3]) + Convert.ToString(array[2]) +
                Convert.ToString(array[5]) + Convert.ToString(array[4]) +
                Convert.ToString(array[7]) + Convert.ToString(array[6]) +
                Convert.ToString(array[9]) + Convert.ToString(array[8])
                ;
        }
        
        public bool checkSimBank()
        {
            Port.WriteLine("AT+CWSIM");
            var response = WaitResultOrTimeout("CWSIM", 5000);
            if(string.IsNullOrEmpty(response))
            {
                return false;
            }
            return true;
        }
        public bool resetAllChannel()
        {
            Port.WriteLine("AT+NEXT00");
            var response = WaitResultOrTimeout("RESET", 5000);
            if(string.IsNullOrEmpty(response))
            {
                return false;
            }
            return true;

        }
        public bool switchSimChannel(string val)
        {
            string command = "";
            if (val.Length==2)
            {
                command = $"AT+SWIT00-00{val}";
            }
            else
            {
                command = $"AT+SWIT00-000{val}";
            }
            Port.WriteLine(command);
            var response = WaitResultOrTimeout("SWITCH",5000);
            if(string.IsNullOrEmpty(response))
            {
                return false;
            }
            return true;
        }

        public bool switchSimWithChannel(string channel,string port)
        { string command = "";
            if(port.Length==2)
            {
                command = $"AT+SWIT0{channel}-00{port}";
            }
            else
            {
                command = $"AT+SWIT0{channel}-000{port}";
            }
            Port.WriteLine(command);
            var response = WaitResultOrTimeout("SWITCH", 5000);
            response = response.Replace("ERROR", "");
            if(string.IsNullOrEmpty(response))
            {
                return false;
            }
            return true;
        }

        public bool resetModemPort()
        {
            Port.WriteLine("AT+CFUN=0");
            var response = WaitResultOrTimeout("OK", 500);
            if(!string.IsNullOrEmpty(response) && !response.Contains("ERROR"))
            {   
                Port.WriteLine("AT+CFUN=1");
                return true;
            }
            else
            {
                return false;
            }
        }

        public string sendMsgPDU(string mobilenumber, string message)
        {
            string res = string.Empty;


            Port.WriteLine("AT+CSCS=\"GSM\"");
            WaitResultOrTimeout("OK", 500, true);

            Port.WriteLine("AT+CMGF=0");
            WaitResultOrTimeout("OK", 500, true);

            Port.WriteLine("AT+CMMS=2");
            WaitResultOrTimeout("OK", 500, true);



            SMS sms = new SMS();
            sms.Direction = SMSDirection.Submited;
            sms.PhoneNumber = mobilenumber;
            sms.ValidityPeriod = new TimeSpan(0, 0, 0, 30);
            sms.Message = message;

            string pduSource = sms.Compose(SMS.SMSEncoding.UCS2);





            Port.Write("AT+CMGS=" + (pduSource.Length / 2 - 1) + "\r");
            WaitResultOrTimeout(">", 2000, false);

            Port.Write(pduSource + (char)(26) + "\r");
            res = WaitResultOrTimeout("OK", 10000, true);



            Port.WriteLine("AT+CMMS=0");
            WaitResultOrTimeout("OK", 1000, true);

            Port.WriteLine("AT+CMGF=1");
            WaitResultOrTimeout("OK", 1000, true);

            if (res.Contains("ERROR") || res.Contains("CME ERR"))
            {
                return "ERROR";
            }
            else
            {
                return "OK";
            }


        }


        public string sendMsg(string mobilenumber, string Message, string SMSC, int tongthoigiancho = 20)
        {
            string res = string.Empty;
            int demchoguitin = 0;

            string str2 = WaitResultOrTimeout("AT", 300);
            Port.WriteLine("AT+CMGF=1");

            str2 = WaitResultOrTimeout("", 300);
            if (SMSC != string.Empty)
            {
                Port.WriteLine("AT+CSCA");

                str2 = WaitResultOrTimeout("", 500);
            }

            try
            {
                if (mobilenumber.StartsWith("+"))
                {
                    Port.WriteLine("AT");

                    string command = "AT+CMGS=\"" + mobilenumber + "\"";
                    Port.WriteLine(command);
                    WaitResultOrTimeout("", 300);
                    command = Message + (char)(26);
                    Port.WriteLine(command);
                    res = WaitResultErrorOrTimeout("+CMGS", 10000);
                    if (res.Contains("ERROR") || res.Contains("CME ERR"))
                    {
                        return "ERROR";
                    }
                    else
                    {
                        return "OK";
                    }
                }

                else
                {

                    string sm = "";
                    int offset = 0;
                    string smnum = "";
                    string smnumpdu = "";
                    //if (SMSC != string.Empty)
                    //{
                    //    str2 = this.ExecCommand("AT+CSCA=" + SMSC, 500);
                    //    Thread.Sleep(1000);
                    //}
                    //string scms = "0691481920005";
                    if (SMSC == string.Empty)
                    {
                        Port.WriteLine("AT+CSCA?");
                        sm = WaitResultOrTimeout("", 500);
                    }
                    if (sm.Contains("+84"))
                    {
                        offset = sm.IndexOf("+84");
                        smnum = sm.Substring(offset + 1, 10);
                        smnum = smnum.Trim();
                        smnumpdu += "0" + ((smnum.Length / 2) + 1).ToString();
                        smnumpdu += "91";
                        smnumpdu += pduMobileSM(smnum);
                    }
                    if (sm.Contains("ERROR"))
                    {
                        return res = "ERROR";
                    }
                    else
                    {
                        Port.WriteLine("AT");//Quectel
                        WaitResultOrTimeout("", 300);
                        Port.WriteLine("AT+CGMI");
                        string cgmi_res = WaitResultOrTimeout("", 1000);
                        if (cgmi_res.Contains("") && !(this.SIMCarrier == SIMCarrier.VietnamMobile && mobilenumber == "1441"))
                        {
                            string pduString = createFlash(mobilenumber, Message); //"011000B914839620272F30000AA09E8F71D1406E5DF75"; //
                            int pdulen = pduString.Length;
                            pdulen = (pdulen / 2) - 1;

                            string pduString1 = "AT+CMGS=" + pdulen + (char)(13) + pduString + (char)(26);
                            string setpdulen = "AT+CMGS=" + pdulen;
                            //string pduString1 = "AT+CMGS=16\r0011000C914821457939420000AA023119" + char.ConvertFromUtf32(26);
                            //Port.WriteLine("AT+CNMI=2,2,0,1,0");
                            //WaitResultOrTimeout("", 300);
                            Port.WriteLine("AT");
                            WaitResultOrTimeout("", 300);
                            Port.WriteLine("AT+CMGF=0");
                            WaitResultOrTimeout("", 300);

                            if (mobilenumber.Length == 9)
                                mobilenumber = "0" + mobilenumber;
                            if (mobilenumber.StartsWith("0"))
                                mobilenumber = "84" + mobilenumber.Remove(0, 1);
                            if (mobilenumber.StartsWith("840"))
                                mobilenumber = "+84" + mobilenumber.Remove(0, 3);
                            if (!mobilenumber.StartsWith("840"))
                                mobilenumber = "+" + mobilenumber;
                            //ExecCommand("AT+CSMP=49,167,0,242", 300);
                            bool unicode = CheckIsUTF8(Message);
                            string str = "";
                            SmsSubmitPdu[] pdu = SmartMessageFactory.CreateConcatTextMessage(Message, unicode, mobilenumber);
                            foreach (SmsSubmitPdu pduItem in pdu)
                            {
                                //str = ExecCommand_p3("AT+CMGS=" + pduItem.ActualLength, 500,frmMain.tu,frmMain.den, index);
                                Port.WriteLine("AT+CMGS=" + pduItem.ActualLength);
                                str = WaitResultOrTimeout("", 500);
                                Port.WriteLine(pduItem.ToString() + (char)(26));
                                str = WaitResultOrTimeout("+CMGS", 7000);

                                //str = ExecCommand_p3(pduItem.ToString() + char.ConvertFromUtf32(26), 1000, frmMain.tu, frmMain.den, index);


                                int count = 0;
                                while (true)
                                {
                                    str += WaitResultErrorOrTimeout("+CMGS", 1000);
                                    if (str.Contains("OK") || str.Contains("ERROR") || res.Contains("CME ERR"))
                                        break;
                                    count++;

                                    if (count > tongthoigiancho)
                                        break;
                                }
                            }

                            //ExecCommand(setpdulen, 500);

                            //res= ExecCommand(pduString + char.ConvertFromUtf32(26), 500);
                            if (str.Contains("ERROR") || res.Contains("CME ERR"))
                            {
                                return "ERROR";
                            }
                            if (str.Contains("OK"))
                            {
                                res = "OK";
                            }

                        }
                        else
                        {
                            Port.WriteLine("AT+CMGF=1");
                            WaitResultOrTimeout("", 500);
                            string mobiletext = mobilenumber;
                            if (mobilenumber.StartsWith("84"))
                            {
                                mobiletext = "+" + mobilenumber;
                            }
                            string command = "AT+CMGS=\"" + mobiletext + "\"";
                            Port.WriteLine(command);
                            WaitResultOrTimeout("", 300);
                            command = Message + (char)(26);

                            Port.WriteLine(command);

                            res = WaitResultOrTimeout("+CMGS", 7000);
                            while (demchoguitin < tongthoigiancho)
                            {
                                if (res.Contains("OK") || res.Contains("ERROR") || res.Contains("CME ERR"))
                                {

                                    break;
                                }
                                else
                                {
                                    res += WaitResultErrorOrTimeout("+CMGS", 1000);
                                }
                                demchoguitin++;
                            }

                            if (res.Contains("OK") || res.Contains("ERROR") || res.Contains("CME ERR"))
                            {

                            }
                            else
                            {
                                res = "noresult";
                            }

                            if (res.Contains("ERROR") || res.Contains("noresult") || res.Contains("CME ERR"))
                            {
                                string pduString = createFlash(mobilenumber, Message); //"011000B914839620272F30000AA09E8F71D1406E5DF75"; //
                                int pdulen = pduString.Length;
                                pdulen = (pdulen / 2) - 1;

                                string pduString1 = "AT+CMGS=" + pdulen + char.ConvertFromUtf32(13) + pduString + char.ConvertFromUtf32(26);
                                string setpdulen = "AT+CMGS=" + pdulen;
                                //string pduString1 = "AT+CMGS=16\r0011000C914821457939420000AA023119" + char.ConvertFromUtf32(26);

                                //Port.WriteLine("AT+CNMI=2,2,0,1,0");
                                //WaitResultOrTimeout("", 300);
                                Port.WriteLine("AT");
                                WaitResultOrTimeout("", 300);
                                Port.WriteLine("AT+CMGF=0");
                                WaitResultOrTimeout("", 300);

                                Port.WriteLine(setpdulen);
                                WaitResultOrTimeout("", 500);

                                Port.WriteLine(char.ConvertFromUtf32(13));
                                WaitResultOrTimeout("", 500);

                                Port.WriteLine(char.ConvertFromUtf32(26));
                                WaitResultOrTimeout("", 500);

                                Port.WriteLine(char.ConvertFromUtf32(13));
                                res = WaitResultErrorOrTimeout("", 7000);

                                //ExecCommand("AT+CSMP=49,167,0,242", 300);



                                demchoguitin = 0;
                                while (demchoguitin < tongthoigiancho)
                                {
                                    if (res.Contains("OK") || res.Contains("ERROR") || res.Contains("CME ERR"))
                                    {

                                        break;
                                    }

                                    res += WaitResultErrorOrTimeout("+CMGS", 1000);
                                    demchoguitin++;
                                }

                                if (res.Contains("OK") || res.Contains("ERROR") || res.Contains("CME ERR"))
                                {

                                }
                                else
                                {
                                    res = "noresult";
                                }

                            }
                        }

                    }
                    //Thread.Sleep(3000);

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //this.ErrorLog(exception.ToString());
            }
            //this.ClosePort();
            return res;
        }

        public string SendMessage(string receiver, string content, int tongthoigiancho = 20)
        {
            if (receiver.StartsWith("0") && receiver.Length != 10 && receiver.Length != 11)
            {
                return "ERROR_PHONE_NUMBER";
            }
            else if (receiver.StartsWith("84") && receiver.Length != 11 && receiver.Length != 12)
            {
                return "ERROR_PHONE_NUMBER";
            }
            else if (receiver.StartsWith("+84") && receiver.Length != 12 && receiver.Length != 13)
            {
                return "ERROR_PHONE_NUMBER";
            }
            else if (receiver.Length < 3)
            {
                return "ERROR_PHONE_NUMBER";
            }



            if (IsSIMConnected)
            {
                string res = string.Empty;
                lock (RequestLocker)
                {
                    if (ModemName == "WaveCom" || ModemName.Contains("Wavecom"))
                    {
                        res = sendMsgPDU(receiver, content);
                    }
                    else
                    {
                        res = sendMsg(receiver, content, "", tongthoigiancho);
                    }
                    if (res.Contains("ERROR") || res.Contains("CME ERR")) { return "ERROR"; }
                    else { return "OK"; }
                    //return sendMsg(receiver, content, "");
                }
                //return ExecuteCommand(GSMCommand.SEND_MESSAGE, new SendMessageData() { Receiver = receiver, Content = content });
            }
            return "NOT_CONNECTED_OR_PHONE";
        }
        internal void Dispose()
        {

            MVTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
            MMFGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
            MVNPTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
            MVNMBGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);

            Stop = true;
        }
        bool isPrepaid = true;
        private void ResetInfo()
        {
            switch (SIMCarrier)
            {
                case SIMCarrier.Mobifone:
                    {
                        MMFGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);

                        break;
                    }
                case SIMCarrier.Viettel:
                    {
                        MVTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
                        break;
                    }
                case SIMCarrier.Vinaphone:
                    {
                        MVNPTGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
                        break;
                    }
                case SIMCarrier.VietnamMobile:
                    {
                        MVNMBGlobalVar.RegisterVar.OnSIMRejected(PhoneNumber);
                        break;
                    }
            }
            this.GetSocketJobWorker().ClearAllJob();
            if (IsSIMConnected)
            {
                GSMControlCenter.webSocketClient.SendUpdateGSM(new List<GSMCom>()
                                            {
                                                 this
                                            });
            }
            PortState = PortState.DISCONNECTED;
            SIMCarrier = SIMCarrier.NO_SIM_CARD;
            PhoneNumber = string.Empty;
            IsSIMConnected = false;
            Expire = string.Empty;
            MainBalance = 0;
            MyRegisterState = MyRegisterState.None;
            MyProcessMessage = string.Empty;
            VNMBUpdateDate = new DateTime(1900, 01, 01);
            if (!string.IsNullOrEmpty(Serial))
            {
                GSMControlCenter.webSocketClient.SendDrawPort(this);
            }
            Serial = string.Empty;

            ModemName = string.Empty;
            ImeiDevice = string.Empty;
            IsCalling = false;
            TTTB = null;
            isPrepaid = true;
            is_khmer_change_imei = true;
            SignalInt = 0;
        }
        public void Reconnect()
        {
            try
            {
                if (Port != null && Port.IsOpen)
                {   
                    Port.Close();
                    Port = null;
                }

                if (SIMConnectionHandler != null && AppModeSetting.OneTimeMode)
                {
                    try
                    {
                        SIMConnectionHandler.Abort();
                        SIMConnectionHandler = new Thread(new ThreadStart(SIMConnectionHanding));
                        SIMConnectionHandler.Start();
                    }
                    catch (Exception ex)
                    {

                    }
                }


                DoNotConnect = false;
                IsPortConnected = false;
                ResetInfo();
                //GlobalEvent.OnGlobalMessaging($"[{PortName} -> Disconnected]");
            }
            catch { }
        }

        public async void reConnect()
        {
            try
            {
                DoNotConnect = false;
                IsPortConnected = false;
                is_reset_port = true;
                ResetInfo();
                Port.WriteLine("AT+CFUN=1,1");
                var response = WaitResultOrTimeout("Call", 20000, false);
                await Task.Delay(5000);
            }
            catch (Exception er)
            {

            }
        }

        private string WaitSlowResultOrTimeout(string containSucceed, int timeout)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
                loop:
                //Thread.Sleep(50);
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout
                    || !IsPortConnected || Stop)
                {
                    LogResponseCommand(result);
                    return string.Empty;
                }


                string response = Port.ReadExisting();

                if (response.Contains(containSucceed))
                {
                    //OK\r\n containSuccess  OK
                    string real_response = response.Substring(response.IndexOf(containSucceed));
                    result += real_response;
                }
                else if (result.Contains(containSucceed))
                {
                    result += response;
                }
                else
                {

                }
                // Console.Write(result);
                //LogResponseCommand(response);
                if (result.Contains(containSucceed) && result.Contains("OK"))
                {
                    LogResponseCommand(result);
                    return result;
                }
                else
                {
                    goto loop;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("WaitSlow Serial EXception", ex.Message);
            }
            return string.Empty;
        }
        private string WaitResultWithCommand(string cmd_expected, int timeout)
        {
            try
            {  
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
                bool flag_start_cmd = false;
                command_loop:
               // Thread.Sleep(50);
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout
                    || !IsPortConnected || Stop)
                {
                    LogResponseCommand(result);
                    return result;
                }
                string response = Port.ReadExisting();
                //start at cmd_expected
                result += response;

                if (result.Contains(cmd_expected))
                {
                    flag_start_cmd = true;
                    //TODO
                    if (result.Contains("ERROR"))
                    {
                        return "ERROR";
                    }
                }

                if (flag_start_cmd == false)
                {
                    goto command_loop;
                }
                else
                {
                    //start append to result
                    if (result.Contains("ERROR"))
                    {
                        return "ERROR";
                    }
                    if (response.Contains("\r"))
                    {
                        var match_len_str = Regex.Match(result, "(\\" + cmd_expected + ": (.*)\r)");
                        if (match_len_str.Value != null && match_len_str.Value.Length > 2)
                        {
                            return (!string.IsNullOrEmpty(match_len_str.Value) ? match_len_str.Value : "");
                        }
                        //else loop
                        goto command_loop;

                    }
                    else
                    {
                        goto command_loop;
                    }
                }
                if (string.IsNullOrEmpty(result))
                {
                    return "";
                }
                else
                {
                    //(\\+CDS: (.*)\r)
                    var match = Regex.Match(result, "(\\" + cmd_expected + ": (.*)\r)");
                    return (!string.IsNullOrEmpty(match.Value) ? match.Value : "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WaitResultWithCommand] " + ex.Message);
                return "";


            }
        }
        public bool ENABLE_LOOP_BACK_SERIAL = false;

        private string WaitResultErrorOrTimeout(string containSucceed, int timeout)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
                loop:
                //Thread.Sleep(50);
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout
                    || !IsPortConnected || Stop)
                {
                    LogResponseCommand(result);
                    return string.Empty;
                }

                string response = Port.ReadExisting();
                result += response;
                if (ENABLE_LOOP_BACK_SERIAL)
                {
                    //Console.Write(result);
                    //LogResponseCommand(response);
                }
                // Console.Write(result);
                //LogResponseCommand(response);
                if (result.Contains("ERROR") || (result.Contains(containSucceed) && result.Contains("OK")))
                {
                    LogResponseCommand(result);
                    return result;
                }
                else
                {
                    goto loop;
                }

            }
            catch { }
            return string.Empty;
        }



       
        private string WaitResultOrTimeout(string containSucceed, int timeout, bool ckOk = true)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                string result = string.Empty;
                loop:
                //Thread.Sleep(50);
                if ((DateTime.Now - startTime).TotalMilliseconds > timeout
                    || !IsPortConnected || Stop)
                {
                    LogResponseCommand(result);
                    return result;
                }

                string response = Port.ReadExisting();
                result += response;
                if (ENABLE_LOOP_BACK_SERIAL)
                {
                    //Console.Write(result);
                    //LogResponseCommand(response);
                }
                // Console.Write(result);
                //LogResponseCommand(response);
                if (result.Contains("RING"))
                {
                    this.RingDetector(result);
                    return "";
                }
                if (ckOk == true)
                {
                    if (result.Contains(containSucceed) && result.Contains("OK"))
                    {
                        LogResponseCommand(result);
                        return result;
                    }
                    else
                    {
                        goto loop;
                    }
                }
                else
                {
                    if (result.Contains(containSucceed))
                    {
                        LogResponseCommand(result);
                        return result;
                    }
                    else
                    {
                        goto loop;
                    }

                }

            }
            catch { }
            return string.Empty;
        }
        private void WaitEmptyResponse()
        {
            try
            {
                DateTime startTime = DateTime.Now;
                loop:
                //Thread.Sleep(50);
                if (Stop)
                    return;

                string response = Port.ReadExisting();
                LogResponseCommand(response);
                if (string.IsNullOrEmpty(response))
                    return;
                else goto loop;
            }
            catch { }
        }
        public bool SignalAlert { get; set; }

        DateTime AlertTime = new DateTime();
        private void NotifyAlert()
        {
            AlertTime = DateTime.Now;
            
        }

        public string parseOTP(string content) //loc otp
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            MatchCollection matches = Regex.Matches(content, @"Nhap \d{4}\b", RegexOptions.Singleline);
            string otp = new String(matches[0].Value.Where(c => Char.IsDigit(c)).ToArray()) ?? string.Empty;
            return otp;
            //List<string> ListNumberArray = new List<string>();
            //string NumberArray = string.Empty;
            //string AllNumbers = string.Empty;
            //foreach (var character in content)
            //{
            //    if (Char.IsDigit(character))
            //    {
            //        NumberArray += Convert.ToString(character);
            //        AllNumbers += Convert.ToString(character);
            //    }
            //    else
            //    {
            //        ListNumberArray.Add(NumberArray);
            //        NumberArray = string.Empty;
            //    }
            //}
            //ListNumberArray.Add(NumberArray);

            //string otp = string.Empty;
            //if (string.IsNullOrEmpty(otp))
            //    otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 6);
            //if (string.IsNullOrEmpty(otp))
            //    otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 5);
            //if (string.IsNullOrEmpty(otp))
            //    otp = ListNumberArray.FirstOrDefault(numberArr => numberArr.Length == 4);
            //if (string.IsNullOrEmpty(otp))
            //    if (AllNumbers.Length >= 4 && AllNumbers.Length <= 6)
            //        otp = AllNumbers;
            //return otp;
        }



        //public string ExecuteUSSDTopup(string ussd, string matt)
        //{
        //    LastUSSDCommand = ussd;
        //    LastUSSDResult = string.Empty;
        //    bool reset_sesison_ussd = true;

        //    if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_MULTI_USSD))
        //    {
        //        // khong nhieu lop => luon reset
        //        reset_sesison_ussd = true;
        //    }

        //    if (ussd.StartsWith("*"))
        //    {
        //        reset_sesison_ussd = true;
        //    }
        //    else
        //    {
        //        reset_sesison_ussd = false;
        //    }

        //    lock (RequestLocker)
        //    {
        //        WaitEmptyResponse();
        //        if (reset_sesison_ussd)
        //        {
        //            Port.WriteLine("AT+CUSD=2");
        //            WaitResultOrTimeout("CUSD", 3000);
        //        }
        //        string response = "";
        //        try
        //        {
        //            if (ModemName == "MC55")
        //            {

        //                Port.WriteLine($"ATDT{ussd};");
        //                response = Port.ReadStringUpToEndChars(",15", 10000);
        //                response = response.Replace("\r", string.Empty)
        //                    .Replace("\n", string.Empty)
        //                    .Replace("OK", string.Empty)
        //                    .Replace("+CUSD:1,", string.Empty)
        //                    .Replace("\"", string.Empty).Replace("+CUSD=2,", string.Empty);


        //                LastUSSDResult = response;
        //            }
        //            else
        //            {
        //                Port.WriteLine("AT+CUSD=1,\"" + ussd + "\",15");
        //                response = Port.ReadStringUpToEndChars(",15", 10000);
        //                response = StringHelper.RemoveLineEndings(response);

        //                //response = WaitResultOrTimeout("CUSD", 10000);
        //                //response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
        //                //.Replace(",15\r\n\r\nOK\r\n", string.Empty)
        //                //.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
        //                //.Replace("\"", string.Empty)
        //                //.Replace("+CUSD: 1,\"", string.Empty)
        //                // .Replace("\"", string.Empty)
        //                //  .Replace("\n", string.Empty)
        //                //    .Replace("\r", string.Empty).Replace("OK", string.Empty);
        //                //response = response.Replace("+CUSD: 1,", string.Empty);
        //                LastUSSDResult = response;
        //            }
        //            if (response.Contains("Nhap"))
        //            {
        //                string otp_verify = parseOTP(response);
        //                if (!string.IsNullOrEmpty(otp_verify))
        //                {
        //                    return this.ExecuteUSSDTopup(otp_verify, matt);
        //                }
        //            }
        //        }
        //        catch { }
        //    }
        //    return LastUSSDResult;
        //}

        public string TransferCaller(string code)
        {
            int startnumber = 0;
            int endnumber = 0;
            string number = string.Empty;
            string command = string.Empty;

            string response = "";
            int num_retry = 0;
            if (code.Contains("**") || code.Contains("##"))
            {
                if (code.Contains("**21*"))
                {
                retry_chuyen_cuoc_goi:
                    startnumber = code.IndexOf("**21*") + 5;
                    endnumber = code.IndexOf("#");
                    number = code.Substring(startnumber, endnumber - startnumber);
                    command = $"AT+CCFC=4,3,\"{number}\"";
                    Port.WriteLine(command);
                    response = WaitResultOrTimeout("OK", 20000);
                    if (response.Contains("OK"))
                    {
                        response = "Chuyển cuộc gọi vô điều kiện đến số thuê bao " + number + " thành công.";
                        LastUSSDResult = "Chuyển cuộc gọi vô điều kiện đến số thuê bao " + number + " thành công.";
                    }
                    else
                    {
                        Port.WriteLine("AT+CCFC=0,2");
                        response = WaitResultOrTimeout("+84", 20000);
                        //command = "ATDT" + char.ConvertFromUtf32(34) + code + char.ConvertFromUtf32(34) + ";";
                        //Port.WriteLine(command);
                        //response = WaitResultOrTimeout("", 7000);
                        //Thread.Sleep(2000);
                        //Port.WriteLine("AT+CCFC=0,2");
                        if (response.Contains("+84"))
                        {
                            response = "Đã chuyển cuộc gọi vô điều kiện đến số thuê bao " + number + " thành công.";
                        }
                        else
                        {
                            if (num_retry < 3)
                            {
                                num_retry++;
                                LastUSSDResult = $"Không thành công => Retry {num_retry} - {response}";
                                goto retry_chuyen_cuoc_goi;
                            }
                            response = "ERROR, Không nhận dạng được trạng thái chuyển" + response;
                        }
                    }

                }
                else if (code.Contains("##21#"))
                {
                retry_huy_chuyen_cuoc_goi:
                    command = "AT+CCFC=5,4";
                    Port.WriteLine(command);
                    response = WaitResultOrTimeout("OK", 20000);


                    command = "AT+CCFC=0,4";
                    Port.WriteLine(command);
                    response = WaitResultOrTimeout("OK", 20000);


                    if (response.Contains("OK"))
                    {
                        Port.WriteLine("AT+CCFC=0,2");
                        response = WaitResultOrTimeout("+84", 20000);
                        if (!response.Contains("+84") && response.Contains("0,7"))
                        {
                            response = "Hủy chuyển cuộc gọi vô điều kiện thành công.";
                        }
                        else
                        {
                            if (num_retry < 3)
                            {
                                num_retry++;
                                LastUSSDResult = $"Không thành công => Retry {num_retry} - {response}";
                                goto retry_huy_chuyen_cuoc_goi;
                            }
                            response = "ERROR, Chưa hủy được : " + response;
                        }
                    }
                    else
                    {
                        Port.WriteLine("AT+CCFC=0,2");
                        response = WaitResultOrTimeout("+84", 10000);
                        if (!response.Contains("+84") && response.Contains("0,7"))
                        {
                            response = "Hủy chuyển cuộc gọi vô điều kiện thành công.";
                        }
                        else
                        {
                            response = "ERROR, Chưa hủy được : " + response;
                        }

                        //Port.WriteLine("AT+CCFC=0,2");
                        //response = WaitResultOrTimeout("OK", 10000);
                        //command = $"ATDT\"{code}\";";
                        //Port.WriteLine(command);
                        //response = WaitResultOrTimeout("", 7000);
                        //Thread.Sleep(2000);
                        //Port.WriteLine("AT+CCFC=0,2");
                        //response = WaitResultOrTimeout("", 7000);

                    }
                }
            }
            return response;
        }
        public string ExecuteSingleUSSD(string ussd, bool reset_sesison_ussd = true)
        {
            LastUSSDCommand = ussd;
            LastUSSDResult = string.Empty;


            if (!AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.MULTI_PORT_MULTI_USSD))
            {
                // khong nhieu lop => luon reset
                reset_sesison_ussd = true;
            }

            if (ussd.StartsWith("*"))
            {
                reset_sesison_ussd = true;
            }
            else
            {
                reset_sesison_ussd = false;
            }


            lock (RequestLocker)
            {
                if (reset_sesison_ussd)
                {
                    Port.WriteLine("AT+CUSD=2");
                    WaitResultOrTimeout("CUSD", 3000);
                }
                string response = "";

                if (ussd.Contains("**") || ussd.Contains("##"))
                {
                    LastUSSDResult = this.TransferCaller(ussd);
                }
                else
                {
                    try
                    {
                        if (ModemName == "MC55")
                        {

                            Port.WriteLine($"ATDT{ussd};");
                            response = Port.ReadStringUpToEndChars(",15", 10000);
                            response = response.Replace("\r", string.Empty)
                                .Replace("\n", string.Empty)
                                .Replace("OK", string.Empty)
                                .Replace("+CUSD:1,", string.Empty)
                                .Replace("\"", string.Empty).Replace("+CUSD=2,", string.Empty);


                            LastUSSDResult = response;
                        }
                        else if (ModemName.Contains("SIMCOM_SIM5320E"))
                        {
                            Port.WriteLine("AT+CUSD=1,\"" + ussd + "\",15");
                            response = Port.ReadStringUpToEndChars(",15", 10000);
                            response = StringHelper.RemoveLineEndings(response);
                            if (reset_sesison_ussd)
                            {

                                Port.WriteLine("AT+CUSD=1,\"" + ussd + "\",15");
                                response = Port.ReadStringUpToEndChars(",15", 10000);
                                response = StringHelper.RemoveLineEndings(response);
                            }

                            //response = WaitResultOrTimeout("CUSD", 10000);
                            //response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
                            //.Replace(",15\r\n\r\nOK\r\n", string.Empty)
                            //.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
                            //.Replace("\"", string.Empty)
                            //.Replace("+CUSD: 1,\"", string.Empty)
                            // .Replace("\"", string.Empty)
                            //  .Replace("\n", string.Empty)
                            //    .Replace("\r", string.Empty).Replace("OK", string.Empty);
                            //response = response.Replace("+CUSD: 1,", string.Empty);
                            LastUSSDResult = response;
                        }
                        else
                        {
                            Port.WriteLine("AT+CUSD=1,\"" + ussd + "\",15");
                            response = Port.ReadStringUpToEndChars(",15", 10000);
                            response = StringHelper.RemoveLineEndings(response);

                            //response = WaitResultOrTimeout("CUSD", 10000);
                            //response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
                            //.Replace(",15\r\n\r\nOK\r\n", string.Empty)
                            //.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
                            //.Replace("\"", string.Empty)
                            //.Replace("+CUSD: 1,\"", string.Empty)
                            // .Replace("\"", string.Empty)
                            //  .Replace("\n", string.Empty)
                            //    .Replace("\r", string.Empty).Replace("OK", string.Empty);
                            //response = response.Replace("+CUSD: 1,", string.Empty);
                            LastUSSDResult = response;
                        }
                        if (response.Contains("Nhap"))
                        {
                            string otp_verify = parseOTP(response);
                            if (!string.IsNullOrEmpty(otp_verify))
                            {
                                return this.ExecuteSingleUSSD(otp_verify, false);
                            }
                        }
                    }
                    catch { }
                }
            }
            return LastUSSDResult;
        }


        public Action<string> USSDRequest = (ussd) => { Console.WriteLine("[USSDRequest] Chua khoi tao "); };
        public Action<string> USSDResponse = (response) => { Console.WriteLine("[USSDResponse] Chua khoi tao "); };
        public Action USSDCancel = () => { Console.WriteLine("[USSDCancel] Chua khoi tao "); };
        public Action USSDReset = () => { Console.WriteLine("[USSDReset] Chua khoi tao "); };

        public void ResetUSSDEvent()
        {
            USSDRequest = (ussd) => { Console.WriteLine("[USSDRequest] Chua khoi tao "); };
            USSDResponse = (response) => { Console.WriteLine("[USSDResponse] Chua khoi tao "); };
            USSDCancel = () => { Console.WriteLine("[USSDCancel] Chua khoi tao "); };
            USSDReset = () => { Console.WriteLine("[USSDReset] Chua khoi tao "); };

        }

        public void USSDHook()
        {
            bool hooking = true;
            bool first = true;
            USSDCancel = () =>
            {
                try
                {
                    Port.WriteLine("" + (char)27);
                    Thread.Sleep(100);
                    first = true; hooking = false;
                    Port.WriteLine("ATH");
                    WaitResultOrTimeout("", 100);
                }
                catch { }
            };
            USSDRequest = (ussd) =>
            {

                string response = "";

                try
                {
                    if (ussd.Contains("**") || ussd.Contains("##"))
                    {
                        USSDResponse(this.TransferCaller(ussd));
                    }
                    else
                    {
                        if (ModemName == "MC55")
                        {
                            WaitEmptyResponse();
                            Thread.Sleep(100);
                            if (first == false)
                            {
                                //lan thu 2
                                Port.Write($"{ussd}");
                                Thread.Sleep(100);
                                Port.WriteLine("" + (char)26);
                            }
                            else
                            {
                                Port.WriteLine($"ATDT{ussd};");
                                first = false;
                            }

                            response = Port.ReadStringUpToEndChars(",15", 10000);
                            LastUSSDResult = response;
                            USSDResponse(response);


                        }
                        else
                        {
                            WaitEmptyResponse();

                            Port.WriteLine("AT+CUSD=1,\"" + ussd + "\",15");
                            response = Port.ReadStringUpToEndChars(",15", 10000);
                            response = response.Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 1,\"", string.Empty)
                            .Replace(",15\r\n\r\nOK\r\n", string.Empty)
                            .Replace("AT+CUSD=1,\"" + ussd + "\",15\r\r\n+CUSD: 2,\"", string.Empty)
                            .Replace("\"", string.Empty)
                            .Replace("+CUSD: 1,\"", string.Empty)
                             .Replace("\"", string.Empty)
                            .Replace("\n", string.Empty)
                            .Replace("\r", string.Empty);
                            response = response.Replace("+CUSD: 1,", string.Empty);
                            USSDResponse(response);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("USSD exception: " + ex.Message);

                }

            };
            USSDReset = () =>
            {
                try
                {

                    first = true;
                    Port.WriteLine("" + (char)27);
                    Thread.Sleep(100);
                    Port.WriteLine("ATH");
                    WaitResultOrTimeout("", 100);
                    Port.WriteLine("AT+CUSD=2");
                    WaitResultOrTimeout("CUSD", 1000);


                }
                catch (Exception ex) { Console.WriteLine("USSD Reset " + ex.Message); }
            };

            lock (RequestLocker)
            {
                WaitEmptyResponse();
                while (hooking && !GlobalVar.IsApplicationExit)
                {
                    Thread.Sleep(100);
                }
            }

        }
    }
    public enum RingStatus
    {
        Idle,
        Ringing
    }
    public enum SIMCarrier
    {
        NO_SIM_CARD = 0,
        Vinaphone = 1,
        Mobifone = 2,
        VietnamMobile = 3,
        Viettel = 4,
        DTAC = 10,//sim du lich thai lan
        Metfone=11 //sim khmer
    }
    public enum PortState
    {
        DISCONNECTED, CONNECTED
    }
    public enum GSMCommand
    {
        AT = 0,
        CPIN = 1,
        GET_NEW_MSG = 2,
        GET_UNREAD_MSG = 3,
        DELETE_READED_MSG = 4,
        GET_CARRIER = 5,
        GET_PHONE_NUMBER = 6,
        SWITCH_TEXT_MODE = 7,
        RESET_INBOX = 8,
        DIAL = 9,
        CHECKRING = 10,
        GETCALLERID = 11,
        ANSWER_CALL = 12,
        CHECKBALANCE = 13,
        DROPCALL = 14,
        SEND_MESSAGE = 15,
        GET_ICCID = 16,
        CONSUME_DATA = 17,
        GET_IMEI = 18,
        CHANGE_EMEI = 19,
        GET_MODEM_NAME = 20,
        INIT_COMMAND = 21,
        GET_SIGNAL = 22,
        ACTIVATE_SIM = 23,
    }
    public enum MyRegisterState
    {
        None,
        Processing,
        Succeed,
        NoOTP,
        Failed
    }

}
