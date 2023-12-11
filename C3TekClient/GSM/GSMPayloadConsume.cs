using C3TekClient.C3Tek;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM
{
    public class GSMPayload
    {
        public string URL { get; set; }
        public int size_mb { get; set; }

        public int TimeWaitTimeOutInSecond { get; set; }
    }
    public class GSMPayloadReponse

    {
        [JsonProperty("code")]

        public int code { get; set; }
        [JsonProperty("message")]

        public string message { get; set; }

        [JsonProperty("status_code")]
        public bool status_code { get; set; }

        [JsonProperty("data")]
        public List<GSMPayload> payload { get; set; }
    }
    public class GSMPayloadSchemasInstance
    {
        public static GSMPayloadSchemas _payloadInstance;

        public static async void loadSchemas()
        {
            _payloadInstance = new GSMPayloadSchemas();

            await _payloadInstance.getPayloadFromURLAsync("https://lamsim.biz/apis/gms/urlPsd");
            //if (_payloadInstance.payloads == null || _payloadInstance.payloads.Count() <= 0)
            //{
            //    await _payloadInstance.getPayloadFromLocal();
            //}
        }
        public static async void loadLocalSchemas()
        {
            if(_payloadInstance !=null && _payloadInstance.payloads.Count > 0)
            {
                return; 
            }
            _payloadInstance = new GSMPayloadSchemas();

            await _payloadInstance.getPayloadFromLocal();

        }
    }
    public class GSMPayloadSchemas
    {
        public Dictionary<string, GSMPayload> payloads;
        public GSMPayloadSchemas()
        {
            payloads = new Dictionary<string, GSMPayload>();
        }



        public async Task<Dictionary<string, GSMPayload>> getC3TekPayloadFromURLAsync(string url = "https://raw.githubusercontent.com/snowdence/dataset-public-payload/master/zero-file/schema.json")
        {
            payloads = new Dictionary<string, GSMPayload>();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var json = await httpClient.GetStringAsync(url);
                    GSMPayloadReponse _payload_response = JsonConvert.DeserializeObject<GSMPayloadReponse>(json);
                    if (_payload_response.status_code)
                    {
                        foreach (GSMPayload _p in _payload_response.payload)
                        {
                            //U_1MB
                            payloads.Add($"U_{_p.size_mb.ToString()}MB", _p);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return payloads;
        }
        public async Task<Dictionary<string, GSMPayload>> getPayloadFromURLAsync(string url = "https://raw.githubusercontent.com/snowdence/dataset-public-payload/master/zero-file/schema.json", bool useAuth = false)
        {
            payloads = new Dictionary<string, GSMPayload>();

            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Basic {C3TekPortal.basicAuth},Bearer {Client.AccessToken}");

                    var json = await httpClient.GetStringAsync(url);

                    string dataEncrypted = JObject.Parse(json).SelectToken("data").Value<string>();
                    string dataJson = new C3TekPortal().Decrypt(dataEncrypted);
                    //string dataURLJson = JObject.Parse(dataJson).SelectToken("data").ToString();
                    bool status = JObject.Parse(dataJson).SelectToken("status_code").Value<bool>();
                    if (status == false)
                    {
                        return null;
                    }
                    else
                    {
                        GSMPayloadReponse _payload_response = JsonConvert.DeserializeObject<GSMPayloadReponse>(dataJson);

                        foreach (GSMPayload _p in _payload_response.payload)
                        {
                            //U_1MB
                            payloads.Add($"U_{_p.size_mb.ToString()}MB", _p);
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
            return payloads;
        }

        public async Task<Dictionary<string, GSMPayload>> getPayloadFromLocal(string url = "https://raw.githubusercontent.com/snowdence/dataset-public-payload/master/zero-file/schema.json")
        {
            payloads = new Dictionary<string, GSMPayload>();
            int[] list_size = { 1, 3, 5, 10, 25, 50 };
            int[] list_time = { 30, 60, 60, 150, 300, 600 };
            for (int i = 0; i < list_size.Length; i++)
            {
                payloads.Add($"U_{list_size[i].ToString()}MB", new GSMPayload()
                {
                    TimeWaitTimeOutInSecond = list_time[i],

                    size_mb = list_size[i],
                    URL = "https://raw.githubusercontent.com/snowdence/dataset-public-payload/master/zero-file/" + list_size[i].ToString()+ "mb.txt",
                    //URL = "https://github.com/snowdence/dataset-public-payload/raw/master/zero-file/" + list_size[i].ToString() + "mb.txt"

                });
            }
            return payloads;
        }
    }
}
