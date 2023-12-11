using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class UpdateTaskData
    {
        [JsonProperty("task_id")]
        public int task_id { get; set; }

        [JsonProperty("status")]
        public int status { get; set; }


        [JsonProperty("message_comment")]
        public string message_comment { get; set; }


        [JsonProperty("message_completed")]
        public string message_completed { get; set; }
    }
    public class UpdateTaskRequest : BaseGSMWebSocketRequest
    {
        [JsonProperty("data")]
        public UpdateTaskData Data { get; set; }
        public UpdateTaskRequest()
        {
        }
    }
}
