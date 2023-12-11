using C3TekClient.GSM;
using C3TekClient.GSM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class USSDJobInfo : BaseJobInfo
    {
        CustomAdvanceUSSDBinding bindRecord;

        public USSDJobInfo(int _id, CustomAdvanceUSSDBinding customAdvanceUSSDBinding,  string _name, string _job_type, BaseJobBody _jobBody, int _config_retry ) : base( _id, _name, _job_type, _jobBody, _config_retry)
        {
            bindRecord = customAdvanceUSSDBinding;

        }
        public USSDJobInfo Execute(GSMCom com)
        {
            string response = "";
            try {
            retry_point:
                LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} : start \r\n");
                response = com.ExecuteSingleUSSD(bindRecord.USSD);
                if(response != "")
                {
                    //not fail
                    this.ResultJobState = EJobResultState.SUCCESS;
                    this.Resolved = true;
                    this.ResultMessage = response;
                    this.JobState = EJobState.STOPED;
                    bindRecord.Status = "Thành công";
                    bindRecord.USSDResult = response;
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
                            bindRecord.Status = "Thất bại";
                            bindRecord.USSDResult =  "";

                        }
                        else
                        {
                            LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} : retry {RetryCount} \r\n");
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
                        bindRecord.Status = "Thất bại";
                        bindRecord.USSDResult = "";
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"[USSDJobInfo Exception] :  {Name} " + ex.Message);
            }
            LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} end \r\n");

            return this;
        }
     
    }
}
