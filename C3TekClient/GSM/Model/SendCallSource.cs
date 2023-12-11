using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM.Model
{
    public enum ESendCallSource { 
        WAITING = 1,
        CALLING =2,
        CALLED_SUCCESS =3 ,
        CALLED_FAIL = 4
    }
    public class SendCallSource
    {
        public string RealPort { get; set; }
        public string AppPort { get; set; }
        public string PhoneNumber { set; get; }
        public string PhoneTo { get; set; }
        public string Status { get; set; }
        public bool Resolved { get; set; }
        public ESendCallSource State { get; set; }

    }
}
