using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class SocketCallJobInfo : BaseJobInfo
    {

        public SocketCallJobInfo(int _id, string _name, string _job_type, CallJobBody _jobBody, int _config_retry) : base(_id, _name, _job_type, _jobBody, _config_retry)
        {

        }
        public SocketCallJobInfo Execute(GSMCom com)
        {
            string response = "";
            CallJobBody jobBody = ((CallJobBody)this.JobBody);
               
            try
            {
            retry_point:
                LogHook($"CALL {com.DisplayName} => To : {jobBody.Phone} : Duration {jobBody.Duration} start \r\n");
                response = com.Call(jobBody.Phone, jobBody.Duration);
                if (response != "" && response.Contains("OK"))
                {
                    //not fail
                    this.ResultJobState = EJobResultState.SUCCESS;
                    this.Resolved = true;
                    this.ResultMessage = response;
                    this.JobState = EJobState.STOPED;
                    LogHook($"CALL {com.DisplayName} => To : {jobBody.Phone } : Duration {jobBody.Duration} success => {response} \r\n");

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
                Console.WriteLine($"[SocketCallJobInfo Exception] :  {Name} " + ex.Message);
            }
            //LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} end \r\n");

            return this;
        }

    }
}
