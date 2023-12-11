using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM.Model
{
    public class CustomAdvanceUSSDBinding
    {
        //public string RealPort { get; set; }
        public string AppPort { get; set; }
        public string PhoneNumber { set; get; }
        public string USSD { set; get; }
        public string USSDResult { get; set; }
        public int DelayTime { get; set; }
        public string Status { get; set; }
    }
    public class TopupUSSDBinding : CustomAdvanceUSSDBinding
    {
        public string MaTT { get; set; } 
    }
}
