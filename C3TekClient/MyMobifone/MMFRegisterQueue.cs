using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.MyMobifone
{
    public class MMFRegisterQueue
    {
        public DateTime QueueTime { get; set; }
        public string PhoneNumber { get; set; }
        public bool Resolved { get; set; }
        public MMFRegisterQueueState QueueState { get; set; }
        public string COM { get; set; }
        public string Serial { get; set; }
        public string Carrier { get; set; }
    }
    public enum MMFRegisterQueueState
    {
        None,
        Processing,
        Failed,
        Succeed,
    }
}
