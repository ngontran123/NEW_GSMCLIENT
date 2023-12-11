using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using C3TekClient.C3Tek;
using C3TekClient.Core.GSMWebSocket.Model;
using DevExpress.XtraBars.ViewInfo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace C3TekClient.MyTelecom
{
    public class MyTelecomService
    {
        public const string MyVnptApi = "https://api-myvnpt.vnpt.vn/mapi/services/";
        public const string MyViettelApi = "https://apivtp.vietteltelecom.vn:6768/myviettel.php/";
        public const string MyVietnamobileApi = "https://www.vietnamobile.com.vn/";
        public const string MyMobifoneApi = "https://api.mobifone.vn/api/";

        private IWebProxy defaultWebProxy = null;
        public MyTelecomService()
        {

        }
        public MyTelecomService(IWebProxy webProxy)
        {
            this.defaultWebProxy = webProxy; 
        }

        
        private TResponse Post<TResponse>(string url, object data)
        {

            var result = default(TResponse);
            try
            {
                using (HttpClient client = new HttpClient())
                {

                    Dictionary<string, string> body = new Dictionary<string, string>();
                    var request = client.PostAsync(url, new FormUrlEncodedContent(body)).Result;
                    var response = request.Content.ReadAsStringAsync().Result;
                    result = JsonConvert.DeserializeObject<TResponse>(response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Exception] " + ex.Message);
            }
            return result;
        }
        public System.Net.IWebProxy GetProxy(String proxyURL, int port, String username, String password)
        {
            //Validate proxy address
            var proxyURI = new Uri(string.Format("{0}:{1}", proxyURL, port));

            //Set credentials
            ICredentials credentials = new NetworkCredential(username, password);

            //Set proxy
            return new WebProxy(proxyURI, true, null, credentials);
        }

        public MyTelecomService SetWebProxy(IWebProxy webProxy)
        {

            this.defaultWebProxy = webProxy; 
            return this;
            
        }
        public MyTelecomService SetWebProxy(ProxyItem webProxyItem)
        {

            this.defaultWebProxy = GetProxy(webProxyItem.Protocol + webProxyItem.Host, webProxyItem.Port , webProxyItem.Username, webProxyItem.Password);
            return this;
        }

        public T PostApiMyVNPT<T>(string entry_point, object data)
        {
            var result = default(T);
            bool useProxy = defaultWebProxy != null;
            try
            {
                var httpClientHandler = new HttpClientHandler() { UseProxy = useProxy };

                if (useProxy)
                {
                  
                    httpClientHandler.Proxy = defaultWebProxy; 
                }

                using (HttpClient client = new HttpClient(httpClientHandler))
                {

                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("okhttp", "4.7.1"));
                    client.DefaultRequestHeaders.Add("Host", "api-myvnpt.vnpt.vn");
                    //client.DefaultRequestHeaders.Add("Content-type", "application/json");
                    string json_data = JsonConvert.SerializeObject(data); 
                    var content = new StringContent(json_data, Encoding.UTF8, "application/json");

                    string url = $"{MyVnptApi}{entry_point}";
                    var request = client.PostAsync(url, content).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    JObject jobject = JObject.Parse(response);
                    result = JsonConvert.DeserializeObject<T>(jobject.ToString());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PostAPIVNPT Exception] " , ex.Message);
            } 
            return result;
        }


        public T PostApiMyViettel<T>(string entry_point, FormUrlEncodedContent data)
        {
            var result = default(T);
            bool useProxy = defaultWebProxy != null;
            try
            {
                var httpClientHandler = new HttpClientHandler() { UseProxy = useProxy };

                if (useProxy)
                {
                    httpClientHandler.Proxy = defaultWebProxy;
                }

                using (HttpClient client = new HttpClient(httpClientHandler))
                {

                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("okhttp", "4.7.1"));
                    client.DefaultRequestHeaders.Add("Host", "apivtp.vietteltelecom.vn");
                    //client.DefaultRequestHeaders.Add("Content-type", "application/json");
                    //string json_data = JsonConvert.SerializeObject(data);
                    //var content = new StringContent(json_data, Encoding.UTF8, "application/x-www-form-urlencoded");

                    string url = $"{MyViettelApi}{entry_point}";
                    var request = client.PostAsync(url, data).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    JObject jobject = JObject.Parse(response);
                    result = JsonConvert.DeserializeObject<T>(jobject.ToString());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PostAPIVNPT Exception] ", ex.Message);
            }
            return result;
        }


        public T PostApiMyMobi<T>(string entry_point, FormUrlEncodedContent data)
        {
            var result = default(T);
            bool useProxy = defaultWebProxy != null;
            try
            {
                var httpClientHandler = new HttpClientHandler() { UseProxy = useProxy };

                if (useProxy)
                {
                    httpClientHandler.Proxy = defaultWebProxy;
                }

                using (HttpClient client = new HttpClient(httpClientHandler))
                {

                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("okhttp", "4.7.1"));
                    client.DefaultRequestHeaders.Add("Host", "api.mobifone.vn");
                    client.DefaultRequestHeaders.Add("apisecret", "UEJ34gtH345DFG45G3ht1");
                    client.DefaultRequestHeaders.Add("appversion", "3.12.2 (178)");
                    client.DefaultRequestHeaders.Add("version_code", "178");
                    client.DefaultRequestHeaders.Add("version_name", "3.13.2");
                    //client.DefaultRequestHeaders.Add("Content-type", "application/json");
                    //string json_data = JsonConvert.SerializeObject(data);
                    //var content = new StringContent(json_data, Encoding.UTF8, "application/x-www-form-urlencoded");

                    string url = $"{MyMobifoneApi}{entry_point}";
                    var request = client.PostAsync(url, data).Result;
                    string response = request.Content.ReadAsStringAsync().Result;
                    JObject jobject = JObject.Parse(response);
                    result = JsonConvert.DeserializeObject<T>(jobject.ToString());
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[PostAPIVNPT Exception] ", ex.Message);
            }
            return result;
        }


    }
}
