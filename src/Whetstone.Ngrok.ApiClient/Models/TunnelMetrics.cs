using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Ngrok.ApiClient.Models
{
    [JsonObject(Description = "Reports on tunnel usage data.", Title = "nGrok Tunnel Metrics")]
    public class TunnelMetrics
    {

        [JsonProperty(PropertyName = "conns")]
        public TunnelConnections Connections { get; set; }

        [JsonProperty(PropertyName = "http")]
        public TunnelHttp Http { get; set; }
    }
}
