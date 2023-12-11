using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class GetListTaskRequest : BaseGSMWebSocketRequest
    {
        
        public int status { get; set; } //0->5
        public int page { get; set; }
    }
}
