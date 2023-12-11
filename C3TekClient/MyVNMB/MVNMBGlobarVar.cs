using C3TekClient.GSM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.MyVNMB
{
    public static class MVNMBGlobalVar
    {
        private static bool _START = false;

        public static List<MVNMBAccount> Accounts = new List<MVNMBAccount>();

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

            private static BindingList<MVNMBRegisterQueue> Queues = new BindingList<MVNMBRegisterQueue>();

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
                        if (existed.QueueState == MVNMBRegisterQueueState.Succeed)
                        {
                            GSMControlCenter.MyRegisterNotifySuccess(phoneNumber, MyRegisterState.Succeed, "Thành công");
                            return;
                        }
                        else
                        {
                            if (existed.QueueState == MVNMBRegisterQueueState.Failed)
                            {
                                existed.Resolved = false;
                                existed.QueueState = MVNMBRegisterQueueState.None;
                            }
                            existed.QueueTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        Queues.Add(new MVNMBRegisterQueue()
                        {
                            COM = com,
                            Serial = serial,
                            PhoneNumber = phoneNumber,
                            QueueState = MVNMBRegisterQueueState.None,
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
                    if (existed != null && existed.QueueState != MVNMBRegisterQueueState.Succeed)
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
                    && queue.QueueState == MVNMBRegisterQueueState.None);
                }
            }
            public static MVNMBRegisterQueue GetQueue()
            {
                lock (Queues)
                {
                    var _queue = Queues.Where(queue => !queue.Resolved
                    && queue.QueueState == MVNMBRegisterQueueState.None)
                        .OrderBy(queue => queue.QueueTime).FirstOrDefault();
                    if (_queue != null)
                        _queue.QueueState = MVNMBRegisterQueueState.Processing;
                    return _queue;
                }
            }

        }
    }
}
