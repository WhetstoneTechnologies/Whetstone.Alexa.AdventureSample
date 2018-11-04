using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Configuration;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public class DynamoDbCurrentNodeRepository : ICurrentNodeRepository
    {

        private ILogger<DynamoDbCurrentNodeRepository> _logger;
        private string _dynamoDbName;
        private string _awsRegion;

        public DynamoDbCurrentNodeRepository(ILogger<DynamoDbCurrentNodeRepository> logger, IOptions<AdventureSampleConfig> config)
        {
            if (logger == null)
                throw new ArgumentException("logger cannot be null");

            if (config == null)
                throw new ArgumentException("config cannot be null");

            if (config.Value == null)
                throw new ArgumentException("config.Value cannot be null");

            string dynamoDbVal = config.Value.DynamoDb;
            string awsRegionVal = config.Value.AwsRegion;

            if (string.IsNullOrWhiteSpace(dynamoDbVal))
                throw new ArgumentException("DynamoDb value must be set on config");

            if (string.IsNullOrWhiteSpace(awsRegionVal))
                throw new ArgumentException("AwsRegion value must be set on config");

            _logger = logger;
            _awsRegion = awsRegionVal;

            _dynamoDbName = dynamoDbVal;

        }


        public async Task<AdventureNode> GetCurrentNodeAsync(AlexaRequest req, IEnumerable<AdventureNode> nodes)
        {
            string curNodeName = await GetCurrentNodeNameAsync(req);
            AdventureNode retNode = null;

            
            if((nodes?.Any()).GetValueOrDefault(false) && !string.IsNullOrWhiteSpace(curNodeName))
            {
                retNode = nodes.FirstOrDefault(x => x.Name.Equals(curNodeName, StringComparison.OrdinalIgnoreCase));

            }

            return retNode;
        }

        public async Task<string> GetCurrentNodeNameAsync(AlexaRequest req)
        {
            string userId = GetUserId(req);
            string curNodeName = null;


            UserRecord userRec = await GetUserAsync(userId);

            if (userRec != null)
                curNodeName = userRec.CurrentNodeName;

            return curNodeName;
        }


        public async Task<UserRecord> GetUserAsync(string userId)
        {

            UserRecord userRec = new UserRecord();
            userRec.UserId = userId;
            RegionEndpoint regionUrl = Amazon.RegionEndpoint.GetBySystemName(_awsRegion);
            using (AmazonDynamoDBClient client = new AmazonDynamoDBClient(regionUrl))
            {

                var getUserRequest = new GetItemRequest
                {
                    TableName = _dynamoDbName,
                    Key = new Dictionary<string, AttributeValue>() {
                        { "userId", new AttributeValue { S = userId} }
                    },
                };

                GetItemResponse response = await client.GetItemAsync(getUserRequest);
                var result = response?.Item;

                if (result?.Count > 0)
                {               
                    string curNodeName = result["curNode"].S;
                    userRec.CurrentNodeName = curNodeName;


                    DateTime lastAccessDate = DateTime.Parse(result["lastAccessedDate"].S, null, System.Globalization.DateTimeStyles.RoundtripKind);
                    userRec.LastAccessedDate = lastAccessDate;
                }
            }

            return userRec;
        }

        private string GetUserId(AlexaRequest req)
        {
            string userId = req?.Session?.User?.UserId;

            if (string.IsNullOrWhiteSpace(userId))
                _logger.LogTrace("userId not found in request in DynamoDbCurrentNodeRepository");

            return userId;
        }

        public async Task SetCurrentNodeAsync(AlexaRequest req, AlexaResponse resp, string currentNodeName)
        {

            string userId = GetUserId(req);

            UserRecord userRec = new UserRecord()
            {
                UserId = userId,
                CurrentNodeName = currentNodeName
            };
            

            // This is an upsert
            await UpdateUserNodeAsync(userRec);
           

            if(resp.SessionAttributes == null)
            {
                resp.SessionAttributes = new Dictionary<string, dynamic>();
            }

            resp.SessionAttributes.Add(AdventureNode.CURNODE_ATTRIB, currentNodeName);

        }



        private async Task UpdateUserNodeAsync(UserRecord user)
        {


            RegionEndpoint regionUrl = Amazon.RegionEndpoint.GetBySystemName(_awsRegion);
            using (AmazonDynamoDBClient client = new AmazonDynamoDBClient(regionUrl))
            {

                var updateRequest = new UpdateItemRequest
                {
                    Key = new Dictionary<string, AttributeValue>() { { "userId", new AttributeValue { S =user.UserId
                } } },
                    // Update price only if the current price is 20.00.
                    ExpressionAttributeNames = new Dictionary<string, string>()
                {
                   
                    {"#C", "curNode" },
                    {"#D", "lastAccessedDate" }
                },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
      
                    {":curNode",new AttributeValue {S = user.CurrentNodeName}},
                    {":lastAccessedDate", new AttributeValue{ S =DateTime.UtcNow.ToString("o") } }
                },
                    UpdateExpression = "SET #C = :curNode, #D = :lastAccessedDate",
                    TableName = _dynamoDbName,
                    ReturnValues = "NONE" // Return all the attributes of the updated item.
                };

                try
                {

                    await client.UpdateItemAsync(updateRequest);
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, $"Error updating user id {user.UserId} with new node {user.CurrentNodeName}");
                    throw;

                }

            }
        }
    }
}
