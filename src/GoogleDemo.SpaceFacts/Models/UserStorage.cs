using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleDemo.SpaceFacts.Models
{
    [JsonObject]
    public class UserStorage
    {

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
