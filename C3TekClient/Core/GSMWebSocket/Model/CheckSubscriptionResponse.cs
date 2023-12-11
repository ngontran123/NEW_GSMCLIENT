using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{

    public class CheckSubscriptionResponseData
    {
        [JsonProperty("force_to_logout")]
        public bool force_to_logout { get; set; }

        [JsonProperty("is_subscription")] //dang ki goi
        public bool is_subscription { get; set; }
        
        [JsonProperty("subscription_package")]
        public string subscription_package { get; set; }
    }
    
    public class CheckSubscriptionResponse
    {
        [JsonProperty("success")]
        public string Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("affect_key")]
        public string AffectKey { get; set; }

        [JsonProperty("data")]
        public CheckSubscriptionResponseData Data { get; set; }
    }
}
