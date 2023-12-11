
using C3TekClient.C3Tek;
using C3TekClient.GSM;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C3TekClient.Helper;
using System.Windows.Forms;
namespace C3TekClient.MyViettel
{
    public class MVTAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        class ResponseMyViettelOTP
        {
            public int errorCode { get; set; }
            public string message { get; set; }
        }

        private ResponseMyViettelRegister lastRecordDataResponse = null; 

        public enum EMyViettelState
        {
            NONE = 0,
            REQUESTED_LOGIN_OTP = 1,
            DONE = 2
        }
        public EMyViettelState CurrentStateNetworkTransaction;

        public MVTAccount()
        {
            CurrentStateNetworkTransaction = EMyViettelState.NONE;
        }
        public void Register()
        {
            var queue = MVTGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            GSMMessage gsmMessage = null;
            AddPhoneResponse transactionInfo = null;
            string lastMessage = "";
            var portal = new C3TekPortal();


            try
            {
                if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                    return;
                CurrentStateNetworkTransaction = EMyViettelState.NONE;
                username = queue.PhoneNumber;

                _password = MVTGlobalVar.RegisterVar.Password;

                bool success = portal.InitRegMy(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier = queue.Carrier }, ref transactionInfo);
                TTTBHelper.RequestPush(username);

                if (success)
                {
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "");

                    var telecomService = new MyTelecom.MyTelecomService();
                    if (GlobalVar.UserSetting.CheckRegMyUseProxy)
                    {
                        telecomService = telecomService.SetWebProxy(GlobalVar.UserSetting.GetRandomProxyItem());
                    }


                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) &&
                            (gsmsms.Sender.Trim() == "MyViettel"||gsmsms.Sender.Trim() == "VTSHOP"))
                        {
                            gsmMessage = gsmsms;
                            if (CurrentStateNetworkTransaction == EMyViettelState.REQUESTED_LOGIN_OTP)
                            {
                                //Lấy OTP và đăng ký tại đây
                                if (string.IsNullOrEmpty(gsmMessage.OTP))
                                {
                                    //queue.QueueState = MVTRegisterQueueState.Failed;
                                    //CurrentStateNetworkTransaction = EMyViettelState.DONE;
                                    //GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, "Lỗi khi lấy OTP từ SMS");
                                    return;
                                }

                                ResponseMyViettelRegister responseMyViettelRegister = telecomService.PostApiMyViettel<ResponseMyViettelRegister>("registerUserNewV4", new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string> ("username", username),
                                        new KeyValuePair<string, string> ("password", _password),
                                        new KeyValuePair<string, string> ("otp", gsmMessage.OTP),
                                        new KeyValuePair<string, string> ("username", username),
                                        new KeyValuePair<string, string> ("device_name", "SM-J415C"),
                                        new KeyValuePair<string, string> ("device_id",  StringHelper.RandomDeviceID()),
                                        new KeyValuePair<string, string> ("os_type", "android"),
                                        new KeyValuePair<string, string> ("os_version", "32"),
                                        new KeyValuePair<string, string> ("app_version", ""),
                                        new KeyValuePair<string, string> ("user_type", "1"),
                                        new KeyValuePair<string, string> ("build_code", "411"),
                                        new KeyValuePair<string, string> ("cmnd", ""),
                                        new KeyValuePair<string, string> ("checksum", "E5DB6DB9-AA0E-5DD9-C0FB-BDB0B3FCCCA2"),
                                    })
                                );
                                if (responseMyViettelRegister != null && !string.IsNullOrEmpty(responseMyViettelRegister.ErrorCode) &&
                                    responseMyViettelRegister.ErrorCode == "0")
                                {
                                    queue.QueueState = MVTRegisterQueueState.Succeed;
                                    lastRecordDataResponse = responseMyViettelRegister;
                                    lastMessage = "Thành công";
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, "Thành công");
                                    CurrentStateNetworkTransaction = EMyViettelState.DONE;

                                }
                                else
                                {
                                    queue.QueueState = MVTRegisterQueueState.Failed;
                                    lastMessage = responseMyViettelRegister != null
                                        ? responseMyViettelRegister.Message
                                        : "ERR: Lỗi khi đăng ký";

                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                    CurrentStateNetworkTransaction = EMyViettelState.DONE;
                                }

                            }
                            //process OTP here
                        }
                    };
                    string deviceId = StringHelper.RandomDeviceID(); 
                    ResponseMyViettelOTP responseMyViettelOtp = telecomService.PostApiMyViettel<ResponseMyViettelOTP>("getOtp", new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string> ("msisdn",username),
                            new KeyValuePair<string, string> ("device_name","SM-J415C"),
                            new KeyValuePair<string, string> ("device_id",deviceId),
                            new KeyValuePair<string, string> ("os_type", "android"),
                            new KeyValuePair<string, string> ("os_version", "32"),
                            new KeyValuePair<string, string> ("app_version", "411"),
                            new KeyValuePair<string, string> ("user_type", "1"),
                        })
                    );
                    if (responseMyViettelOtp != null &&  responseMyViettelOtp.errorCode == 0)
                    {   GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "Gửi OTP");
                        CurrentStateNetworkTransaction = EMyViettelState.REQUESTED_LOGIN_OTP;
                    }
                    else
                    {
                        lastMessage = responseMyViettelOtp != null
                            ? responseMyViettelOtp.message
                            : "ERR: Lỗi khi yêu cầu OTP"; //OTP
                        
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                        queue.QueueState = MVTRegisterQueueState.Failed;

                        CurrentStateNetworkTransaction = EMyViettelState.DONE;


                    }
                    int waitSecond = 240;
                    DateTime startTime = DateTime.Now;
                loop:

                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit ||
                        MVTGlobalVar.RegisterVar.Stop)
                    {
                        if (CurrentStateNetworkTransaction == EMyViettelState.DONE)
                        {

                        }
                        else
                        {
                            queue.Resolved = true;
                            queue.QueueState = MVTRegisterQueueState.Failed;
                            lastMessage = "Quá thời gian xử lý";
                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed,
                                lastMessage);
                            portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, serial = queue.Serial , status = -1, message = lastMessage });
                            return;
                        }
                    }

                    if (CurrentStateNetworkTransaction != EMyViettelState.DONE)
                    {
                        Thread.Sleep(2500);
                        goto loop;
                    }

                    if (queue.QueueState == MVTRegisterQueueState.Failed)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = -2 , message = lastMessage });

                    }
                    else
                    {
                        portal.UpdateRegMy(new UpdateTelecomVT()
                        {
                            com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = 1, message = lastMessage, 
                            token= lastRecordDataResponse.Data.Data.Token ?? "",  
                            check_sum = lastRecordDataResponse.Data.Data.Checksum ?? "", 
                            key_device_acc = lastRecordDataResponse.Data.Data.KeyDeviceAcc ?? "",
                            key_refresh = lastRecordDataResponse.Data.Data.KeyRefresh ?? "",
                            key_refresh_finger_print = lastRecordDataResponse.Data.Data.KeyRefreshFingerPrint ?? "",
                            device_id = deviceId ?? ""
                        });
                    }
                    queue.Resolved = true;
                }
                else
                {
                    queue.Resolved = true;
                    CurrentStateNetworkTransaction = EMyViettelState.DONE;
                    queue.QueueState = MVTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"Không thể khởi tạo giao dịch lamsim");

                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MVTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                    if (transactionInfo != null)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, serial = queue.Serial, status = -3 , message = ex.Message.ToString()});
                    }
                }
            }
            finally
            {
                MVTGlobalVar.RegisterVar.OnEachCompleted();
            }
        }
        public void RegisterOld()
        {
            var queue = MVTGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            try
            {
                if (queue == null || queue.QueueState != MVTRegisterQueueState.Processing)
                    return;

                username = queue.PhoneNumber;
                _password = MVTGlobalVar.RegisterVar.Password;
                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "");

                var portal = new C3TekPortal();
                AddPhoneResponse transactionInfo = null;

                bool success = portal.AddPhone(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier = queue.Carrier }, ref transactionInfo);
                TTTBHelper.RequestPush(username);


                if (success && transactionInfo != null)
                {
                    GSMMessage gsmMessage = null;
                    string fullContent = string.Empty;
                    bool otpSent = false;
                    CallbackResultTracking tracking = null;
                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) && gsmsms.Sender.Trim() == "MyViettel")
                        {
                            gsmMessage = gsmsms;
                            if (tracking != null && (tracking.status == 0 || tracking.status == 1))
                                otpSent = portal.AddSMS(gsmMessage.Sender, gsmMessage.Receiver, gsmMessage.Content, transactionInfo.transaction_id, gsmMessage.Carrier);
                        }
                    };

                    int waitSecond = 300;
                    DateTime startTime = DateTime.Now;
                    bool processStarted = false;
                loop:
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit || MVTGlobalVar.RegisterVar.Stop)
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVTRegisterQueueState.Failed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, otpSent ? MyRegisterState.Failed : MyRegisterState.NoOTP, otpSent ? $"Quá thời gian chờ xử lý" : "Không về OTP");
                        return;
                    }

                    tracking = portal.ResultTracking(transactionInfo.transaction_id, username).FirstOrDefault();
                    if (tracking == null)
                    {
                        Thread.Sleep(5000);
                        goto loop;
                    }
                    else if (tracking.status == 0)
                    {
                        startTime = DateTime.Now;
                        Thread.Sleep(5000);
                        goto loop;
                    }
                    else if (tracking.status == 3 || tracking.status == 4 || tracking.status == 5)
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVTRegisterQueueState.Failed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{(string.IsNullOrEmpty(tracking.message) ? string.Empty : tracking.message)}");
                        return;
                    }
                    else if (tracking.status == 1)
                    {
                        if (!processStarted)
                        {
                            processStarted = true;
                            startTime = DateTime.Now;
                        }
                        Thread.Sleep(5000);
                        goto loop;
                    }
                    else if (tracking.status == 2)
                    {
                        Username = username;
                        Password = _password;
                        //result = this;
                        //MVTGlobalVar.Accounts.Add(result);
                        queue.Resolved = true;
                        queue.QueueState = MVTRegisterQueueState.Succeed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, $"{tracking.message}");
                        MyCenter.AddMy(new My() { MyCarrier = MyCarrier.VIETTEL, Username = username, Password = _password, TransactionID = transactionInfo.transaction_id.ToString() });
                    }
                    else
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVTRegisterQueueState.Failed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ETT: Status {tracking.status.ToString()}");
                        return;
                    }
                }
                else
                {
                    queue.Resolved = true;
                    queue.QueueState = MVTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ES: {portal.LastMessage}");
                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MVTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                }
            }
            finally
            {
                MVTGlobalVar.RegisterVar.OnEachCompleted();
            }
        }
    }
}
