using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class USSDJobBody : BaseJobBody
    {
        public string command = "";
        public USSDJobBody(string _command)
        {
            this.command = _command; 
        }
    }
}
