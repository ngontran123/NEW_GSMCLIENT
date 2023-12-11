using C3TekClient.C3Tek;
using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C3TekClient.Helper;
using DevExpress.Utils.Drawing.Animation;

namespace C3TekClient.MyVNPT
{
    public enum EMyVNPTRegisterState
    {
        NONE = 0,
        REQUESTED_LOGIN_OTP = 1,
        INPUT_LOGINED_OTP = 2,
        REQUESTED_FORGET_PASSWORD_OTP = 3,
        CHANGED_PASSWORD = 4,
        DONE = 5
    }
    public class ResponseMyVNPT
    {
        public string error_code { get; set; }
        public string message { get; set; }
    }

    public class MVNPTAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public int state = 0;

        // Trạng thái ánh xạ với nhà mạng
        public EMyVNPTRegisterState CurrentStateNetworkTransaction;

        public MVNPTAccount()
        {
            CurrentStateNetworkTransaction = EMyVNPTRegisterState.NONE;
        }
        public void Register()
        {
            var queue = MVNPTGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            GSMMessage gsmMessage = null;

            AddPhoneResponse transactionInfo = null;
            string lastMessage = "";
            var portal = new C3TekPortal();

            try
            {
                if (queue == null || queue.QueueState != MVNPTRegisterQueueState.Processing)
                    return;


                CurrentStateNetworkTransaction = EMyVNPTRegisterState.NONE;
                username = queue.PhoneNumber;
                _password = MVNPTGlobalVar.RegisterVar.Password;


                bool success = portal.InitRegMy(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier = queue.Carrier }, ref transactionInfo);
                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "Yêu cầu lamsim");

                TTTBHelper.RequestPush(username);

