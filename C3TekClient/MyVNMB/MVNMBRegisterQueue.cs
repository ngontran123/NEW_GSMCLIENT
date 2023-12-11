using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.MyVNMB
{
    public class MVNMBRegisterQueue
    {
        public DateTime QueueTime { get; set; }
        public string PhoneNumber { get; set; }
        public bool Resolved { get; set; }
        public MVNMBRegisterQueueState QueueState { get; set; }
        public string COM { get; set; }
        public string Serial { get; set; }
        public string Carrier { get; set; }
    }
    public enum MVNMBRegisterQueueState
    {
        None,
        Processing,
        Failed,
        Succeed,
    }
}
