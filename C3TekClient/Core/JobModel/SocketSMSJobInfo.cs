using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class SocketSMSJobInfo : BaseJobInfo
    {

        public SocketSMSJobInfo(int _id, string _name, string _job_type, SMSJobBody _jobBody, int _config_retry) : base(_id, _name, _job_type, _jobBody, _config_retry)
        {

        }
        public SocketSMSJobInfo Execute(GSMCom com)
        {
            string response = "";
            SMSJobBody jobBody = ((SMSJobBody)this.JobBody);
               
            try
            {
            retry_point:
                LogHook($"SMS {com.DisplayName} => To : {jobBody.To } : Message {jobBody.Message} start \r\n");
                response = com.SendMessage(jobBody.To, jobBody.Message);
                if (response != "" && response.Contains("OK") && !response.Contains("ERROR"))
                {
                    //not fail
                    this.ResultJobState = EJobResultState.SUCCESS;
                    this.Resolved = true;
                    this.ResultMessage = response;
                    this.JobState = EJobState.STOPED;
                    LogHook($"SMS {com.DisplayName} => To : {jobBody.To } : Message {jobBody.Message} success => {response} \r\n");

                }
                else 
                {
                    //empty
                    if (this.isRetry())
                    {
                        if (RetryCount > ConfigRetry)
                        {
                            this.ResultJobState = EJobResultState.FAIL;
                            this.Resolved = true;
                            this.ResultMessage = response;
                            this.JobState = EJobState.STOPED;
                            //that bai

                        }
                        else
                        {
                            //retry
                            //LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} : retry {RetryCount} \r\n");
                            RetryCount++;
                            goto retry_point;
                        }
                    }
                    else
                    {
                        this.ResultJobState = EJobResultState.FAIL;
                        this.Resolved = true;
                        this.ResultMessage = response;
                        this.JobState = EJobState.STOPED;
                        //fail 
                        //bindRecord.Status = "Thất bại";
                        //bindRecord.USSDResult = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMSJobInfo Exception] :  {Name} " + ex.Message);
            }
            //LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} end \r\n");

            return this;
        }

    }
}
