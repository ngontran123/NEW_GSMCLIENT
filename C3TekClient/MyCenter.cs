using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient
{
    public static class MyCenter
    {
        public static BindingList<My> Mys = new BindingList<My>();
        private static object lockMys = new object();

        public static void AddMy(My my)
        {
            lock (lockMys)
            {
                Mys.Add(my);
            }
        }

        public static void RemoveMy(string username)
        {
            lock (lockMys)
            {
                var my = Mys.FirstOrDefault(m => m.Username == username);
                if (my != null)
                    Mys.Remove(my);
            }
        }
    }
    public class My
    {
        public string TransactionID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public MyCarrier MyCarrier { get; set; }
    }
    public enum MyCarrier
    {
        VIETTEL, MOBIFONE, VINAPHONE, VIETNAMMOBILE
    }
}
