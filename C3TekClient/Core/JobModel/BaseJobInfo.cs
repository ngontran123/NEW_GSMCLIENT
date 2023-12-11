using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    
    public class BaseJobInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ConfigRetry { get; set; }
        public string JobType { get; set; }
        public int ConfigSleepAfterCompleted { get; set; }

        public BaseJobBody JobBody { get; set; }
        public EJobState JobState { get; set; }
           
        public EJobResultState ResultJobState { get; set; }
        public string ResultMessage { get; set; }
        public int RetryCount { get; set; }
        public bool Resolved { get; set; }
        public Action<string> LogHook = (message) => { Console.WriteLine($"[BaseJobInfo LogHook()] : {message} "); };

        public int DelayTime { get; set; }
        public int Timeout { get; set; }
        public BaseJobInfo(int id, string _name, string _job_type, BaseJobBody _jobBody, int _config_retry )
        {
            this.Id = id;
            this.Name = _name;
            this.JobType = _job_type;
            this.JobBody = _jobBody;
            this.ConfigRetry = _config_retry; 
            InitDefaultJob();
        }

        
        public void InitDefaultJob()
        {
            Description = "";
            ConfigSleepAfterCompleted = 0;
            JobState = EJobState.NONE;
            ResultJobState = EJobResultState.NONE;
            ResultMessage = "";
            RetryCount = 0;
            Resolved = false;
            DelayTime = 0;
            Timeout = 0;
        }
        public bool isCompleted()
        {
            return Resolved; 
        }
        public bool isSuccess()
        {
            return this.ResultJobState == EJobResultState.SUCCESS;
        }
        public bool isFail()
        {
            return this.ResultJobState == EJobResultState.FAIL;
        }
        public string getResultMessage()
        {
            return this.ResultMessage;
        }
        public bool isRetry()
        {
            return ConfigRetry > 0;
        }
    }
}
