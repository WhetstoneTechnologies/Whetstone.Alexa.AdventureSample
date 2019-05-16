// Copyright (c) 2019 Whetstone Technologies. All rights reserved.
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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading.Tasks;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Models;

namespace Whetstone.AdventureSample
{
    public class BlobAdventureRepository : AdventureReposistoryBase, IAdventureRepository
    {
        private ILogger<BlobAdventureRepository> _logger;

        public BlobAdventureRepository(IOptions<AdventureSampleConfig> config, 
                                       IDistributedCache cache, 
                                       ILogger<BlobAdventureRepository> logger) : base(config, cache)
        {
            _logger = logger ?? throw new ArgumentException("logger is null");
           
        }



        public async Task<Adventure> GetAdventureAsync()
        {

            Adventure retAdventure = await GetCachedAdventureAsync();


            if (retAdventure == null)
            {
                string titleText = await GetStoredAdventureAsync(_adventureConfig.AzureConfigConnectionString);

                retAdventure = DeserializeAdventure(titleText);

                await SetCachedAdventureAsync(retAdventure);
            }


            return retAdventure;
        }



        private async Task<string> GetStoredAdventureAsync(string connectionString)
        {

            string yamlContents = null;
            CloudStorageAccount cloudStorageAccount = null;

            if (CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob storage here.
                // ...
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = null;

                try
                {
                  container=  blobClient.GetContainerReference(_configContainerName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting file container {_configContainerName} and account {cloudStorageAccount.BlobEndpoint.ToString()}");

                }

                if (container != null)
                {
                    string configPath = GetConfigPath();

                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(configPath);

                    try
                    {
                        yamlContents = await blockBlob.DownloadTextAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"Error getting file {configPath} from container {_configContainerName} and account {cloudStorageAccount.BlobEndpoint.ToString()}");

                    }
                }
            }
            else
            {
                _logger.LogError("Could not connect to storage account");
            }

            return yamlContents;
        }
        
    }
}
