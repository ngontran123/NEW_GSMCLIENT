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

namespace C3TekClient.MyMobifone
{
    public class MMFAccount
    {
        public string Username { get; set; } 
        public string Password { get; set; }

        public enum EMyMobiRegisterState
        {
            NONE = 0,
            REQUESTED_LOGIN_OTP = 1,
            INPUT_LOGINED_OTP = 2,
            REQUESTED_FORGET_PASSWORD_OTP = 3,
            CHANGED_PASSWORD = 4,
            DONE = 5
        }

        public EMyMobiRegisterState CurrentStateNetworkTransaction { get; set; }
        public MMFAccount()
        {

        }

        public class ErrorResponse
        {
            public string code { get; set; }
            public string impact { get; set; }
            public string   message { get; set; }
        }

        public class PhoneDataLogin
        {
            public string phone { get; set; }
            public int type { get; set; }
            public string name { get; set; }
            public int isPrimary { get; set; }
            public int receive_noti { get; set; }
        }

        public class DataLogin
        {
            public string apiSecret { get; set; }
            public string refreshKey { get; set; }
            public string userId { get; set; }
            public object avatar { get; set; }
            public int hasPass { get; set; }
            public List<PhoneDataLogin> phone { get; set; }
            public string name { get; set; }
            public int tokenExpired { get; set; }
            public object reward { get; set; }
        }

        public class IResponseMobiAPI
        {
            public List<ErrorResponse> errors { get; set; }

        }
        public class ResponseCommonMobiAPI : IResponseMobiAPI
        {
            public bool data { get; set; }
        }

        public class ResponseLoginMobiAPI :IResponseMobiAPI
        {
            public DataLogin data { get; set; }
        }

        public void Register()
        {
            var queue = MMFGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            GSMMessage gsmMessage = null;
            IResponseMobiAPI responseMobiApi = null;

            AddPhoneResponse transactionInfo = null;
            string lastMessage = "";
            var portal = new C3TekPortal();


            try
            {
                if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                    return;

                CurrentStateNetworkTransaction = EMyMobiRegisterState.NONE;
                username = queue.PhoneNumber;
                _password = MMFGlobalVar.RegisterVar.Password;
                bool success = portal.InitRegMy(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier = queue.Carrier }, ref transactionInfo);
                TTTBHelper.RequestPush(username);

                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "Kết nối lamsim");
                if (success)
                {

                    string topError = "";

                    var telecomService = new MyTelecom.MyTelecomService();

                    if (GlobalVar.UserSetting.CheckRegMyUseProxy)
                    {
                        telecomService = telecomService.SetWebProxy(GlobalVar.UserSetting.GetRandomProxyItem());
                    }

                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) &&
                            gsmsms.Sender.Trim() == "MyMobiFone")
                        {
                            gsmMessage = gsmsms;

                            //handle otp login
                            if (string.IsNullOrEmpty(gsmMessage.OTP))
                            {
                                return;
                            }

                            if (CurrentStateNetworkTransaction == EMyMobiRegisterState.REQUESTED_LOGIN_OTP)
                            {
                                //2. API login with otp here

                                responseMobiApi = null;

                                responseMobiApi = telecomService.PostApiMyMobi<ResponseLoginMobiAPI>("auth/otplogin",
                                    new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("phone", username),
                                        new KeyValuePair<string, string>("otp", gsmMessage.OTP ?? ""),
                                    })
                                );
                                if (responseMobiApi == null)
                                {
                                    lastMessage = "Lỗi khi đăng nhập OTP";

                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                }
                                else if (responseMobiApi.errors == null || responseMobiApi.errors.Count == 0)
                                {
                                    //No error
                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.INPUT_LOGINED_OTP;
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing,
                                        "Đăng nhập thành công OTP");

                                    //request next here

                                    responseMobiApi = null;

