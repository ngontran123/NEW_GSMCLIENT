using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.MyVNPT
{
    public static class MVNPTGlobalVar
    {
        private static bool _START = false;

        public static List<MVNPTAccount> Accounts = new List<MVNPTAccount>();

        public static void StartHanding()
        {
            if (_START)
                return;
            _START = true;
        }


        public static class RegisterVar
        {
            public static int OTPTimeout = 60;
            public static int TotalThread = 15;
            public static int RunningThread = 0;
            public static string Password { get; set; }

            public static bool Stop = false;

            private static BindingList<MVNPTRegisterQueue> Queues = new BindingList<MVNPTRegisterQueue>();

            public static object LockRunningThread = new object();
            public static void OnEachCompleted()
            {
                lock (LockRunningThread)
                {
                    RunningThread--;
                }
            }
            public static void Reset()
            {
                Password = string.Empty;
                TotalThread = 20;
                RunningThread = 0;
                Stop = false;
            }
            public static void OnSIMInjected(string phoneNumber, string com, string serial, SIMCarrier carrier)
            {
                lock (Queues)
                {
                    var existed = Queues.FirstOrDefault(queue => queue.PhoneNumber == phoneNumber);
                    if (existed != null)
                    {
                        if (existed.QueueState == MVNPTRegisterQueueState.Succeed)
                        {
                            GSMControlCenter.MyRegisterNotifySuccess(phoneNumber, MyRegisterState.Succeed, "Thành công");
                            return;
                        }
                        if (existed.QueueState == MVNPTRegisterQueueState.Failed)
                        {
                            existed.Resolved = false;
                            existed.QueueState = MVNPTRegisterQueueState.None;
                        }
                        existed.QueueTime = DateTime.Now;
                    }
                    else
                    {
                        Queues.Add(new MVNPTRegisterQueue()
                        {
                            COM = com,
                            Serial = serial,
                            PhoneNumber = phoneNumber,
                            QueueState = MVNPTRegisterQueueState.None,
                            QueueTime = DateTime.Now,
                            Resolved = false,
                            Carrier = GlobalVar.MapCarrier[carrier]
                        });
                    }
                }
            }
            public static void OnSIMRejected(string phoneNumber)
            {
                lock (Queues)
                {
                    var existed = Queues.FirstOrDefault(queue => queue.PhoneNumber == phoneNumber);
                    if (existed != null && existed.QueueState != MVNPTRegisterQueueState.Succeed)
                    {
                        Queues.Remove(existed);
                    }
                }
            }

            public static bool HasQueue()
            {
                lock (Queues)
                {
                    return Queues.Any(queue => !queue.Resolved
                    && queue.QueueState == MVNPTRegisterQueueState.None);
                }
            }
            public static MVNPTRegisterQueue GetQueue()
            {
                lock (Queues)
                {
                    var _queue = Queues.Where(queue => !queue.Resolved
                    && queue.QueueState == MVNPTRegisterQueueState.None)
                        .OrderBy(queue => queue.QueueTime).FirstOrDefault();
                    if (_queue != null)
                        _queue.QueueState = MVNPTRegisterQueueState.Processing;
                    return _queue;
                }
            }

        }
    }
}
