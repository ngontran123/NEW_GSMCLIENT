using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.C3Tek
{
    public class C3TekSMSModel
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Content { get; set; }

    }
    public class C3TekListSMSModel
    {
        public List<C3TekSMSModel> list_sms { get; set; }
    }
}
