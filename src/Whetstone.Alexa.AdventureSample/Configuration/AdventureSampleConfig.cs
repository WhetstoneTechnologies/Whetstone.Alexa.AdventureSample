using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Alexa.AdventureSample.Configuration
{
    public class AdventureSampleConfig
    {

        public string ConfigPath { get; set; }

        public string ConfigBucket { get; set; }


        public string AwsRegion { get; set; }

        public string DynamoDb { get; set; }

    }
}
