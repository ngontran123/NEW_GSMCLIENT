using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class BaseJobSocketInfo : BaseJobInfo
    {
        public BaseJobSocketInfo(int id, string _name, string _job_type, BaseJobBody _jobBody, int _config_retry) : base(id, _name, _job_type, _jobBody, _config_retry)
        {
        }
    }
}
