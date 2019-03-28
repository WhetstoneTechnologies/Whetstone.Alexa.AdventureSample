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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Models;
using Xunit;
using YamlDotNet.Serialization;

namespace Whetstone.AdventureSample.IntegTests
{
    public class BlobStorageTest
    {

        [Fact]
        public async Task GetMissingRecord()
        {
            string storageCon = GetAzureContainerConnectionString();


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageCon);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("devsession");

            string appId = "amzn1.ask.skill.63830cb1-27a5-406c-8bf0-03b58ecab7e0";

            string userId = "amzn1.ask.account.AF4KN2MQATPLWG642W6JZ2O552ID3DV7TMWCLKUESJXNS5SYMF4UURDASAHPYLLMPYH3Q6UJLPKGMYTGBNBXH4AMMGCLIV4V26P67UAYUAU5QSERVCPNPBJF7B7RKFO72D756I74XWHY6JV3CPTURZ53OUAFRV7RISJHJQIIZXEZOZ2EC2PBKFBJBBEYKBGLFUZBKYB4SIPAS4Q";

            string userIdHash = GetStringSha256Hash(userId);

            TableOperation retrieveOperation = TableOperation.Retrieve<SessionStorageEntity>(appId, userIdHash);


            TableResult tabResult = await table.ExecuteAsync(retrieveOperation);

            SessionStorageEntity storageEntity = (SessionStorageEntity)tabResult.Result;

        }

        internal static string GetStringSha256Hash(string text)
        {
            if (String.IsNullOrEmpty(text))
                return String.Empty;

            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                byte[] textData = System.Text.Encoding.UTF8.GetBytes(text);
                byte[] hash = sha.ComputeHash(textData);
                return BitConverter.ToString(hash).Replace("-", String.Empty);
            }
        }


        [Fact]
        public async Task SetSessionStorage()
        {
            string storageCon = GetAzureContainerConnectionString();


            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageCon);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference("devsession");

            string appId = "someappid";

            string userId = "someuserid";

            // Create a new customer entity.
            SessionStorageEntity sessionEntity = new SessionStorageEntity(appId, userId);
            sessionEntity.CurrentNodeName = "CurNode";
            sessionEntity.LastAccessTime = DateTime.UtcNow;

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(sessionEntity);

            await table.ExecuteAsync(insertOperation);



            TableOperation retrieveOperation = TableOperation.Retrieve<SessionStorageEntity>(appId, userId);


            TableResult tabResult = await table.ExecuteAsync(retrieveOperation);

            SessionStorageEntity storageEntity = (SessionStorageEntity)tabResult.Result;

        }


        [Fact]
        public async Task GetBlobClientRepositoryTestAsync()
        {
            System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_CONTAINER, "titles");

            var builder = new ConfigurationBuilder()
              .AddEnvironmentVariables();
           
            IConfiguration config = builder.Build();

            IServiceCollection servCol = new ServiceCollection();
            

           await servCol.AddAdventureSampleServicesAsync(config);

            var servProv = servCol.BuildServiceProvider();

           var blobLogger =  servProv.GetRequiredService<ILogger<BlobAdventureRepository>>();

            var blobOptions = servProv.GetRequiredService<IOptions<AdventureSampleConfig>>();

            var distCache = servProv.GetRequiredService<IDistributedCache>();

            IAdventureRepository advRep = new BlobAdventureRepository(blobOptions, distCache, blobLogger);

            var advConfig = await advRep.GetAdventureAsync();

        }


        [Fact]
        public async Task GetBlobClientAsync()
        {
            // Use setx at the command line to save your Azure blob storage connection string
            // setx titles_storage_con "CONSTRING FROM AZURE PORTAL STORAGE ACCOUNT"


            string storageConnectionString = GetAzureContainerConnectionString();

            // Check whether the connection string can be parsed.
            CloudStorageAccount cloudStorageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out cloudStorageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob storage here.
                // ...


                // Create a CloudBlobClient object from the storage account.
                // This object is the root object for all operations on the 
                // blob service for this particular account.
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();

                // Get a reference to a CloudBlobContainer object in this account. 
                // This object can be used to create the container on the service, 
                // list blobs, delete the container, etc. This operation does not make a 
                // call to the Azure Storage service.  It neither creates the container 
                // on the service, nor validates its existence.
                CloudBlobContainer container = blobClient.GetContainerReference("titles");

                CloudBlockBlob blockBlob = container.GetBlockBlobReference("adventure.yaml");

                string yamlContents = await blockBlob.DownloadTextAsync(Encoding.UTF8, null, null, null);

                Deserializer deser = new Deserializer();

                Adventure adv = deser.Deserialize<Adventure>(yamlContents);

            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageConnection' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the sample application.");
                Console.ReadLine();
            }
        }

        [Fact]
        public async Task UploadAdventureFileAsync()
        {
            // Use setx at the command line to save your Azure blob storage connection string
            // setx titles_storage_con "CONSTRING FROM AZURE PORTAL STORAGE ACCOUNT"

            string yamlContentsText = File.ReadAllText("storytitles/adventure.yaml");

            string storageConnectionString = GetAzureContainerConnectionString();

            // Check whether the connection string can be parsed.
            CloudStorageAccount cloudStorageAccount;
            if (CloudStorageAccount.TryParse(storageConnectionString, out cloudStorageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob storage here.
                // ...
                
                CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference("titles");
                CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(@"adventuresample/adventure.yaml");
                await cloudBlockBlob.UploadTextAsync(yamlContentsText, Encoding.UTF8, null, null, null);
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageConnection' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the sample application.");
                Console.ReadLine();
            }




        }

        private string GetAzureContainerConnectionString()
        {
            string storageConnection = "titles_storage_con";


            string storageConnectionString = Environment.GetEnvironmentVariable(storageConnection);

            if (string.IsNullOrWhiteSpace(storageConnectionString))
                storageConnectionString = "UseDevelopmentStorage=true";

            storageConnectionString = "UseDevelopmentStorage=true";

            return storageConnectionString;

        }
    }
}
