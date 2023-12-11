using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class GSMWebSocketResponseData
    {
        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("affect_key")]
        public string AffectKey{ get; set; }

       
    }
    public class GSMWebSocketResponse
    {
        [JsonProperty("data")]
        public GSMWebSocketResponseData Data;
    }
}
