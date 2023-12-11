using C3TekClient.C3Tek;
using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace C3TekClient.MyVNMB
{
    public class MVNMBAccount
    {
        public string Username { get; set; }
        public string Password { get; set; }
        
        
        public void Register()
        {
            var queue = MVNMBGlobalVar.RegisterVar.GetQueue();
            string username = "";
            string _password = "";
            try
            {
                if (queue == null || queue.QueueState != MVNMBRegisterQueueState.Processing)
                    return;

                username = queue.PhoneNumber;
                _password = MVNMBGlobalVar.RegisterVar.Password;
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

                        if (gsmsms.Receiver == username && !string.IsNullOrEmpty(gsmsms.OTP) && gsmsms.Sender.Trim() == "Bima" || gsmsms.Sender.Trim() == "6610510997" )
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
                    if ((DateTime.Now - startTime).TotalSeconds > waitSecond || GlobalVar.IsApplicationExit || MVNMBGlobalVar.RegisterVar.Stop)
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVNMBRegisterQueueState.Failed;
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
                        queue.QueueState = MVNMBRegisterQueueState.Failed;
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
                        //MVNMBGlobalVar.Accounts.Add(result);
                        queue.Resolved = true;
                        queue.QueueState = MVNMBRegisterQueueState.Succeed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Succeed, $"{tracking.message}");
                        MyCenter.AddMy(new My() { MyCarrier = MyCarrier.VIETNAMMOBILE, Username = username, Password = _password, TransactionID = transactionInfo.transaction_id.ToString() });
                    }
                    else
                    {
                        queue.Resolved = true;
                        queue.QueueState = MVNMBRegisterQueueState.Failed;
                        GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ETT: Status {tracking.status.ToString()}");
                        return;
                    }
                }
                else
                {
                    queue.Resolved = true;
                    queue.QueueState = MVNMBRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"ES: {portal.LastMessage}");
                }
            }
            catch (Exception ex)
            {
                if (queue != null)
                {
                    queue.Resolved = true;
                    queue.QueueState = MVNMBRegisterQueueState.Failed;
                    GSMControlCenter.MyRegisterNotifySuccess(username, MyRegisterState.Failed, $"{ex.Message}");
                }
            }
            finally
            {
                MVNMBGlobalVar.RegisterVar.OnEachCompleted();
            }
        }

    }
}
