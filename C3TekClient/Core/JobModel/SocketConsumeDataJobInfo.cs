using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.JobModel
{
    public class SocketConsumeDataJobInfo : BaseJobInfo
    {

        public SocketConsumeDataJobInfo(int _id, string _name, string _job_type, ConsumeDataJobBody _jobBody, int _config_retry) : base(_id, _name, _job_type, _jobBody, _config_retry)
        {

        }
        public SocketConsumeDataJobInfo Execute(GSMCom com)
        {
            string response = "";
            if(!com.ModemName.Contains("EC20"))
            {
                this.ResultJobState = EJobResultState.FAIL;
                this.Resolved = true;
                this.ResultMessage = $"Chức năng tiêu thụ data chỉ hỗ trợ EC20";
                this.JobState = EJobState.STOPED;
                return this;
            }

            try
            {
            retry_point:
                LogHook($"{com.DisplayName} => {((ConsumeDataJobBody)this.JobBody).package} : start \r\n");
                response = com.ExecuteConsumeData(((ConsumeDataJobBody)this.JobBody).getNumPackageOnly()); //default 5

                if (!string.IsNullOrEmpty(response))
                {
                    string size_success_mb = String.Format("{0:0.00}", response.Length / 1024.00 / 1024.00);      // "123.46"

                    //not fail
                    this.ResultJobState = EJobResultState.SUCCESS;
                    this.Resolved = true;
                    this.ResultMessage = $"Đã tiêu thụ {size_success_mb} MB";
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
                Console.WriteLine($"[SocketConsumeJobinfo Exception] :  {Name} " + ex.Message);
                this.ResultJobState = EJobResultState.FAIL;
                this.Resolved = true;
                this.ResultMessage = "EXCEPTION";
                this.JobState = EJobState.STOPED;
            }


            LogHook($"{com.DisplayName} => {((ConsumeDataJobBody)this.JobBody).package} : end \r\n");


            //LogHook($"{bindRecord.AppPort} => {bindRecord.USSD} end \r\n");

            return this;
        }

    }
}
