using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class ConsumeDataJobBody : BaseJobBody
    {
        public string package = "";
        public ConsumeDataJobBody(string package)
        {
            this.package = package; 
        }
        public string getNumPackageOnly()
        {
            string pk =Helper.StringHelper.ParseDigitString(this.package);
            return (string.IsNullOrEmpty(pk) ? "5" : pk); 
        }
    }
}
