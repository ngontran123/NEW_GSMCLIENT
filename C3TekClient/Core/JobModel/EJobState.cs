using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public enum EJobState
    {
        NONE = -1,
        WAIT = 0,
        RUNNING = 2, 
        RETRYING = 3,
        PAUSED = 4,  
        STOPED  =5
    }
}
