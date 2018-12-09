// Copyright (c) 2018 Whetstone Technologies. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.Alexa.AdventureSample.Configuration
{
    public class AdventureSampleConfig
    {
        /// <summary>
        /// Root path of the media files.
        /// </summary>
        /// <value>
        /// Path to the media files (e.g. storymedia/adventuresample/)
        /// </value>
        public string MediaPath { get; set; }


        /// <summary>
        /// Bucket or AzureBlob container than hosts the media files.
        /// </summary>
        /// <value>
        /// The name of the media container.
        /// </value>
        public string MediaContainerName { get; set; }


        /// <summary>
        /// Azure needs to know the name of the media container account. This is not required for AWS.
        /// </summary>
        /// <value>
        /// The name of the media container account.
        /// </value>
        public string MediaContainerAccountName { get; set; }

        public string ConfigContainerName { get; set; }

        /// <summary>
        /// Either the AzureTable or the DynamoDb table name to use to store user state.
        /// </summary>
        /// <value>
        /// The name of the user state table.
        /// </value>
        public string UserStateTableName { get; set; }

        /// <summary>
        /// Path to the directory that contains the YAML configuration file. 
        /// </summary>
        /// <value>
        /// The configuration path to the YAML file.
        /// </value>
        public string ConfigPath { get; set; }

        /// <summary>
        /// Azure storage container string. This container string is used by both the 
        /// </summary>
        /// <value>
        /// The azure configuration connection string.
        /// </value>
        public string AzureConfigConnectionString { get; set; }

        public string AwsRegion { get; set; }


    }
}