                if (success)
                {
                    var telecomService = new MyTelecom.MyTelecomService();
                    if (GlobalVar.UserSetting.CheckRegMyUseProxy)
                    {
                        telecomService = telecomService.SetWebProxy(GlobalVar.UserSetting.GetRandomProxyItem());
                    }


                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) && (gsmsms.Sender.Trim() == "900" || gsmsms.Sender.Trim() =="MyVNPT"
                            || gsmsms.Sender.Trim() == "7712186788084"))


                        {
                            gsmMessage = gsmsms;

                            if (CurrentStateNetworkTransaction == EMyVNPTRegisterState.REQUESTED_LOGIN_OTP)
                            {
                                if (string.IsNullOrEmpty(gsmMessage.OTP))
                                {
                                    return;
                                }
                                var responseRegMy = telecomService.PostApiMyVNPT<ResponseMyVNPT>("authen_register",
                                            new { msisdn = username, pin = gsmMessage.OTP, password = SecurityHelper.CreateMD5(_password) });

                                if (!string.IsNullOrEmpty(responseRegMy.error_code) && responseRegMy.error_code == "0")
                                {
                                    lastMessage = "Thành công";
                                    queue.QueueState = MVNPTRegisterQueueState.Succeed;
                                    CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed,
                                        lastMessage);
                                }
                                else if (responseRegMy.message.Contains("Thuê bao đã có tài khoản trên hệ thống"))
                                {
                                    //Forgot password

                                    var responseForgetPassword = telecomService.PostApiMyVNPT<ResponseMyVNPT>(
                                            "otp_send", new
                                            {
                                                msisdn = username,
                                                otp_service = "authen_miss_password"
                                            });
                                    if (!string.IsNullOrEmpty(responseForgetPassword.error_code) &&
                                        responseForgetPassword.error_code.Equals("0"))
                                    {
                                        lastMessage = "Đã yêu cầu thay đổi mật khẩu";
                                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.REQUESTED_FORGET_PASSWORD_OTP;
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing,
                                            lastMessage);
                                    }
                                    else
                                    {
                                        //fail to requets forget password;
                                        lastMessage = "Có lỗi yêu cầu thay đổi mật khẩu";
                                        queue.QueueState = MVNPTRegisterQueueState.Failed;
                                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                    }
                                }
                                else
                                {
                                    lastMessage = "Thất bại : " + responseRegMy.message ?? "";

                                    queue.QueueState = MVNPTRegisterQueueState.Failed;
                                    CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                }
                             
                                //else
                                //{
                                //    lastMessage = "Không nhận được OTP";
                                //    queue.QueueState = MVNPTRegisterQueueState.Failed;
                                //    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.NoOTP, lastMessage);
                                //}
                            }
                            else if (CurrentStateNetworkTransaction == EMyVNPTRegisterState.REQUESTED_FORGET_PASSWORD_OTP)
                            {
                                if (!string.IsNullOrEmpty(gsmMessage.OTP))
                                {
                                    var responseRegMy = telecomService.PostApiMyVNPT<ResponseMyVNPT>("authen_miss_password",
                                        new { msisdn = username, otp = gsmMessage.OTP, password = SecurityHelper.CreateMD5(_password) });

                                    if (!string.IsNullOrEmpty(responseRegMy.error_code) && responseRegMy.error_code == "0")
                                    {
                                        lastMessage = "Đổi mật khẩu thành công";
                                        queue.QueueState = MVNPTRegisterQueueState.Succeed;
                                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, lastMessage);
                                    }
                                    else
                                    {
                                        lastMessage = "Lỗi khi reset mật khẩu : "  +  responseRegMy.message ?? "";
                                        queue.QueueState = MVNPTRegisterQueueState.Failed;
                                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                    }
                                }
                                //else
                                //{
                                //    lastMessage = "Không nhận được OTP quên mật khẩu";

                                //    queue.QueueState = MVNPTRegisterQueueState.Failed;
                                //    CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;

                                //    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.NoOTP, lastMessage);
                                //}
                            }

                        }
                    };




                    ResponseMyVNPT responseGetOTPLogin = telecomService.PostApiMyVNPT<ResponseMyVNPT>("otp_send",
                        new { msisdn = username, otp_service = "authen_register,payment_wallet_register" });

                    if (!string.IsNullOrEmpty(responseGetOTPLogin.error_code) && responseGetOTPLogin.error_code.Equals("0"))
                    {
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "Đã gửi OTP");
                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.REQUESTED_LOGIN_OTP;

                    }
                    else
                    {
                        lastMessage = "Lỗi khi gửi OTP : " + responseGetOTPLogin.message ?? "" ;
                        queue.QueueState = MVNPTRegisterQueueState.Failed;
                        CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                    }


                    int waitSecond = 240;
                    DateTime startTime = DateTime.Now;
                    bool processStarted = false;
                loop:
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit || MVNPTGlobalVar.RegisterVar.Stop)
                    {
                        if (CurrentStateNetworkTransaction == EMyVNPTRegisterState.DONE)
                        {

                        }
                        else
                        {
                            lastMessage = "Quá thời gian xử lý";
                            queue.Resolved = true;
                            queue.QueueState = MVNPTRegisterQueueState.Failed;
                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                            portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = -1, message = lastMessage });
                            return;
                        }
                    }

                    if (CurrentStateNetworkTransaction != EMyVNPTRegisterState.DONE)
                    {
                        Thread.Sleep(2500);
                        goto loop;
                    }
                    if (queue.QueueState == MVNPTRegisterQueueState.Failed)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = -2, message = lastMessage });

                    }
                    else if(queue.QueueState ==MVNPTRegisterQueueState.Succeed)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = 1, message = lastMessage });
                    }
                 
                    queue.Resolved = true;
                    //done
                }
                else
                {
                    CurrentStateNetworkTransaction = EMyVNPTRegisterState.DONE;
                    queue.QueueState = MVNPTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"Không thể khởi tạo giao dịch lamsim");
                    queue.Resolved = true;

                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MVNPTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                    if (transactionInfo != null)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, serial = queue.Serial, status = -3, message = ex.Message.ToString() });
                    }
                }
            }
            finally
            {
                MVNPTGlobalVar.RegisterVar.OnEachCompleted();
            }
        }

        public void RegisterOld()
        {
            var queue = MVNPTGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            try
            {
                if (queue == null || queue.QueueState != MVNPTRegisterQueueState.Processing)
                    return;

                username = queue.PhoneNumber.Trim();
                _password = MVNPTGlobalVar.RegisterVar.Password.Trim();
                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "");

                var portal = new C3TekPortal();
                AddPhoneResponse transactionInfo = null;
                bool success = portal.AddPhone(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier = queue.Carrier }, ref transactionInfo);


                if (success && transactionInfo != null)
                {
                    GSMMessage gsmMessage = null;
                    string fullContent = string.Empty;
                    bool otpSent = false;
                    CallbackResultTracking tracking = null;
                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) && 
                            (gsmsms.Sender.Trim() == "900" || gsmsms.Sender.Trim() =="MyVNPT"
                                                           || gsmsms.Sender.Trim() == "7712186788084"

                            ))
                        {
                            gsmMessage = gsmsms;
                            if (tracking != null && (tracking.status == 0 || tracking.status == 1))
                                otpSent = portal.AddSMS(gsmMessage.Sender, gsmMessage.Receiver, gsmMessage.Content, transactionInfo.transaction_id, gsmMessage.Carrier);
                        }
                    };

                    int waitSecond = 500;
                    DateTime startTime = DateTime.Now;
                    bool processStarted = false;
                loop:
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit || MVNPTGlobalVar.RegisterVar.Stop)
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVNPTRegisterQueueState.Failed;
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
                        queue.QueueState = MVNPTRegisterQueueState.Failed;
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
                        //MVNPTGlobalVar.Accounts.Add(result);
                        queue.Resolved = true;
                        queue.QueueState = MVNPTRegisterQueueState.Succeed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, $"{tracking.message}");
                        MyCenter.AddMy(new My() { MyCarrier = MyCarrier.VINAPHONE, Username = username, Password = _password, TransactionID = transactionInfo.transaction_id.ToString() });
                    }
                    else
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVNPTRegisterQueueState.Failed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ETT: Status {tracking.status.ToString()}");
                        return;
                    }
                }
                else
                {
                    queue.Resolved = true;
                    queue.QueueState = MVNPTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ES: {portal.LastMessage}");
                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MVNPTRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                }
            }
            finally
            {
                MVNPTGlobalVar.RegisterVar.OnEachCompleted();
            }
        }

    }
}
