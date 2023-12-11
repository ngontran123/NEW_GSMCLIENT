using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.C3Tek
{
    public class RequestLoggerCenter
    {
        public static BindingList<RequestLogger> RequestLoggers = new BindingList<RequestLogger>();
        private static object lockRequestLoggers = new object();
        public static void AddLog(RequestLogger logger)
        {
            lock (lockRequestLoggers)
            {
                RequestLoggers.Add(logger);
            }
        }
        public static void ClearLog()
        {
            lock (lockRequestLoggers)
            {
                RequestLoggers.Clear();
            }
        }
    }

    public class RequestLogger
    {
        public string Phone { get; set; }
        public int OrderID { get; set; }
        public string ActionName { get; set; }
        public DateTime ActionTime { get; set; }
        public string Note { get; set; }
    }
}
