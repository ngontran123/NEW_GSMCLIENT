using DevExpress.Data.Filtering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace C3TekClient.C3Tek
{
    public class C3TekPortal
    {
        private static readonly string BaseAddress = "https://lamsim.biz/apis/";
        public string LastMessage { get; set; }
        private readonly string keyHex = "36333643363936353645373435463643363136443733363936443245363236393741";
        public readonly static string basicAuth = "YWRtaW46OGFjQjZTczdqUUM2cG5oQg==";
        public LoginInfo Login(string username, string password)
        {  

            LastMessage = string.Empty;
            var response = Post<ResponseDataTypeOf<LoginInfo>>("auth/login", new { phone = username, password = password });
            LastMessage = response.message;
            return response.data;
        }
        
        public int AddProxy()
        {
            
            string data = stringProxy();
            string[] d = data.Split(',');

            ProxyItem pi = new ProxyItem();
            pi.Protocol = "http://";
            int result = 0;

            for (int i = 0; i < 20; i++)
            {
                pi.Host = d[4 * i];
                pi.Port = Int32.Parse(d[4 * i + 1]);
                pi.Username = d[4 * i + 2];
                pi.Password = d[4 * i + 3];
                GlobalVar.UserSetting.ListProxyItems.Add(pi);
                //if(C3TekClient.Helper.UtilHelper.CheckProxy("https://" + d[4 * i], Int32.Parse(d[4 * i + 1]), d[4 * i + 2], d[4 * i + 3]))
                //{
                    result++;
                //}
            }
            
            GlobalVar.UserSetting.Save();
            return result;
        }

        public string stringProxy()
        {
            ProxyItem result = null;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicAuth},Bearer {Client.AccessToken}");
                var request = client.GetAsync($"{BaseAddress}proxy/list").Result;
                string response = request.Content.ReadAsStringAsync().Result;
                string dataEncrypted = JObject.Parse(response).SelectToken("data").Value<string>();
                string dataJson = Decrypt(dataEncrypted);
                string data =dataJson.Substring(dataJson.IndexOf("["));
                Regex reg = new Regex("[^A-Za-z0-9,.]");
                string re = reg.Replace(data, "");
                return re;
                
            }    
        }

        public bool AddPhone(AddPhone phone, ref AddPhoneResponse res)
        {
            bool result = false;
            try
            {
                LastMessage = string.Empty;
                List<AddPhone> phones = new List<AddPhone>();
                phones.Add(phone);
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = 0,
                    ActionName = "Push Phone To Server (Before)",
                    ActionTime = DateTime.Now,
                    Note = ""
                });
                var response = Post<ResponseDataTypeOf<List<AddPhoneResponse>>>("telecom/add", phones);
                LastMessage = response.message;
                if (response.data != null && response.data.Any())
                    res = response.data.FirstOrDefault();
                    result = response.status_code;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = (response != null && response.data != null && response.data.Any()) ? response.data.FirstOrDefault().transaction_id : -1,
                    ActionName = "Push Phone To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = response.message
                });
            }
            catch (Exception ex)
            {
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = 0,
                    ActionName = "Error: Push Phone To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = ex.Message
                });
            }
            return result;
        }


        public bool InitRegMy(AddPhone phone, ref AddPhoneResponse res)
        {
            bool result = false;
            try
            {
                LastMessage = string.Empty;
                List<AddPhone> phones = new List<AddPhone>();
                phones.Add(phone);
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = 0,
                    ActionName = "Push Phone To Server (Before)",
                    ActionTime = DateTime.Now,
                    Note = ""
                });
                var response = Post<ResponseDataTypeOf<List<AddPhoneResponse>>>("telecom/regMy", phones);
                LastMessage = response.message;
                //string dataJson = Decrypt(response.message);
                //MessageBox.Show(dataJson);
                if (response.data != null && response.data.Any())
                    res = response.data.FirstOrDefault();
                result = response.status_code;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = (response != null && response.data != null && response.data.Any()) ? response.data.FirstOrDefault().transaction_id : -1,
                    ActionName = "Push Phone To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = response.message
                });
            }
            catch (Exception ex)
            {
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone.phone,
                    OrderID = 0,
                    ActionName = "Error: Push Phone To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = ex.Message
                });
            }            
            return result;
        }

        public bool UpdateRegMy(UpdateTelecom updateTelecomData)
        {
            bool result = false;
            AddPhoneResponse res;
            try
            {
                LastMessage = string.Empty;
               
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = updateTelecomData.phone,
                    OrderID = 0,
                    ActionName = "Update status My (Before)",
                    ActionTime = DateTime.Now,
                    Note = "" 
                });
                List<UpdateTelecom> listUpdateTelecoms = new List<UpdateTelecom>();
                listUpdateTelecoms.Add(updateTelecomData);
                var response = Post<ResponseDataTypeOf<List<AddPhoneResponse>>>("telecom/regMy", listUpdateTelecoms);
                LastMessage = response.message; 
                if (response.data != null && response.data.Any())
                    res = response.data.FirstOrDefault();
                result = response.status_code;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = updateTelecomData.phone,
                    OrderID = (response != null && response.data != null && response.data.Any()) ? response.data.FirstOrDefault().transaction_id : -1,
                    ActionName = "Push status My (After)",
                    ActionTime = DateTime.Now,
                    Note = response.message
                });
            }
            catch (Exception ex)
            {
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = updateTelecomData.phone,
                    OrderID = 0,
                    ActionName = "Error: Update status My (After)",
                    ActionTime = DateTime.Now,
                    Note = ex.Message
                });
            }
            return result;
        }


        public bool AddPhones(List<AddPhone> phoneCollection)
        {
            LastMessage = string.Empty;
            var response = Post<ResponseNoData>("telecom/add", phoneCollection);
            LastMessage = response.message;
            return response.status_code;
        }
        public bool LogAllSMS(string sender, string receiver, string content, string carrier,string date)
        {
            bool result = false;

            try
            {
                var response = Post<ResponseNoData>("sms/add", new { sms_sender = sender, sim_phone_number = receiver, sms_content = content , carrier = carrier });
                LastMessage = response.message;
                result = response.status_code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[LogAllSMS API - Exception]:  " + ex.Message);
                result = false;
            }
            return result;
        }
        public bool AddSMS(string sender, string receiver, string content, int orderid, string carrier)
        {
            bool result = false;
            try
            {
                LastMessage = string.Empty;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = receiver,
                    OrderID = orderid,
                    ActionName = "Push SMS To Server (Before)",
                    ActionTime = DateTime.Now,
                    Note = $"Sender: {sender}, Receiver: {receiver}, Content: {content}"
                });
                var response = Post<ResponseNoData>("sms/add", new { sms_sender = sender, sim_phone_number = receiver, sms_content = content , carrier =carrier });
                LastMessage = response.message;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = receiver,
                    OrderID = orderid,
                    ActionName = "Push SMS To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = response.message
                });
                result = response.status_code;
            }
            catch (Exception ex)
            {
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = receiver,
                    OrderID = orderid,
                    ActionName = "Error: Push SMS To Server (After)",
                    ActionTime = DateTime.Now,
                    Note = ex.Message
                });
            }
            return result;
        }
        public List<CallbackResultTracking> ResultTracking(List<int> transactionIdCollection)
        {
            List<CallbackResultTracking> result = new List<CallbackResultTracking>();
            LastMessage = string.Empty;
            var response = Post<ResponseDataTypeOf<List<CallbackResultTracking>>>("telecom/callback/phone", transactionIdCollection);
            LastMessage = response.message;
            result = response.data;
            return result;
        }

        public List<CallbackResultTracking> ResultTracking(int transactionId, string phone)
        {
            List<CallbackResultTracking> result = new List<CallbackResultTracking>();
            try
            {

                LastMessage = string.Empty;
                List<int> transactionIdCollection = new List<int>();
                transactionIdCollection.Add(transactionId);
                var response = Post<ResponseDataTypeOf<List<CallbackResultTracking>>>("telecom/callback/phone", transactionIdCollection);
                LastMessage = response.message;
                result = response.data;
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone,
                    OrderID = transactionId,
                    ActionName = "Tracking Result",
                    ActionTime = DateTime.Now,
                    Note = $"{response.message} - Trạng thái: {response.data.FirstOrDefault().status}"
                });
            }
            catch (Exception ex)
            {
                RequestLoggerCenter.AddLog(new RequestLogger()
                {
                    Phone = phone,
                    OrderID = transactionId,
                    ActionName = "Tracking Result",
                    ActionTime = DateTime.Now,
                    Note = "ERROR: " + ex.Message
                });
            }
            return result;
        }
        public bool UpdateTTTB(TTTB tttb)
        {
            bool result = false;
            LastMessage = string.Empty;
            var response = Post<ResponseNoData>("sms/UpdateTttb", tttb);
            LastMessage = response.message;
            result = response.status_code;
            RequestLoggerCenter.AddLog(new RequestLogger() { Phone = tttb.phone, OrderID = 0, ActionName = "Update TTTB", ActionTime = DateTime.Now, Note = response.message });
            return result;
        }

       
        public BalanceInfo BalanceTracking()
        {
            BalanceInfo result = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicAuth},Bearer {Client.AccessToken}");
                var request = client.GetAsync($"{BaseAddress}user/balance").Result;
                string response = request.Content.ReadAsStringAsync().Result;
                string dataEncrypted = JObject.Parse(response).SelectToken("data").Value<string>();
                string dataJson = Decrypt(dataEncrypted);
                result = JObject.Parse(dataJson).SelectToken("data").ToObject<BalanceInfo>();
            }
            return result;
        }

        public string VoiceRecognitionToText(byte[] audio)
        {
            return string.Empty;
        }

        public void Feedback(string content)
        {
            try
            {
                //LastMessage = string.Empty;
                var response = Post<ResponseNoData>("user/AddFeedback", new { content = content });
                RequestLoggerCenter.AddLog(new RequestLogger() { Phone = "", OrderID = 0, ActionName = "Feedback", ActionTime = DateTime.Now, Note = response.message });
                //LastMessage = response.message;
            }
            catch { }
        }
        public VersionInfo GetLastestVersion()
        {
            VersionInfo result = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicAuth},Bearer {Client.AccessToken}");
                var request = client.GetAsync($"{BaseAddress}home/CheckVersionV3").Result;
                string response = request.Content.ReadAsStringAsync().Result;
                string dataEncrypted = JObject.Parse(response).SelectToken("data").Value<string>();
                string dataJson = Decrypt(dataEncrypted);
                result = JObject.Parse(dataJson).SelectToken("data").ToObject<VersionInfo>();
            }
            return result;
        }

        private TResponse Post<TResponse>(string endpoint, object data)
        {
            var result = default(TResponse);
            using (HttpClient client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(Client.AccessToken))
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicAuth},Bearer {Client.AccessToken}");
                else
                {
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {basicAuth}");
                }
                Dictionary<string, string> body = new Dictionary<string, string>();
                body.Add("data", Encrypt(JsonConvert.SerializeObject(data)));
                var request = client.PostAsync($"{BaseAddress}{endpoint}", new FormUrlEncodedContent(body)).Result;
                var response = request.Content.ReadAsStringAsync().Result;
                string dataEncrypted = JObject.Parse(response).SelectToken("data").Value<string>();
                string dataJson = Decrypt(dataEncrypted);
                //MessageBox.Show(dataJson);
                result = JsonConvert.DeserializeObject<TResponse>(dataJson);
            }
            return result;
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
    }

    public class VersionInfo
    {
        public string version_code { get; set; }
        public string version_name { get; set; }
        public string version_date { get; set; }
        public string description { get; set; }
        public string product_code { get; set; }
        public string product_name { get; set; }
        public string url_download { get; set; }
        public bool is_latest { get; set; }
        public string version_type { get; set; }
        public bool force_upgrade { get; set; }
    }

    public class BalanceInfo
    {
        public int amount { get; set; }
        public BalanceQuota quota { get; set; }
    }

    public class BalanceQuota
    {
        public int VMS { get; set; }
        public int VNP { get; set; }
        public int VTT { get; set; }
        public int VNM { get; set; }

    }
    public class ResponseDataTypeOf<DataType> : ResponseNoData
    {
        public DataType data { get; set; }
    }
    public class ResponseNoData
    {
        public int code { get; set; }
        public bool status_code { get; set; }
        public string message { get; set; }
    }
    public class AddPhoneResponse
    {
        public string phone { get; set; }
        public string serial { get; set; }
        public int transaction_id { get; set; }
    }
    public class LoginInfo
    {
        public string token { get; set; }
        public LoginUserInfo data { get; set; }
    }
    //"{\"code\":200,\"status_code\":true,\"message\":\"Login th\\u00e0nh c\\u00f4ng\",\"data\":{\"token\":\"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJodHRwczpcL1wvbGFtc2ltLmJpelwvYXBpc1wvYXV0aFwvbG9naW4iLCJpYXQiOjE2MjE5NzA3NjEsImV4cCI6MTYyMjU3NTU2MSwibmJmIjoxNjIxOTcwNzYxLCJqdGkiOiJKbGJrTE1KYkhKSnRkZjJvIiwic3ViIjoyMjIsInBydiI6IjIzYmQ1Yzg5NDlmNjAwYWRiMzllNzAxYzQwMDg3MmRiN2E1OTc2ZjcifQ.ZL8W-rgYR7SPdaf3Cy59ZuSuozDn6jo23qpu5eILePI\",\"data\":{\"name\":\"Tr\\u1ea7n Minh \\u0110\\u1ee9c\",\"phone\":\"\",\"amount\":,\"is_buy_modem\":1,\"type_modem\":\"EC20\",\"message_qc\":\"\",\"is_subscription\":false,\"subscription_package\":\"blank\"}}}"

   
    public class ListBuyModemEntry
    {
        public string chip { get; set; }
        public int total { get; set; }

    }
    public class LoginUserInfo
    {
        public string name { get; set; }
        public string phone { get; set; }
        public int amount { get; set; }
        public string type_model { get; set; }
        public string message_qc { get; set;  }
        
        public bool is_subscription { get; set; }
        public string subscription_package { get; set; }
        public bool is_buy_modem { get; set; }
        
        public bool is_other_user { get; set; }
        public bool is_stk { get; set; }

        public List<ListBuyModemEntry> list_buy_modem { get; set; }

        
    }

    public class AddPhone
    {
        public string phone { get; set; }
        public string serial { get; set; }

        [JsonProperty("telecom")]
        public string carrier { get; set; }
        public string com { get; set; }
        public string password { get; set; }

    }

    public class UpdateTelecom
    {
        public string com { get; set; }

        public string phone { get; set; }

        [JsonProperty("telecom")]
        public string carrier { get; set; }
        public string serial { get; set; }
        public int transaction_id { get; set; }
        public int status { get; set; }
        public string message { get; set; }

        

    }

    public class UpdateTelecomVT : UpdateTelecom
    {
        public string com { get; set; }

        public string phone { get; set; }

        [JsonProperty("telecom")]
        public string carrier { get; set; }
        public string serial { get; set; }
        public int transaction_id { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        
        public string token { get; set; }
        public string check_sum { get; set; }

        [JsonProperty("device_id")]
        public string device_id { get; set; }
        public string key_device_acc { get; set; }
        public string key_refresh { get; set; }
        public string key_refresh_finger_print { get; set; }

    }
    public class CallbackResultTracking
    {
        public string phone { get; set; }
        public int status { get; set; }
        public string password { get; set; }
        public string message { get; set; }
    }
    public class TTTB
    {
        public string phone { get; set; }
        public string full_name { get; set; }
        public string cmnd { get; set; }
        public string address_cmnd { get; set; }
        public string date_cmnd { get; set; }
        public string type_tb { get; set; }
        public string birthday { get; set; }
        public string full_content { get; set; }
        public string date_active { get; set; }
    }
}
