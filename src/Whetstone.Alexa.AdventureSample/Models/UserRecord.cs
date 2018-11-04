using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Alexa.AdventureSample.Models
{
    public class UserRecord
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }


        [JsonProperty(PropertyName = "curNode", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrentNodeName { get; set; }

        [JsonProperty(PropertyName = "lastAccessedDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime LastAccessedDate { get; set; }
    }
}
