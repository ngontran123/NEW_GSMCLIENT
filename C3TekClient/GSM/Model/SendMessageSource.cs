using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM.Model
{
    public enum ESendMessageSource
    {
        WAITING = 1,
        SENDING = 2,
        SENT_SUCESS = 3,
        SENT_FAIL = 4
    }
    public class SendMessageSource
    {
        public string RealPort { get; set; }
        public string AppPort { get; set; }
        public string PhoneNumber { set; get; }
        public string PhoneTo { get; set; }
        public string Body { get; set; }
        public string Status { get; set; }
        public bool Resolved { get; set; }
        public ESendMessageSource State { get; set; }
    }
}
