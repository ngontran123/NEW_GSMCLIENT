using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.MyVNPT
{
    public class MVNPTRegisterQueue
    {
        public DateTime QueueTime { get; set; }
        public string PhoneNumber { get; set; }
        public bool Resolved { get; set; }
        public MVNPTRegisterQueueState QueueState { get; set; }
        public string COM { get; set; }
        public string Serial { get; set; }
        public string Carrier { get; set; }
    }
    public enum MVNPTRegisterQueueState
    {
        None,
        Processing,
        Failed,
        Succeed,
    }
}
