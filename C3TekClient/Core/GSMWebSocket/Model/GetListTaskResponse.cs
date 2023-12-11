using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class GetListTaskDetail
    {
        [JsonProperty("TASK_ID")]
        public int TASK_ID { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("serial")]
        public string Serial { get; set; }


        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("message_comment")]
        public string MessageComment { get; set; }

        [JsonProperty("message_completed")]
        public string MessageCompleted { get; set; }

    }
    public class GetListTaskResponse 
    {
        [JsonProperty("data")]
        public List<GetListTaskDetail> Data { get; set; }
        public GetListTaskResponse()
        {
            Data = new List<GetListTaskDetail>();
        }
    }
}
