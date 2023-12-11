using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class CallJobBody : BaseJobBody
    {
        public string Phone = "";
        public int Duration = 15;
        public CallJobBody(string _phone, int _duration)
        {
            this.Phone = _phone;
            this.Duration =  _duration; 
        }
    }
}
