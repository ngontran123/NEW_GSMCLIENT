using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.GSM.Model
{
    public class SessionSendMessage
    {
        public List<SendMessageSource> ListMessageSources { get; set; }
        public string ReceivePhone { get; set; }
        public string ContentSms { get; set; }
    }
}
