using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class InfoPCData
    {
        public string name { get; set;}
        public string macAddress { get; set; }
    }
    public class COMDataInfo
    {
        
        public string modemName { get; set; }
        public string realCom { get; set; }
        public string virCom { get; set;  }
        public string phoneNumber { get; set;  }
        
        public string carrier { get; set; }

        public string serial { get; set; }
        public string imeiDevice { get; set; }
        
        public int mainBalance { get; set; }
        public string expired { get; set;  }

    }
    public class AddComInfoRequestData
    {
        public AddComInfoRequestData()
        {
            listCom = new List<COMDataInfo>();
        }
        [JsonProperty("infoPc")]
        public InfoPCData infoPc { get; set; }

        [JsonProperty("listCom")]
        public List<COMDataInfo> listCom { get; set; } //push list pc

    }
    public class AddInfoPCRequest : BaseGSMWebSocketRequest
    {
        
        [JsonProperty("data")]
        public AddComInfoRequestData  data {get;set;}
    }
    
}
