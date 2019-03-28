using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.Ngrok.ApiClient.Models
{


    public class NgrokTunnelsResponse
    {
        [JsonProperty(PropertyName = "tunnels")]
        public List<Tunnel> Tunnels { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }
    }










}
