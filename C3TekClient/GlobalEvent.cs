using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient
{
    public static class GlobalEvent
    {
        public static Action<string> ONATCommandResponse = (response) => { };
        public static Action<string> OnGlobalMessaging = (message) => { };
        public static Action<string> OnJobLog = (message) => { };
    }
}
