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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Configuration;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public class BlobAdventureRepository : AdventureReposistoryBase, IAdventureRepository
    {
        private ILogger<BlobAdventureRepository> _logger;

        public BlobAdventureRepository(ILogger<BlobAdventureRepository> logger, IOptions<AdventureSampleConfig> config)
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


            _logger = logger;
            _adventureConfig = advConfig;
        }



        public async Task<Adventure> GetAdventureAsync()
        {

            Adventure retAdventure = null;

            string titlePath = GetConfigPath();

            string titleText = await GetStoredAdventureAsync(titlePath, _adventureConfig.MediaContainerName,
                _adventureConfig.AzureConfigConnectionString);

            retAdventure = DeserializeAdventure(titleText);

            return retAdventure;
        }



        private async Task<string> GetStoredAdventureAsync(string titlePath, string containerName, string connectionString)
        {

            string yamlContents = null;
            CloudStorageAccount cloudStorageAccount = null;

            //  RoleEnvironment.
            // CloudStorageAccount.TryParse() Role


            if (CloudStorageAccount.TryParse(connectionString, out cloudStorageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob storage here.
                // ...
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("titles");
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(titlePath);
                yamlContents = await blockBlob.DownloadTextAsync(Encoding.UTF8, null, null, null);
            }
            else
            {
                _logger.LogError("Could not connect to storage account");
            }

            return yamlContents;
        }

        //private async Task<string> GetStoredAdventureAsync(string titlePath, string containerName)
        //{

        //    string yamlContents = null;
        //    CloudStorageAccount cloudStorageAccount;

        //    string storageConnection = "titles_storage_con";


        //    // Retrieve the connection string for use with the application. The storage connection string is stored
        //    // in an environment variable on the machine running the application called storageConnection.
        //    // If the environment variable is created after the application is launched in a console or with Visual
        //    // Studio, the shell or application needs to be closed and reloaded to take the environment variable into account.
        //    string storageConnectionString = Environment.GetEnvironmentVariable(storageConnection);

        //    if (CloudStorageAccount.TryParse(storageConnectionString, out cloudStorageAccount))
        //    {
        //        // If the connection string is valid, proceed with operations against Blob storage here.
        //        // ...
        //        CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
        //        CloudBlobContainer container = blobClient.GetContainerReference("titles");
        //        CloudBlockBlob blockBlob = container.GetBlockBlobReference(titlePath);
        //        yamlContents = await blockBlob.DownloadTextAsync(Encoding.u, null, null, null);
        //    }
        //    else
        //    {
        //        _logger.LogError("Could not connect to storage account");
        //    }

        //    return yamlContents;
        //}
    }
}