                                    responseMobiApi = telecomService.PostApiMyMobi<ResponseCommonMobiAPI>("auth/getotp",
                                        new FormUrlEncodedContent(new[]
                                        {
                                            new KeyValuePair<string, string>("phone", username),
                                            new KeyValuePair<string, string>("prefix", "forgetOTP"),
                                        })
                                    );

                                    if (responseMobiApi == null)
                                    {
                                        lastMessage = "ERR: Không thể yêu cầu quên mật khẩu";

                                        CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                        queue.Resolved = true;
                                        queue.QueueState = MMFRegisterQueueState.Failed;

                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed,lastMessage);
                                    }
                                    else if (responseMobiApi.errors == null || responseMobiApi.errors.Count == 0)
                                    {
                                        lastMessage = "Lấy OTP thay đổi mật khẩu";
                                        CurrentStateNetworkTransaction =
                                            EMyMobiRegisterState.REQUESTED_FORGET_PASSWORD_OTP;
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, lastMessage);
                                    }
                                    else
                                    {
                                        lastMessage = "Lỗi từ nhà mạng: " + topError ?? ""; 
                                        CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                        queue.Resolved = true;
                                        queue.QueueState = MMFRegisterQueueState.Failed;
                                        topError = responseMobiApi.errors[0].message ?? "";
                                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                    }

                                }
                                else
                                {
                                    //Neu co loi
                                    lastMessage = "Lỗi từ nhà mạng: " + topError ?? "";
                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;
                                    topError = responseMobiApi.errors[0].message ?? "";
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                                }
                            }
                            else if (CurrentStateNetworkTransaction ==
                                     EMyMobiRegisterState.REQUESTED_FORGET_PASSWORD_OTP)
                            {
                                //request change password here

                                responseMobiApi = telecomService.PostApiMyMobi<ResponseCommonMobiAPI>(
                                    "auth/recoverpass",
                                    new FormUrlEncodedContent(new[]
                                    {
                                        new KeyValuePair<string, string>("phone", username),
                                        new KeyValuePair<string, string>("otp", gsmMessage.OTP),
                                        new KeyValuePair<string, string>("password",
                                            SecurityHelper.ComputeSha256Hash(_password)),
                                    })
                                );
                                if (responseMobiApi == null)
                                {
                                    lastMessage = "Lỗi khi đổi mật khẩu";

                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;

                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed,
                                        "Không thể đổi mật khẩu");
                                }
                                else if (responseMobiApi.errors == null || responseMobiApi.errors.Count == 0)
                                {
                                    //khong co loi
                                    lastMessage = "Thành công";

                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Succeed;

                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, lastMessage);
                                }
                                else
                                {
                                    lastMessage = "Lỗi từ nhà mạng: " + topError ?? "";

                                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                                    queue.Resolved = true;
                                    queue.QueueState = MMFRegisterQueueState.Failed;
                                    topError = responseMobiApi.errors[0].message ?? "";
                                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);

                                }

                            }

                        }
                    };


                    responseMobiApi = telecomService.PostApiMyMobi<ResponseCommonMobiAPI>("auth/getloginotp",
                        new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("phone", username),

                        })
                    );


                    if (responseMobiApi == null)
                    {
                        lastMessage = "ERR: Không thể yêu cầu lấy OTP";

                        CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                        queue.Resolved = true;
                        queue.QueueState = MMFRegisterQueueState.Failed;

                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                    }
                    else if (responseMobiApi.errors == null || responseMobiApi.errors.Count == 0)
                    {
                        //No error
                        CurrentStateNetworkTransaction = EMyMobiRegisterState.REQUESTED_LOGIN_OTP;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "Gửi OTP");
                    }
                    else
                    {
                        //Neu co loi
                        lastMessage = "Lỗi từ nhà mạng: " + topError ?? "";
                        CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                        queue.QueueState = MMFRegisterQueueState.Failed;
                        queue.Resolved = true;
                        topError = responseMobiApi.errors[0].message ?? "";
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                    }

                    int waitSecond = 240;
                    DateTime startTime = DateTime.Now;
                    bool processStarted = false;
                    loop:
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit ||
                        MMFGlobalVar.RegisterVar.Stop)
                    {
                        if (CurrentStateNetworkTransaction == EMyMobiRegisterState.DONE)
                        {

                        }
                        else
                        {
                            lastMessage = "Quá thời gian xử lý";

                            queue.Resolved = true;
                            queue.QueueState = MMFRegisterQueueState.Failed;
                            GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, lastMessage);
                            portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = -1, message = lastMessage });

                            return;
                        }
                    }

                    if (CurrentStateNetworkTransaction != EMyMobiRegisterState.DONE)
                    {
                        Thread.Sleep(5000);
                        goto loop;
                    }
                    if (queue.QueueState == MMFRegisterQueueState.Failed)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = -2, message = lastMessage });

                    }
                    else
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, carrier = queue.Carrier, serial = queue.Serial, status = 1, message = lastMessage });
                    }
                    queue.Resolved = true;

                    //done
                    //1. Get Login OTP


                    //2. Login with OTP


                    //3. Get Forgot password OTP


                    //4. Change password with OTP
                }
                else
                {
                    queue.Resolved = true;
                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE;
                    queue.QueueState = MMFRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"Không thể khởi tạo giao dịch lamsim");
                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MMFRegisterQueueState.Failed;
                    CurrentStateNetworkTransaction = EMyMobiRegisterState.DONE; 
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                    if (transactionInfo != null)
                    {
                        portal.UpdateRegMy(new UpdateTelecom() { com = queue.COM, phone = username, transaction_id = transactionInfo.transaction_id, serial = queue.Serial, status = -3, message = ex.Message.ToString() });
                    }
                }
            }
            finally
            {
                MMFGlobalVar.RegisterVar.OnEachCompleted();

            }
        }

        public void RegisterOld()
        {
            var queue = MMFGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            try
            {
                if (queue == null || queue.QueueState != MMFRegisterQueueState.Processing)
                    return;

                username = queue.PhoneNumber;
                _password = MMFGlobalVar.RegisterVar.Password;
                GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Processing, "");

                var portal = new C3TekPortal();
                AddPhoneResponse transactionInfo = null;
                bool success = portal.AddPhone(new AddPhone() { com = queue.COM, phone = username, password = _password, serial = queue.Serial, carrier =  queue.Carrier}, ref transactionInfo);


                if (success && transactionInfo != null)
                {
                    GSMMessage gsmMessage = null;
                    string fullContent = string.Empty;
                    bool otpSent = false;
                    CallbackResultTracking tracking = null;
                    GSMControlCenter.OnNewMessage += (gsmsms) =>
                    {
                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) && gsmsms.Sender.Trim() == "MyMobiFone")
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
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit || MMFGlobalVar.RegisterVar.Stop)
                    {
                        queue.Resolved = true;
                        queue.QueueState = MMFRegisterQueueState.Failed;
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
                        queue.QueueState = MMFRegisterQueueState.Failed;
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
                        //Username = username;
                        //Password = _password;
                        //result = this;
                        //MMFGlobalVar.Accounts.Add(result);
                        queue.Resolved = true;
                        queue.QueueState = MMFRegisterQueueState.Succeed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, $"{tracking.message}");
                        MyCenter.AddMy(new My() { MyCarrier = MyCarrier.MOBIFONE, Username = username, Password = _password, TransactionID = transactionInfo.transaction_id.ToString() });
                    }
                    else
                    {
                        queue.Resolved = true;
                        queue.QueueState = MMFRegisterQueueState.Failed;
                        //STT: sai trạng thái
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"STTE: Status {tracking.status.ToString()}");
                        return;
                    }
                }
                else
                {
                    //PSE
                    queue.Resolved = true;
                    queue.QueueState = MMFRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"PSE: {portal.LastMessage}");
                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MMFRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                }
            }
            finally
            {
                MMFGlobalVar.RegisterVar.OnEachCompleted();
            }
        }

    }
}
