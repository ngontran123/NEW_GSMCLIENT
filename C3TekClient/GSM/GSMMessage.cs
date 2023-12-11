using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM
{
    public class GSMMessage
    {
        public string DisplayCOMName { get; set; }
        public string COM { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Carrier { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
        public string OTP { get; set; }
        public MessageType MessageType { get; set; }
    }
    public class SendMessageData
    {
        public string Receiver { get; set; }
        public string Content { get; set; }
    }
    public enum MessageType
    {
        SMS,
        Voice
    }
}
