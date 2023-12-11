using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class BaseGSMWebSocketRequest
    {
        [JsonProperty("action")]
        public string Action { get; set; }  

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }
    }
}
