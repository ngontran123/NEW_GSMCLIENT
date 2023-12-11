using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class SMSJobBody : BaseJobBody
    {
        public string To = "";
        public string Message = "";
        public SMSJobBody(string _to, string _message)
        {
            this.To = _to;
            this.Message = _message; 
        }
    }
}
