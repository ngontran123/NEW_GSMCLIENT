using C3TekClient.C3Tek;
using C3TekClient.Core.GSMWebSocket.Model;
using C3TekClient.Core.JobModel;
using C3TekClient.GSM;
using C3TekClient.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;


namespace C3TekClient.Core.GSMWebSocket
{
    public class WebSocketClient
    {
        //public static string WS_SERVER_NAME = "ws://localhost:8080";
        public static string WS_SERVER_NAME = "ws://103.109.43.215:9595";

        public static bool _DEBUG_MODE_ = true;
        public WebSocket ws;
        public BindingList<GSMCom> _coms;
        public BindingList<BaseJobInfo> _jobs;
        private Thread _reconnectThread;
        private Thread _checkSubscriptionThread;
        private Thread _checkIntervalPing;

        private int _autoReconnectCheckInterval = 4000;
        private int _autoCheckSubscriptionInterval = 60000;
        private int _autoPingInterval = 5000;
        public int AutoReconnectCheckInterval
        {
            get { return _autoReconnectCheckInterval; }
            set { _autoReconnectCheckInterval = value; }
        }
        public int AutoCheckSubscriptionInterval
        {
            get { return _autoCheckSubscriptionInterval; }
            set { _autoCheckSubscriptionInterval = value; }
        }

        private static bool AutoReconnect = true;
        //public enum SIMCarrier
        //{
        //    NO_SIM_CARD = 0,
        //    Vinaphone = 1,
        //    Mobifone = 2,
        //    VietnamMobile = 3,
        //    Viettel = 4
        //}
        public static Dictionary<SIMCarrier, string> _mapCarrier = new Dictionary<SIMCarrier, string>()
        {
            {SIMCarrier.NO_SIM_CARD, "NONE" },
            {SIMCarrier.Vinaphone ,  "VNP" },
            {SIMCarrier.Viettel , "VTT" },
            {SIMCarrier.Mobifone , "VMS" },
            {SIMCarrier.VietnamMobile, "VNM" },
            {SIMCarrier.DTAC, "DTAC" },
            {SIMCarrier.Metfone,"METFONE" }
        };

        public WebSocketClient(BindingList<GSMCom> GSMComs)
        {
            ws = new WebSocket(WS_SERVER_NAME);
            _jobs = new BindingList<BaseJobInfo>();
            ws.OnOpen += Ws_OnOpen;
            ws.OnError += Ws_OnError;
            ws.OnClose += Ws_OnClose;
            ws.OnMessage += Ws_OnMessage;
            ws.EmitOnPing = true;
            this._coms = GSMComs;
        }
        public void initCheckSubscriptionThread()
        {
            ThreadStart checksubjob = CheckSubscriptionThreadJob;
            _checkSubscriptionThread = new Thread(checksubjob);
            _checkSubscriptionThread.Name = "Check Subscription Job";
            _checkSubscriptionThread.IsBackground = true;
            _checkSubscriptionThread.Start();
        }
        public void CheckSubscriptionThreadJob()
        {
            while (!GlobalVar.IsApplicationExit) //&& IsFileHandleInvalid(_comDevice))
            {
                try
                {
                    if(ws.IsAlive){
                        SendCheckSubscription();
                    }
                }
                catch (Exception ex) { }
                Thread.Sleep(_autoCheckSubscriptionInterval);

            }
        }



        public void initIntervalPing()
        {
            ThreadStart checkIntervalPing = CheckIntervalPing;
            _checkIntervalPing = new Thread(checkIntervalPing);
            _checkIntervalPing.Name = "Check Interval Ping Job";
            _checkIntervalPing.IsBackground = true;
            _checkIntervalPing.Start();
        }
        public void CheckIntervalPing()
        {
            while (!GlobalVar.IsApplicationExit) //&& IsFileHandleInvalid(_comDevice))
            {
                try
                {
                    if (ws.IsAlive)
                    {
                        ws.Ping();
                        ws.Send("ping");
                        ws.Send("pong");
                    }
                }
                catch (Exception ex) { }
                Thread.Sleep(_autoPingInterval);

            }
        }



        public void handleReconnect()
        {
            if (_reconnectThread == null || !_reconnectThread.IsAlive)
            {
                ThreadStart reconnectJob = ReconnectThreadJob;
                _reconnectThread = new Thread(reconnectJob);
                _reconnectThread.Name = "Serial Reconnect";
                _reconnectThread.IsBackground = true;
                _reconnectThread.Start();
            }
        }
        /// <summary>
        /// Reconnect thread's main method.
        /// </summary>
        private void ReconnectThreadJob()
        {
            while (!GlobalVar.IsApplicationExit && AutoReconnect && !isAlive()) //&& IsFileHandleInvalid(_comDevice))
            {
                try
                {
                    this.Open();
                }
                catch { }
                Thread.Sleep(_autoReconnectCheckInterval);
            }
        }
        public bool isAlive()
        {
            return this.ws.IsAlive;
        }
        public void Open()
        {
            ws.Connect();
            _jobs = new BindingList<BaseJobInfo>();
            //open refresh basejobinfo

        }

