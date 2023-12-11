using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.C3Tek
{
    public class C3TekAccount
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Is_buy_modem { get; set; }
        public int Balance { get; set; }
        public string ModemType { get; set; }
        public string Message_Qc { get; set; }
        public bool Is_Subscription { get; set; }
        public string SubscriptionPackage { get; set; }
        public bool IsOtherUser { get; set; }
        public bool IsSTK { get; set; }

        public List<ListBuyModemEntry> ListBuyModem { get; set; }

    }

    public enum AccountSubmit
    {
        Login,
        Register
    }
}
