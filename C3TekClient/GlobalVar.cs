using C3TekClient.C3Tek;
using C3TekClient.GSM;
using C3TekClient.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using C3TekClient.Helper;

namespace C3TekClient
{
    public static class GlobalVar
    {

        public static string UD_CHECKVERSION_URL = "https://lamsim.biz/apis/home/CheckVersionStkV3";
        //public static string UD_CHECKVERSION_URL = "https://sgp1.digitaloceanspaces.com/s3w/version.json";
        public static bool __DEBUG_AUTOLOGIN = false;
        public static bool __ENABLE_CRYPT = true;
        public static bool __ENABLE_AUTHEN_CHECK_VERSION = true;


        private static IDictionary<string, int> listBuyModemDict = new Dictionary<string, int>();

        private static UserSetting _UserSetting { get; set; }
        public static UserSetting UserSetting
        {
            get
            {
                if (_UserSetting == null)
                {
                    string json = Properties.Settings.Default.UserSettings;
                    if (!string.IsNullOrEmpty(json))
                    {
                        _UserSetting = JsonConvert.DeserializeObject<UserSetting>(json);
                    }
                    else
                    {
                        _UserSetting = new UserSetting();
                        _UserSetting.Save();
                    }

                }
                return _UserSetting;
            }
            set { if (value != null) _UserSetting = value; }
        }

        public static bool _CheckBalanceAndPhone = UserSetting.CheckBalanceAutoMode;
        public static bool CheckBalanceAndPhone
        {
            get { return _CheckBalanceAndPhone; }
            set
            {
                GlobalVar.UserSetting.CheckBalanceAutoMode = value;
                _CheckBalanceAndPhone = value;
                UserSetting.Save(); 
            }

        }


        public static bool _CheckRegMyUseProxy = UserSetting.CheckRegMyUseProxy;
        public static bool _CheckRegManualImei = UserSetting.CheckManualImei;

        public static bool CheckRegMyUseProxy
        {
            get { return _CheckRegMyUseProxy; }
            set
            {
                GlobalVar.UserSetting.CheckRegMyUseProxy = value;
                _CheckRegMyUseProxy = value;
                UserSetting.Save();
            }

        }
        public static bool CheckRegManualImei
        {
            get { return _CheckRegManualImei; }
            set
            {
                GlobalVar.UserSetting.CheckManualImei = value;
                _CheckRegManualImei = value;
                UserSetting.Save();
            }
        }




        public static bool IsApplicationExit = false;
        public static bool AutoAnswerIncomingCall = true;
        public static bool EnableVoiceRecognitionToText = false;
        public static bool EnableIncomingCallRing = false;
        public static bool RealtimeSMSTracking = true;
        public static bool EnableSMSRing = true;



        private static object AutoDashboardModeLock = new object();

        public static bool AutoDashboardMode = true;

        internal static bool EnablePlaySound = true; //when answer



        public static Dictionary<SIMCarrier, string> MapCarrier = new Dictionary<SIMCarrier, string>()
        {
            {SIMCarrier.NO_SIM_CARD, "NONE" },
            {SIMCarrier.Vinaphone ,  "VNP" },
            {SIMCarrier.Viettel , "VTT" },
            {SIMCarrier.Mobifone , "VMS" },
            {SIMCarrier.VietnamMobile, "VNM" },
            {SIMCarrier.DTAC , "DTAC"},
            {SIMCarrier.Metfone,"METFONE"}
        };
        public static Dictionary<ESubscriptionPackage, string> MapSubscriptionPackage = new Dictionary<ESubscriptionPackage, string>()
        {
            {ESubscriptionPackage.NONE, "NO_SUB" },
            {ESubscriptionPackage.SUB_BASIC ,  "BASIC" },
            {ESubscriptionPackage.SUB_DELUX , "DELUX" },
            {ESubscriptionPackage.NO_ACCEPT, "MOD" },
            {ESubscriptionPackage.ADMIN, "ADMIN" },
        };

        public static void ToggleAutoDashboardMode()
        {
            lock (AutoDashboardModeLock)
            {
                AutoDashboardMode = !AutoDashboardMode;
            }
        }
        public static void setAutoDashboardMode(bool state)
        {
            lock (AutoDashboardModeLock)
            {
                AutoDashboardMode = state;
            }
        }


