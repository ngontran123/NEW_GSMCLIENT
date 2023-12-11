using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using C3TekClient.Core.JobModel;
using System.ComponentModel;

namespace C3TekClient.Core.JobScheduler
{
    public class JobWorker 
    {
        BindingList<BaseJobInfo> jobInfos = new BindingList<BaseJobInfo>();
        private readonly object _lockJobInfo = new object();

        GSMCom _refCom;
        public JobWorker(GSMCom _com)
        {
            jobInfos =  new BindingList<BaseJobInfo>(); 
            _refCom = _com;
        }
        public BaseJobInfo ConsumeNextJob()
        {
            BaseJobInfo job = null;
            //consume job here
            
                //consume next task
            job = GetNextJobQueue();
            if(job != null)
            {
                if(job.JobType== "USSD")
                {
                    //consume job ussd
                    USSDJobInfo ussdJob = (USSDJobInfo)job;
                    ussdJob.Execute(_refCom);
                }
            }
            
            return job;
        }
        
        public BindingList<BaseJobInfo> GetJobInfoBinding()
        {
            return this.jobInfos;
        }
      
        public bool HasJobQueue()
        {
            lock (_lockJobInfo)
            {
                return jobInfos.Any(q => !q.isCompleted()); 
            }
        }
        public BaseJobInfo GetNextJobQueue()
        {
            lock (_lockJobInfo)
            {
                var _queue = jobInfos.Where(q => !q.isCompleted()).FirstOrDefault();
                if (_queue != null)
                {
                    _queue.JobState = EJobState.RUNNING;
                }
                return _queue;
            }
        }


        public void PushJobInfo(BaseJobInfo job)
        {
            lock (_lockJobInfo)
            {
                jobInfos.Add(job);
            }
        }
        public int CountJob()
        {
            lock (_lockJobInfo)
            {
                return jobInfos.Count;
            }
        }
        public void ClearAllJob()
        {
            lock (_lockJobInfo)
            {
                jobInfos.Clear();
            }
        }
        
        
        
    }
}
