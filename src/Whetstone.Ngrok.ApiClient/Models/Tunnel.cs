using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.Ngrok.ApiClient.Models
{

    [DebuggerDisplay("Name = {Name}, PublicUrl = {PublicUrl}")]
    [JsonObject(Description ="Details, including metrics, about an active nGrok tunnel", Title ="nGrok Tunnel")]
    public class Tunnel
    {
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Uri")]
        public string uri { get; set; }

        [JsonProperty(PropertyName = "public_url")]
        public string PublicUrl { get; set; }

        [JsonProperty(PropertyName = "proto")]
        public string Proto { get; set; }

        [JsonProperty(PropertyName = "config")]
        public TunnelConfig Config { get; set; }

        [JsonProperty(PropertyName = "metrics")]
        public TunnelMetrics Metrics { get; set; }
    }

}