        public static void ForceKillMyself()
        {
            Process.GetCurrentProcess().Kill();
        }

    }


   
    public static class AppPermissionMiddleware
    {
        public static Dictionary<EFeaturePermission, ESubscriptionPackage> NoModemPermission = new Dictionary<EFeaturePermission, ESubscriptionPackage>()
        {
            { EFeaturePermission.NONE , ESubscriptionPackage.NONE},
            { EFeaturePermission.SEND_SMS_MANUAL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.SEND_SMS_EXCEL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.SEND_SMS_WEB , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.RECEIVE_SMS_MANUAL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.RECEIVE_SMS_AUTO , ESubscriptionPackage.NONE},
            { EFeaturePermission.RECEIVE_SMS_WEB , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.SINGLE_USSD , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.MULTI_PORT_SINGLE_USSD , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.MULTI_PORT_SINGLE_USSD_WEB , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.SINGLE_MULTI_USSD , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.MULTI_PORT_MULTI_USSD , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.MULTI_PORT_WEB_MULTI_USSD , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CREATE_MY , ESubscriptionPackage.NONE},
            { EFeaturePermission.CALL_OUT_ONE_PHONENUMBER , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.CALL_OUT_EXCEL , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CALL_OUT_WEB , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CALL_OUT_PLAY_AUDIO , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.RECEIVE_AND_ACCEPT , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.RECEIVE_AND_ACCEPT_RECORD , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CONSUME_DATA , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CHANGE_IMEI , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.CONSUME_DATA_WEB , ESubscriptionPackage.NO_ACCEPT},
            { EFeaturePermission.SHOW_POPUP , ESubscriptionPackage.NONE},
        };

        public static Dictionary<EFeaturePermission, ESubscriptionPackage> ModemPermission = new Dictionary<EFeaturePermission, ESubscriptionPackage>()
        {
            { EFeaturePermission.NONE , ESubscriptionPackage.NONE},
            { EFeaturePermission.SEND_SMS_MANUAL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.SEND_SMS_EXCEL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.SEND_SMS_WEB , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.RECEIVE_SMS_MANUAL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.RECEIVE_SMS_AUTO , ESubscriptionPackage.NONE},
            { EFeaturePermission.RECEIVE_SMS_WEB , ESubscriptionPackage.SUB_DELUX},

            { EFeaturePermission.SINGLE_USSD , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.MULTI_PORT_SINGLE_USSD , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.MULTI_PORT_SINGLE_USSD_WEB , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.SINGLE_MULTI_USSD , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.MULTI_PORT_MULTI_USSD , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.MULTI_PORT_WEB_MULTI_USSD , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.CREATE_MY , ESubscriptionPackage.NONE},
            { EFeaturePermission.CALL_OUT_ONE_PHONENUMBER , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.CALL_OUT_EXCEL , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.CALL_OUT_WEB , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.CALL_OUT_PLAY_AUDIO , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.RECEIVE_AND_ACCEPT , ESubscriptionPackage.SUB_BASIC},
            { EFeaturePermission.RECEIVE_AND_ACCEPT_RECORD , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.CONSUME_DATA , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.CHANGE_IMEI , ESubscriptionPackage.NONE},
            { EFeaturePermission.CONSUME_DATA_WEB , ESubscriptionPackage.SUB_DELUX},
            { EFeaturePermission.SHOW_POPUP , ESubscriptionPackage.NONE},
        };

        public static bool hasAccessFeature(EFeaturePermission feature)
        {
            return (Client.isBuyModem) ? (ModemPermission[feature] <= Client.SubcriptionPackage) : NoModemPermission[feature] <= Client.SubcriptionPackage;
        }
    }
    public class AppModeSetting
    {
        //SMS setting
        protected static bool _AutoTrackingSMS = true;

        protected static bool _OneTimeMode = false;
        //Call setting
        protected static bool _AutoAnswerCall = true;
        protected static bool _AutoRecordCall = false;
        protected static bool _AutoPlayAudioAnswer = false;
        protected static int _TimeToEndAnswer = 15;
        

        public static string Locale { get; set; }

