using C3TekClient.Core.GSMWebSocket.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    class DrawPortData
    {
        [JsonProperty("comDraw")]
        public string comDraw { get; set; }
    }
    class DrawPortRequest : BaseGSMWebSocketRequest
    {
        [JsonProperty("data")]
        public DrawPortData data { get; set; }
    }
}
