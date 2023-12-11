using DevExpress.Xpo;
using Newtonsoft.Json;
using System;

namespace C3TekClient.Core.GSMWebSocket.Model
{
    public class AddProxyRequestData : XPObject
    {
        public AddProxyRequestData() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public AddProxyRequestData(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }
        public class AddProxyRequest : BaseGSMWebSocketRequest
        {

            [JsonProperty("data")]
            public AddProxyRequestData data { get; set; }
        }
    }

}