        public static ResourceManager MyResource = new ResourceManager(typeof(MyResource));
        public static CultureInfo CurrentCultureInfo { get; set; }

        public static bool OneTimeMode
        {
            get { return _OneTimeMode;}
            set { _OneTimeMode = value; }
        }
        public static int TimeToEndAnswer
        {
            get
            {
                return _TimeToEndAnswer;
            }
            set
            {
                if(value > 0 && value < 20 * 60 *1000) { 
                    _TimeToEndAnswer = value;
                }
                else
                {
                    _TimeToEndAnswer = 15;
                }
            }
        }
        public static bool AutoTrackingSMS
        {
            get
            {
                return _AutoTrackingSMS; 
            }
            set
            {
                //neu bat auto
                if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_SMS_AUTO) && value == true)
                {
                    _AutoTrackingSMS = value;
                }
                else
                {
                    _AutoTrackingSMS = false; 
                }
            }
        }

        public static bool AutoRecordCall
        {
            get
            {
                return _AutoRecordCall;
            }
            set
            {
                if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT))
                {
                    _AutoRecordCall = value;
                }
                else
                {
                    _AutoRecordCall = false;
                }
            }
        }
        public static bool AutoAnswerCall
        {
            get
            {
                return _AutoAnswerCall;
            }
            set
            {
                if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT))
                {
                    _AutoAnswerCall = value;
                }
                else
                {
                    _AutoAnswerCall = false;
                }
            }
        }

        public static bool AutoPlayAnswerAudio
        {
            get
            {
                return _AutoPlayAudioAnswer;
            }
            set
            {
                if (AppPermissionMiddleware.hasAccessFeature(EFeaturePermission.RECEIVE_AND_ACCEPT))
                {
                    _AutoPlayAudioAnswer = value;
                }
                else
                {
                    _AutoPlayAudioAnswer = false;
                }
            }
        }

    }




    public class UserSetting
    {
        public List<ManualPortName> ManualPortNames = new List<ManualPortName>();
        public List<ProxyItem> ListProxyItems = new List<ProxyItem>();

        public ProxyItem GetRandomProxyItem()
        {
            if (ListProxyItems == null || ListProxyItems.Count == 0)
            {
                return null;
            }
            var random = new Random();
            int index = random.Next(ListProxyItems.Count);
            return ListProxyItems[index]; 
            
        }
        public bool RememberLogin { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool CheckBalanceAutoMode { get; set; }

        public bool CheckRegMyUseProxy { get; set; }

        public bool CheckManualImei { get; set; }


        public string Locale { get; set; }

        public void Save()
        {
            if (this != null)
            {
                string jsonSetting = JsonConvert.SerializeObject(this);
                Properties.Settings.Default.UserSettings = jsonSetting;
                Properties.Settings.Default.Save();
            }
        }


    }

    public class ProxyItem
    {
        public string Host { get; set; }
        public string Protocol { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public  IWebProxy GetProxy()
        {
           return UtilHelper.GetProxy(Protocol + Host , Port , Username, Password);
        }
        public static ProxyItem ParseProxyFromString(string line)
        {
            Regex regexPattern = new Regex(@"([http|https|socks4|socks5]+\:\/\/)?(.*?):(.*?):(.*?):(.*?)$");

            Match match = regexPattern.Match(line);
            if (match.Success)
            {
                string protocol = match.Groups[1].Value ?? "";
                string host = match.Groups[2].Value ?? "";
                string port = match.Groups[3].Value ?? "80";
                string username = match.Groups[4].Value ?? "";
                string password = match.Groups[5].Value ?? "";
                if (string.IsNullOrEmpty(protocol))
                {
                    if (port == "80") protocol = "http://";
                    if (port == "443") protocol = "https://";
                }

                int portInt = Int32.Parse(port);
                return new ProxyItem()
                {
                    Protocol = protocol,
                    Host = host, 
                    Port = portInt,
                    Username = username,
                    Password =password
                };

            }
            else
            {
                return null;
            }
        }
        public override string ToString()
        {
            return $"{Protocol ?? "http://"}{Host ?? ""}:{Port}:{Username}:{Password}";
        }
    }

    public class ManualPortName
    {
        public string WindowsPortName { get; set; }
        public string UserPortName { get; set; }
    }

}
