using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Ngrok.ApiClient.Models
{
    public class TunnelConfig
    {
        [JsonProperty(PropertyName ="addr")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "inspect")]
        public bool Inspect { get; set; }
    }
}