        public void SendDrawPort(GSMCom com)
        {
            DrawPortRequest req = new DrawPortRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "DRAW_PORT",
                data = new DrawPortData()
                {
                    comDraw = com.PortName
                }
            };

            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }
        public void SendUpdateGSM(List<GSMCom> gsmChangeStatus)
        {
            List<COMDataInfo> comChangeInfo = new List<COMDataInfo>();
            string current_mac = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up).Select(nic => nic.GetPhysicalAddress().ToString()).FirstOrDefault() ?? "DEFAULT-MAC";
            string machine_name = System.Environment.MachineName ?? "PC";
            foreach (GSMCom item in gsmChangeStatus)
            {
                COMDataInfo comData = new COMDataInfo()
                {
                    modemName = item.ModemName,
                    carrier = _mapCarrier[item.SIMCarrier],
                    imeiDevice = item.ImeiDevice,
                    mainBalance = item.MainBalance,
                    phoneNumber = item.PhoneNumber,
                    realCom = item.PortName,
                    expired = item.Expire,
                    serial = item.Serial,
                    virCom = item.DisplayName
                };
                comChangeInfo.Add(comData);
            }


            AddInfoPCRequest req = new AddInfoPCRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "ADD_INFO_PC",
                data = new AddComInfoRequestData()
                {
                    infoPc = new InfoPCData()
                    {
                        name = machine_name,
                        macAddress = current_mac
                    },
                    listCom = comChangeInfo
                }
            };

            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }
        public void SendCheckSubscription()
        {
            Console.WriteLine("SendCheckSubscription()");
            CheckSubscriptionRequest req = new CheckSubscriptionRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "CHECK_SUBSCRIPTION"
            };
            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }
        public void SendGetListTask()
        {
            Console.WriteLine("SendGetListTask()");
            GetListTaskRequest req = new GetListTaskRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "GET_LIST_TASK",
                status = 0,
                page = 0
            };
            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }

        public void SendUpdateTask(BaseJobInfo job)
        {
            Console.WriteLine("SendUpdateTask()");
            int status = -1;
            if (job.ResultJobState == EJobResultState.SUCCESS)
            {
                status = 2; //success
            }
            else if (job.ResultJobState == EJobResultState.FAIL)
            {
                status = 3; //fail
            }
            else if (job.ResultJobState == EJobResultState.RUNNING)
            {
                status = 1; //processing
            }
            else if (job.ResultJobState == EJobResultState.NONE)
            {
                //(not receive result)
            }
            if (status == -1)
            {
                return;
            }

            UpdateTaskRequest req = new UpdateTaskRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "UPDATE_TASK",
                Data =
                   new UpdateTaskData()
                   {
                       task_id = job.Id,
                       status = status,
                       message_completed = job.ResultMessage,
                       message_comment = ""
                   }

            };



            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }

        public void CancelTask(int task_id, string reason = "Không tìm thấy cổng COM")
        {
            Console.WriteLine("SendUpdateTask()");

            UpdateTaskRequest req = new UpdateTaskRequest()
            {
                AccessToken = Client.AccessToken,
                Action = "UPDATE_TASK",
                Data =
                   new UpdateTaskData()
                   {
                       task_id = task_id,
                       status = 5,
                       message_completed = "",
                       message_comment = reason
                   }

            };



            string json_data = ConvertHelper.getJson(req);
            this.sendData(json_data);
        }
        private string RandomIV()
        {
            byte[] IV = new byte[16];
            Random random = new Random();
            for (int i = 0; i < 16; i++)
            {
                random.NextBytes(IV);
            }
            return System.Convert.ToBase64String(IV);
        }
        public string Encrypt(string plainText)
        {
            string result = string.Empty;
            using (Chilkat.Crypt2 crypt = new Chilkat.Crypt2())
            {
                string iv = RandomIV();
                var ab = System.Convert.FromBase64String(iv);
                crypt.CryptAlgorithm = "aes";
                crypt.CipherMode = "cbc";
                crypt.KeyLength = 256;
                crypt.PaddingScheme = 0;
                crypt.EncodingMode = "hex";
                crypt.SetEncodedIV(iv, "base64");
                crypt.SetEncodedKey(keyHex, "hex");
                crypt.Charset = "utf-8";

                using (var bin = new Chilkat.BinData())
                {
                    byte[] encryptedBytes = crypt.EncryptString(plainText);
                    bin.AppendBinary(encryptedBytes);
                    using (var gzip = new Chilkat.Gzip())
                    {
                        byte[] compresseed = gzip.CompressMemory(encryptedBytes);
                        result = System.Convert.ToBase64String(compresseed) + ":" + iv;
                    }
                }

            }
            return result;

        }
        private readonly string keyHex = "36333643363936353645373435463643363136443733363936443245363236393741";

        public string Decrypt(string encryptedString)
        {
            string result = string.Empty;
            string iv = string.Empty;
            string dataEncrypted = string.Empty;

            string[] attributes = encryptedString.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (attributes.Length == 2)
            {
                dataEncrypted = attributes[0];
                iv = attributes[1];
            }
            else
            {
                dataEncrypted = attributes[0];
            }

            using (Chilkat.Crypt2 crypt = new Chilkat.Crypt2())
            {
                crypt.Charset = "utf-8";
                crypt.CryptAlgorithm = "aes";
                crypt.CipherMode = "cbc";
                crypt.KeyLength = 256;
                crypt.PaddingScheme = 0;
                crypt.EncodingMode = "hex";
                if (!string.IsNullOrEmpty(iv))
                    crypt.SetEncodedIV(iv, "base64");
                crypt.SetEncodedKey(keyHex, "hex");
                using (var bin = new Chilkat.BinData())
                {
                    bool success = bin.AppendEncoded(dataEncrypted, "base64");
                    if (success)
                        using (var gzip = new Chilkat.Gzip())
                        {
                            success = gzip.UncompressBd(bin);
                            if (success)
                            {
                                success = crypt.DecryptBd(bin);
                                if (success)
                                {
                                    string base64 = bin.GetEncoded("base64");
                                    byte[] data = System.Convert.FromBase64String(base64);
                                    result = System.Text.ASCIIEncoding.UTF8.GetString(data);
                                }
                            }
                        }

                }
            }

            return result;
        }
        public string BuildReq<T>(T data)
        {
            if (data == null) { return ""; }
            JObject rss =
                new JObject(
                    new JProperty("data", data)
                );
            return rss.ToString();

        }
        public void sendData(string data)
        {
            //string dataSend = "{ \"isDebug\": true,\r\n" + "\"data\" : " + data + "}";
            
            string dataSend = "{ \"isDebug\": false,\r\n" + "\"data\" : \"" +  Encrypt(data) + "\"}";
            GlobalEvent.OnJobLog(dataSend);
            if (ws.IsAlive)
            {
                ws.Send(dataSend);
            }
        }
        public void send(byte[] req)
        {
            if (ws.IsAlive)
            {
                ws.Send(req);
            }
        }
        public void Send(string req)
        {
            if (ws.IsAlive)
            {
                ws.Send(req);
            }
        }
        public void Close()
        {
            ws.Close();
        }
        //
        // serial => listjob
        public static Dictionary<string, Dictionary<int, BaseJobInfo>> _jobBySerials;

        public bool pushToListCenterTask(string serial, BaseJobInfo job)
        {
            if (!_jobBySerials.ContainsKey(serial))
            {
                //check job exist
                _jobBySerials.Add(serial, new Dictionary<int, BaseJobInfo>());
            }
            Dictionary<int, BaseJobInfo> dictJob = _jobBySerials[serial];
            if (dictJob.ContainsKey(job.Id))
            {
                return false;
            }
            dictJob.Add(job.Id, job);
            return true;
        }
        public class SocketPackageData
        {
            public string data { get; set; }
        }
        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine($"OnMessage() received:  {e.IsText} {e.RawData} ");
            if (!_DEBUG_MODE_)
                throw new Exception("Implement the decrypt here");

            if (e.IsPing)
            {
                // Do something to notify that a ping has been received.
                ws.Send("pong");
                return;
            }

            string json = e.Data;
            if(Client.GetCurrentAccount().Username == "0846889911") { 
                GlobalEvent.OnJobLog("Raw data: ");
                GlobalEvent.OnJobLog(json);
            }
            try
             {
                SocketPackageData o = JsonConvert.DeserializeObject<SocketPackageData>(json);
                if (o == null || o.data == null)
                {
                    return; 
                }
                //JObject data = (JObject)o["data"];

                string data_json = Decrypt(o.data.ToString());
                if (Client.GetCurrentAccount().Username == "0846889911")
                {
                    GlobalEvent.OnJobLog("Decoded data: ");
                    GlobalEvent.OnJobLog(data_json);
                }
                if (data_json.Contains("RES_GET_LIST_TASK"))
                {
                    GetListTaskResponse listTaskResponse = JsonConvert.DeserializeObject<GetListTaskResponse>(data_json);
                    foreach (GetListTaskDetail task in listTaskResponse.Data)
                    {
                        int task_id = task.TASK_ID;
                        var com = _coms.Where(c => c.Serial == task.Serial).FirstOrDefault();

                        if (com == null)
                        {
                            continue;
                        }
                        BaseJobInfo job_info = null;
                        switch (task.Type)
                        {
                            case "SINGLE_USSD":
                                JObject ussdContent = JObject.Parse(task.Content);
                                if (ussdContent != null)
                                {
                                    string ussd_to = (ussdContent["to"] != null) ? ussdContent["to"].ToString() : "";

                                    job_info = new SocketUSSDJobInfo(task_id, "SINGLE_USSD", "SINGLE_USSD", new USSDJobBody(ussd_to), 0);
                                }
                                break;
                            case "SMS":
                                JObject smsContent = JObject.Parse(task.Content);
                                if (smsContent != null)
                                {
                                    string sms_to = (smsContent["to"] != null) ? smsContent["to"].ToString() : "";
                                    string sms_msg = (smsContent["body"] != null) ? smsContent["body"].ToString() : "";
                                    job_info = new SocketSMSJobInfo(task_id, "SMS-TASK", "SMS", new SMSJobBody(sms_to, sms_msg), 0);
                                }
                                break;
                            case "CALL":
                                JObject callContent = JObject.Parse(task.Content);
                                if (callContent != null)
                                {
                                    string call_to = (callContent["to"] != null) ? callContent["to"].ToString() : "";
                                    string call_duration = (callContent["time_dial"] != null) ? callContent["time_dial"].ToString() : "";
                                    int call_duration_int = Convert.ToInt32(call_duration);
                                    job_info = new SocketCallJobInfo(task_id, "CALL-TASK", "CALL", new CallJobBody(call_to, call_duration_int), 0);
                                }
                                break;
                            case "CONSUME_DATA":
                                //content = 1MB 

                                JObject consumeDataContent = JObject.Parse(task.Content);
                                if (consumeDataContent != null)
                                {
                                    string package = (consumeDataContent["to"] != null) ? consumeDataContent["to"].ToString() : "";
                                    job_info = new SocketConsumeDataJobInfo(task_id, "CONSUME-DATA-TASK", "CONSUME_DATA", new ConsumeDataJobBody(package), 0);
                                }

                                break;
                            case "MULTI_USSD":
                                JObject multiUSSDContent = JObject.Parse(task.Content);
                                if (multiUSSDContent != null)
                                {
                                    string ussd_to = (multiUSSDContent["to"] != null) ? multiUSSDContent["to"].ToString() : "";

                                    job_info = new SocketMultiUSSDJobInfo(task_id, "MULTI-USSD-TASK", "MULTI_USSD", new USSDJobBody(ussd_to), 0);
                                }
                                break;
                            case "USSD_BALANCE":
                                JObject ussdBalanceContent = JObject.Parse(task.Content);
                                if (ussdBalanceContent != null)
                                {
                                    string ussd_to = (ussdBalanceContent["to"] != null) ? ussdBalanceContent["to"].ToString() : "";

                                    job_info = new SocketUSSDBalanceJobInfo(task_id, "USSD-BALANCE-TASK", "USSD_BALANCE", new USSDJobBody(ussd_to), 0);
                                }
                                break;
                        }
                        if (job_info != null)
                        {
                            pushToListCenterTask(task.Serial, job_info);
                            com.GetSocketJobWorker().PushJobInfo(job_info);
                        }

                    }
                    Console.WriteLine("[Socket - RES_GET_LIST_TASK] => reponse : " + json);
                }
                else if (data_json.Contains("RES_CHECK_SUBSCRIPTION"))
                {
                    CheckSubscriptionResponse listTaskResponse = JsonConvert.DeserializeObject<CheckSubscriptionResponse>(o.data.ToString());
                    if (listTaskResponse.Data.force_to_logout)
                    {
                        MessageBox.Show(listTaskResponse.Message);
                        GlobalVar.IsApplicationExit = true;
                        Application.Exit();
                    }

                    if (listTaskResponse.Data.is_subscription != Client.GetCurrentAccount().Is_Subscription)
                    {
                        MessageBox.Show(listTaskResponse.Message);
                        GlobalVar.IsApplicationExit = true;
                        Application.Exit();
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[wsONMessage] exception : {ex.Message} {json}");
            }

        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            handleReconnect();
            GlobalEvent.OnJobLog($"Connection closed");

            Console.WriteLine($"OnClose() Event: {e.Code} - {e.Reason} - {e.WasClean} ");
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            GlobalEvent.OnJobLog($"[Error] Event: {e.Message} - {e.Exception}");

            Console.WriteLine($"OnError() Event: {e.Message} - {e.Exception} ");
        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            _jobBySerials = new Dictionary<string, Dictionary<int, BaseJobInfo>>();
            //SendGetListTask();
            GlobalEvent.OnJobLog($"Connection opened");

            Console.WriteLine($"OnOpen() Event ");
            initCheckSubscriptionThread();
            initIntervalPing();
        }
    }
}
