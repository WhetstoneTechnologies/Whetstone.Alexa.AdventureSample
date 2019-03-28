using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Ngrok.ApiClient.Models
{

    public class TunnelHttp
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "rate1")]
        public decimal Rate01 { get; set; }

        [JsonProperty(PropertyName = "rate5")]
        public decimal Rate05 { get; set; }

        [JsonProperty(PropertyName = "rate15")]
        public decimal Rate15 { get; set; }

        [JsonProperty(PropertyName = "p50")]
        public decimal P50 { get; set; }

        [JsonProperty(PropertyName = "p90")]
        public decimal P90 { get; set; }

        [JsonProperty(PropertyName = "p95")]
        public decimal P95 { get; set; }

        [JsonProperty(PropertyName = "p99")]
        public decimal P99 { get; set; }
    }
}
