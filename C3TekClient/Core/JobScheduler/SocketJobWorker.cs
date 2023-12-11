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
    public class SocketJobWorker
    {
        BindingList<BaseJobInfo> jobInfos = new BindingList<BaseJobInfo>();
        private readonly object _lockJobInfo = new object();

        GSMCom _refCom;
        public SocketJobWorker(GSMCom _com)
        {
            jobInfos = new BindingList<BaseJobInfo>();
            _refCom = _com;
        }
        public BaseJobInfo ConsumeNextJob()
        {
            BaseJobInfo job = null;
            //consume job here

            //consume next task
            job = GetNextJobQueue();
            if (job != null)
            {
                //split to debug. Release can use BaseJobInfo instead to reduce code
                if (job.JobType == "SINGLE_USSD")
                {
                    //consume job ussd
                    SocketUSSDJobInfo ussdJob = (SocketUSSDJobInfo)job;
                    ussdJob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);

                }
                else if (job.JobType == "SMS")
                {
                    //consume job ussd
                    SocketSMSJobInfo smsJob = (SocketSMSJobInfo)job;
                    smsJob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);
                }
                else if(job.JobType == "CALL")
                {
                    SocketCallJobInfo callJob = (SocketCallJobInfo)job;
                    callJob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);
                }
                else if(job.JobType == "MULTI_USSD")
                {
                    SocketMultiUSSDJobInfo multiUSSDJob = (SocketMultiUSSDJobInfo)job;
                    multiUSSDJob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);
                }
                else if (job.JobType == "CONSUME_DATA")
                {
                    SocketConsumeDataJobInfo consumeDatajob = (SocketConsumeDataJobInfo)job;
                    consumeDatajob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);
                }
                else if (job.JobType  == "USSD_BALANCE")
                {
                    SocketUSSDBalanceJobInfo ussdBalanceJob = (SocketUSSDBalanceJobInfo)job;
                    ussdBalanceJob.Execute(_refCom);
                    GSMControlCenter.webSocketClient.SendUpdateTask(job);
                }

                GlobalEvent.OnJobLog($"ConsumeJob => {job.JobType} => {job.Name}");
                
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
