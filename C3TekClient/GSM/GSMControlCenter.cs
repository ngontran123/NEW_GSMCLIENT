using C3TekClient.C3Tek;
using C3TekClient.Core.GSMWebSocket;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace C3TekClient.GSM
{


    public static class GSMControlCenter
    {
        public static Action<GSMMessage> OnNewMessage = (data) => { };
        /*
         *    lsModems.Add("M26", 0);
            lsModems.Add("EC20F", 0);
            lsModems.Add("MC55", 0);
            lsModems.Add("WaveCom", 0);
            lsModems.Add("SIMCOM_SIM5320E", 0);
         */

        public static string M26 = "M26";
        public static string EC20F = "EC20F";
        public static string MC55 = "MC55";
        public static string WaveCom = "WaveCom";
        public static string SIMCOM_SIM5320E = "SIMCOM_SIM5320E";
        public static string UC20 = "UC20";
        public static Thread PortHandler { get; set; }
        private static bool Stop = false;
        // private static bool AlertEC20 = false;
        public static void Dispose()
        {
            Stop = true;
            foreach (var gsmCom in GSMComs)
            {
                try
                {
                    gsmCom.Dispose();
                }
                catch { }
            }
        }
        public static BindingList<GSMCom> GSMComs = new BindingList<GSMCom>();
        public static WebSocketClient webSocketClient;
        public static BindingList<GSMMessage> GSMMessages = new BindingList<GSMMessage>();
        private static object lockGSMComs = new object();


        public static void MyRegisterNotifySuccess(string phone, MyRegisterState myRegisterState, string message)
        {
            lock (lockGSMComs)
            {
                var com = GSMComs.FirstOrDefault(_com => _com.PhoneNumber == phone);
                if (com != null && com.IsPortConnected && com.IsSIMConnected)
                {
                    com.MyRegisterState = myRegisterState;
                    com.MyProcessMessage = message;
                }
            }
        }

        public static object LockGSMMessages = new object();
        private static IDictionary<string, int> lsModems = new Dictionary<string, int>();

        public static void resetModemStats()
        {
            lsModems.Clear();
            lsModems.Add("M26", 0);
            lsModems.Add("EC20F", 0);
            lsModems.Add("MC55", 0);
            lsModems.Add("WaveCom", 0);
            lsModems.Add("SIMCOM_SIM5320E", 0);
            lsModems.Add("UC20", 0);
        }

        public static bool checkRuleNumberModem()
        {
            List<ListBuyModemEntry> entries = Client.GetCurrentAccount().ListBuyModem;
            IDictionary<string, int> entry_map = new Dictionary<string, int>();
            
            
            foreach(ListBuyModemEntry en in entries)
            {
                entry_map.Add(en.chip, en.total);
            }

            
            foreach(KeyValuePair<string, int> kvp in lsModems) {
                if(kvp.Value > 0)
                {
                    //Có dòng modem trong máy
                    if(!entry_map.ContainsKey(kvp.Key) ||entry_map[kvp.Key]  == 0)
                    {
                        //mà dữ liệu server không có modem loại này
                        //MessageBox.Show($"entries: {JsonConvert.SerializeObject(entries)} || entry_map {JsonConvert.SerializeObject(entry_map)}, key {kvp.Key}");

                        return false;
                    }
                    if(entry_map.ContainsKey(kvp.Key) && kvp.Value > entry_map[kvp.Key])
                    {
                        //Có dòng modem này tuy nhiên thực số modem máy  > số modem mua
                        //MessageBox.Show($"entries: {JsonConvert.SerializeObject(entries)} || entry_map {JsonConvert.SerializeObject(entry_map)}, key {kvp.Key}");
                        
                        return false;
                    }
                }
            }
            return true;

        
        }
        public static void Start()
        {
            resetModemStats();
            PortHandler = new Thread(new ThreadStart(PortHanding));
            PortHandler.Start();
        }


        public class PortInfo
        {
            public string Name;
            public string Description;
        }

        // Method to prepare the WMI query connection options.
        public static ConnectionOptions PrepareOptions()
        {
            ConnectionOptions options = new ConnectionOptions();
            options.Impersonation = ImpersonationLevel.Impersonate;
            options.Authentication = AuthenticationLevel.Default;
            options.EnablePrivileges = true;
            return options;
        }

        // Method to prepare WMI query management scope.
        public static ManagementScope PrepareScope(string machineName, ConnectionOptions options, string path)
        {
            ManagementScope scope = new ManagementScope();
            scope.Path = new ManagementPath(@"\\" + machineName + path);
            scope.Options = options;
            scope.Connect();
            return scope;
        }

        // Method to retrieve the list of all COM ports.
        public static List<PortInfo> FindComPorts()
        {
            List<PortInfo> portList = new List<PortInfo>();
            ConnectionOptions options = PrepareOptions();
            ManagementScope scope = PrepareScope(Environment.MachineName, options, @"\root\CIMV2");

            // Prepare the query and searcher objects.
            ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode = 0");
            ManagementObjectSearcher portSearcher = new ManagementObjectSearcher(scope, objectQuery);

            using (portSearcher)
            {
                string caption = null;
                // Invoke the searcher and search through each management object for a COM port.
                foreach (ManagementObject currentObject in portSearcher.Get())
                {
                    if (currentObject != null)
                    {
                        object currentObjectCaption = currentObject["Caption"];
                        if (currentObjectCaption != null)
                        {
                            caption = currentObjectCaption.ToString();
                            if (caption.Contains("(COM"))
                            {
                                PortInfo portInfo = new PortInfo();
                                portInfo.Name = caption.Substring(caption.LastIndexOf("(COM")).Replace("(", string.Empty).Replace(")", string.Empty);
                                portInfo.Description = caption;
                                portList.Add(portInfo);
                            }
                        }
                    }
                }
            }
            return portList;
        }
        public static List<string> findAllPortName()
        {
            List<PortInfo> portInfos = FindComPorts();
            if (portInfos == null)
            {
                return new List<string>();
            }
            List<string> list_ports = portInfos.Select(person => person.Name).ToList();
            return list_ports;
        }

        private static int CurrentPortDisplayIndex = 1;




        public static void PortHanding()
        {
          
            
            try
            {
                webSocketClient = new WebSocketClient(GSMComs);
            }
            catch (Exception ex)
            {
                Console.WriteLine("WEbsocket init exception");
            }
            while (!Stop)
            {
                try
                {
                    string[] ports = SerialPort.GetPortNames();
                   

                    //                    if (!AlertEC20 && GSMComs.Any(com=>com.ModemName == "EC20F")){
                    //                        string guild_ec20_sim = @"Mỗi khi anh chị cần thay sim trên Module GSM EC20 cần:
                    //+ Tắt phần mềm C3TekClient đang mở
                    //+ GẮN SIM MỚI VÀO XONG, ĐỢI MODEM NHẬN SÓNG (tầm 15s - 20s)
                    //+ Mở smscaster auto detect để chắc chắn thiết bị nhận được sim
                    //+ Mở phần mềm C3TekClient và đợi cho các sim sẵn sàng để sử dụng
                    //";
                    //                        MessageBox.Show(guild_ec20_sim, "EC20F - Lưu ý khi dùng", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    //                        AlertEC20 = true;
                    //                    }
                   
                    if (ports != null && ports.Any())
                    {

                        foreach (string port in ports)
                        {
                            if (GSMComs.Any(com => com.PortName == port))
                            {
                                //Nếu port trong GSMcom   
                                continue;
                            }

                            //if (port != "COM24")
                            //{
                            //    continue;
                            //}

                            var portNameManual = GlobalVar.UserSetting.ManualPortNames.FirstOrDefault(pn => pn.WindowsPortName == port);

                            var gsmCom = new GSMCom()
                            {
                                DisplayName = portNameManual == null ? $"{R.S("port")} {CurrentPortDisplayIndex}" : portNameManual.UserPortName
                                ,
                                Log = false
                            };
                            GSMComs.Add(gsmCom);
                            gsmCom.Start(port);
                            if (portNameManual == null)
                                CurrentPortDisplayIndex++;
                        }

                        if (Client.GetCurrentAccount().Username != "0846889911" && Client.isBuyModem && !Client.GetCurrentAccount().IsOtherUser) { 
                            resetModemStats();
                            foreach (var gsmCom in GSMComs)
                            {
                                if (lsModems.ContainsKey(gsmCom.ModemName))
                                {
                                    lsModems[gsmCom.ModemName] += 1;
                                }
                            }
                            if (checkRuleNumberModem() == false)
                            {
                                //fail
                                foreach (var gsmCom in GSMComs)
                                {
                                    gsmCom.Stop = true;
                                }
                                GSMComs.Clear();
                                GlobalVar.IsApplicationExit = true;
                                Stop = true;
                                MessageBox.Show("Phát hiện gian lận trong hệ thống. Vui lòng gỡ các thiết bị GSM không mua từ C3Tek ra khỏi máy tính và khởi động lại phần mềm");
                                Application.Exit();
                            }
                        }
                    }
                    else
                    {
                        foreach (var gsmCom in GSMComs)
                        {
                            gsmCom.Stop = true;
                        }
                        GSMComs.Clear();
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine("Port connection" + ex.Message);
                }

                if (!Client.isBuyModem)
                {
                    Thread.Sleep(15000);
                }
                Thread.Sleep(1000);
            }
            webSocketClient.Close();
        }
    }
}
