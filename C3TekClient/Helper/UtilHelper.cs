using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace C3TekClient.Helper
{
    class UtilHelper
    {

        public static async Task<string> GetPublicIP()
        {
            string ip = string.Empty;
            try
            {
                var httpClient = new HttpClient();
                ip = await httpClient.GetStringAsync("https://api.ipify.org");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error when get ip");
            }
            return ip;
        }
        
        public static bool CheckProxy(string proxyUrl, int port, string  username, string password)
        {
            try
            {
                var proxy = GetProxy(proxyUrl, port, username, password);
                if (proxy == null)
                {
                    return false;
                }
                var req = (HttpWebRequest) HttpWebRequest.Create("http://ip-api.com/json");
                req.Proxy = proxy; 
                
                var resp = req.GetResponse();
                var json = new StreamReader(resp.GetResponseStream()).ReadToEnd();

                var myip = (string) JObject.Parse(json)["query"];
                
                if (!string.IsNullOrEmpty(myip))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;

            }

            return false;
        }

        public static System.Net.IWebProxy GetProxy(String proxyURL, int port, String username, String password)
        {
            //Validate proxy address
            try
            {
                var proxyURI = new Uri(string.Format("{0}:{1}", proxyURL, port));

                //Set credentials
                ICredentials credentials = new NetworkCredential(username, password);

                //Set proxy
                return new WebProxy(proxyURI, true, null, credentials);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
