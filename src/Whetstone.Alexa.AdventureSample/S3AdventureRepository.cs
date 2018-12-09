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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.Alexa.AdventureSample.Configuration;
using Whetstone.Alexa.AdventureSample.Models;
using YamlDotNet.Serialization;

namespace Whetstone.Alexa.AdventureSample
{
    public class S3AdventureRepository : AdventureReposistoryBase, IAdventureRepository
    {
     
        private ILogger<S3AdventureRepository> _logger;

        public S3AdventureRepository(ILogger<S3AdventureRepository> logger, IOptions<AdventureSampleConfig> config)
        {
            if (logger == null)
                throw new ArgumentException("logger is null");

            if (config == null)
                throw new ArgumentException("config cannot be null");

            if (config.Value == null)
                throw new ArgumentException("config.Value cannot be null");

            AdventureSampleConfig advConfig = config.Value;

            if (string.IsNullOrWhiteSpace(advConfig.MediaContainerName))
                throw new Exception("ConfigBucket missing from configuration");

            //if (string.IsNullOrWhiteSpace(advConfig.ConfigPath))
            //    throw new Exception("ConfigPath missing from configuration");

            if (string.IsNullOrWhiteSpace(advConfig.AwsRegion))
                throw new Exception("AwsRegion missing from configuration");

            _logger = logger;
            _adventureConfig = advConfig;
        }

        public async Task<Adventure> GetAdventureAsync()
        {

            Adventure adv = await GetCachedAdventure();

            // TODO Cache the yaml file.



            return adv;
        }


        private async Task<Adventure> GetCachedAdventure()
        {

            string configPath = GetConfigPath();
        
            string textContents = await GetConfigTextContentsAsync(_adventureConfig.AwsRegion,
                                                                    _adventureConfig.MediaContainerName,
                                                                    configPath);

            return DeserializeAdventure(textContents);
        }


        private async Task<string> GetConfigTextContentsAsync(string regionName, string containerName, string path)
        {
            string configContents = null;
            try
            {
                RegionEndpoint endPoint = RegionEndpoint.GetBySystemName(regionName);

                using (IAmazonS3 client = new AmazonS3Client(endPoint))
                {

                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = containerName,
                        Key = path
                    };

                    _logger.LogInformation($"Getting text content from bucket {containerName} and path {path}");

                    using (GetObjectResponse response = await client.GetObjectAsync(request))
                    {
                        using (BufferedStream buffer = new BufferedStream(response.ResponseStream))
                        {
                            using (StreamReader reader = new StreamReader(buffer))
                            {
                                configContents = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (AmazonS3Exception s3Ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", s3Ex);

            }
            catch (AmazonServiceException servEx)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", servEx);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving text file {path} from bucket {containerName}", ex);
            }
            return configContents;
        }


    }
}
