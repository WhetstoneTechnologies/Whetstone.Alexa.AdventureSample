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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Models;
using Whetstone.Alexa;

namespace Whetstone.AdventureSample
{
    public class TableCurrentNodeRepository : SessionStoreRepositoryBase, ICurrentNodeRepository
    {

        private ILogger<TableCurrentNodeRepository> _logger;
        private readonly string _storageTableName = null;
        private readonly string _storageConnectionString = null;

        public TableCurrentNodeRepository(ILogger<TableCurrentNodeRepository> logger, IOptions<AdventureSampleConfig> config)
        {
            if (logger == null)
                throw new ArgumentException("logger cannot be null");

            if (config == null)
                throw new ArgumentException("config cannot be null");

            if (config.Value == null)
                throw new ArgumentException("config.Value cannot be null");

            _storageTableName = config.Value.UserStateTableName;
            _storageConnectionString = config.Value.AzureConfigConnectionString;

            if (string.IsNullOrWhiteSpace(_storageTableName))
                throw new ArgumentException("StorageTableName must be set");

            if (string.IsNullOrWhiteSpace(_storageConnectionString))
                throw new ArgumentException("StorageConnectionString value must be set on config");

            _logger = logger;
        }


        public override async Task<string> GetCurrentNodeNameAsync(AlexaRequest req)
        {
            string curNodeName = null;

            string userId = this.GetUserId(req);

            string appId = req.Session?.Application?.ApplicationId;

            string userIdHash = GetStringSha256Hash(userId);

            TableOperation retrieveOperation = TableOperation.Retrieve<SessionStorageEntity>(appId, userIdHash);

            CloudTable sessionTable = GetCloudTable();
            TableResult tabResult = null;
         
            tabResult = await sessionTable.ExecuteAsync(retrieveOperation);

            if (tabResult.HttpStatusCode == 200)
            {
                SessionStorageEntity storageEntity = (SessionStorageEntity)tabResult.Result;
                curNodeName = storageEntity.CurrentNodeName;
            }
            return curNodeName;
        }

        public async Task SetCurrentNodeAsync(AlexaRequest req, AlexaResponse resp, string currentNodeName)
        {
            
            CloudTable sessionTable = GetCloudTable();

            // Create a new customer entity.

            string userId = this.GetUserId(req);

            string appId = req.Session?.Application?.ApplicationId;

            string userIdHash = GetStringSha256Hash(userId);

            SessionStorageEntity sessionEntity = new SessionStorageEntity(appId, userIdHash);

            sessionEntity.CurrentNodeName = currentNodeName;
            sessionEntity.LastAccessTime = DateTime.UtcNow;
           

            // Create the TableOperation object that inserts the customer entity.
            TableOperation insertOperation = TableOperation.InsertOrReplace(sessionEntity);

            await sessionTable.ExecuteAsync(insertOperation);



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


        private CloudTable GetCloudTable()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(_storageTableName);

            return table;
        }
    }
}
