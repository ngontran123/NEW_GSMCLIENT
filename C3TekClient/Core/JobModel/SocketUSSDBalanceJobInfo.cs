using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class SocketUSSDBalanceJobInfo : BaseJobInfo
    {
        
        public SocketUSSDBalanceJobInfo(int _id, string _name, string _job_type, USSDJobBody _jobBody, int _config_retry) : base(_id, _name, _job_type, _jobBody, _config_retry)
        {
       
        }
        public SocketUSSDBalanceJobInfo Execute(GSMCom com)
        {
            string response = "";
            int checkBalanceResult = -1;
            string ussd_command = ((USSDJobBody)this.JobBody).command;
            try
            {
            retry_point:
                LogHook($"{com.DisplayName} => {((USSDJobBody)this.JobBody).command} : start \r\n");
                checkBalanceResult = com.CheckBalance();
                response = (checkBalanceResult == -1) ? "" : checkBalanceResult.ToString();   
                if (response != "")
                {
                    //not fail
                    this.ResultJobState = EJobResultState.SUCCESS;
                    this.Resolved = true;
                    this.ResultMessage = response;
                    this.JobState = EJobState.STOPED;
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
                            this.ResultMessage = "Có lỗi xảy ra";
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
                        this.ResultMessage = "Có lỗi xảy ra";
                        this.JobState = EJobState.STOPED;

                        //fail 
                        //bindRecord.Status = "Thất bại";
                        //bindRecord.USSDResult = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[USSDJobInfo Exception] :  {Name} " + ex.Message);
            }
            //LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} end \r\n");

            return this;
        }

    }
}
