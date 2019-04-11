using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GoogleDemo.SpaceFacts.Models;
using System.Linq;

namespace GoogleDemo.SpaceFacts
{
    public static class GoogleSpaceFactsFunction
    {
        private const string GET_FACT_MESSAGE = "Here's your fact: ";
        private const string HELP_MESSAGE = "You can say tell me a space fact, or, you can say exit... What can I help you with?";
        private const string HELP_REPROMPT = "What can I help you with?";
        private const string STOP_MESSAGE = "Goodbye!";


        private static readonly string INTENT_MAIN = "actions.intent.MAIN";

        /// <summary>
        /// This is provided if the user does not respond to a Google Action prompt or reprompt.
        /// </summary>
        private static readonly string INTENT_NOINPUT = "actions.intent.NO_INPUT";

        private static readonly ThreadLocal<Random> threadLocalRandom
            = new ThreadLocal<Random>(() => new Random());

        internal enum SpaceRequestType
        {
            GetFact,
            Help,
            Stop
        }


        private static readonly List<string> SpaceFacts = new List<string>()
        {
            "A year on Mercury is just 88 days long.",
            "Despite being farther from the Sun, Venus experiences higher temperatures than Mercury.",
            "Venus rotates counter-clockwise, possibly because of a collision in the past with an asteroid.",
            "On Mars, the Sun appears about half the size as it does on Earth.",
            "Earth is the only planet not named after a god.",
            "Jupiter has the shortest day of all the planets.",
            "The Milky Way galaxy will collide with the Andromeda Galaxy in about 5 billion years.",
            "The Sun contains 99.86% of the mass in the Solar System.",
            "The Sun is an almost perfect sphere.",
            "A total solar eclipse can happen once every 1 to 2 years. This makes them a rare event.",
            "Saturn radiates two and a half times more energy into space than it receives from the sun.",
            "The temperature inside the Sun can reach 15 million degrees Celsius.",
            "The Moon is moving approximately 3.8 cm away from our planet every year."
        };


        private static string GetRandomFact()
        {
            int factLimit = SpaceFacts.Count - 1;
            int factIndex = threadLocalRandom.Value.Next(0, factLimit);
            return SpaceFacts[factIndex];
        }

        private static readonly JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        [FunctionName("GoogleSpaceFactsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            WebhookRequest request = null;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogDebug($"Body text: {requestBody}");

            // Check if it is a conversation web hook health check

            bool isHealthCheck = IsHealthCheck(requestBody, log);

            string responseJson = null;
            string userId = null;
            SpaceRequestType requestType;

            if (isHealthCheck)
            {
                // a health check just expects a properly formatted response. If the request
                // is a health check, then generate a fact response with a random user Id.
                userId = Guid.NewGuid().ToString("N");
                requestType = SpaceRequestType.GetFact;
            }
            else
            {
                try
                {
                    request = jsonParser.Parse<WebhookRequest>(requestBody);
                }
                catch (InvalidProtocolBufferException ex)
                {

                    log.LogError(ex, "Web hook request could not be parsed.");
                    return new BadRequestObjectResult("Error deserializing Dialog flow request from message body");
                }

                userId = GetUserId(request);
                string intent = request.QueryResult.Intent.DisplayName;
                requestType = GetSpaceRequestType(intent);
            }

            WebhookResponse webHookResp = null;

            switch (requestType)
            {
                case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();
                    string factText = string.Concat(GET_FACT_MESSAGE, spaceFact);
                    webHookResp = GetDialogFlowResponse(userId, factText);
                    break;
                case SpaceRequestType.Help:
                    webHookResp = GetDialogFlowResponse(userId, HELP_MESSAGE, HELP_REPROMPT);
                    break;
                case SpaceRequestType.Stop:
                    webHookResp = GetDialogFlowResponse(userId, STOP_MESSAGE);
                    break;
            }

            responseJson = webHookResp?.ToString();

            ContentResult contRes = new ContentResult()
            {
                Content = responseJson,
                ContentType = "application/json",
                StatusCode = 200
            };

            return contRes;
        }

        private static WebhookResponse GetDialogFlowResponse(string userId, string message)
        {


            return GetDialogFlowResponse(userId, message, null);

        }


        private static WebhookResponse GetDialogFlowResponse(string userId, string message, string reprompt)
        {
            bool expectUserResp = !string.IsNullOrWhiteSpace(reprompt);

            WebhookResponse webHookResp = InitializeResponse(expectUserResp, userId);


            var fulfillmentMessage = webHookResp.FulfillmentMessages[0];

            fulfillmentMessage.SimpleResponses = new Intent.Types.Message.Types.SimpleResponses();
            var simpleResp = new Intent.Types.Message.Types.SimpleResponse();
            simpleResp.Ssml = $"<speak>{message}</speak>";           
            fulfillmentMessage.SimpleResponses.SimpleResponses_.Add(simpleResp);

            return webHookResp;

        }


        private static WebhookResponse InitializeResponse(bool expectUserInput, string userId)
        {
            WebhookResponse webResp = new WebhookResponse();

            var message = new Intent.Types.Message();
            webResp.FulfillmentMessages.Add(message);
            message.Platform = Intent.Types.Message.Types.Platform.ActionsOnGoogle;

            // var payload = Struct.Parser.ParseJson("{\"google\": { \"expectUserResponse\": true}} ");


            //message.Payload = new
            Value payloadVal = new Value();
            payloadVal.StructValue = new Struct();

            Value expectedUserResp = new Value();
            expectedUserResp.BoolValue = expectUserInput;
            payloadVal.StructValue.Fields.Add("expectUserResponse", expectedUserResp);

            Value userStorageValue = new Value();

            UserStorage userStorage = new UserStorage();
            userStorage.UserId = userId;
            userStorageValue.StringValue = JsonConvert.SerializeObject(userStorage);

            payloadVal.StructValue.Fields.Add("userStorage", userStorageValue);

            Struct payloadStruct = new Struct();

            payloadStruct.Fields.Add("google", payloadVal);


            webResp.Payload = payloadStruct;




            return webResp;
        }


        private static SpaceRequestType GetSpaceRequestType(string intentName)
        {
            SpaceRequestType spaceReqType = SpaceRequestType.Help;

           
            if(intentName.Equals("Default Welcome Intent", StringComparison.OrdinalIgnoreCase) ||
                   intentName.Equals("GetNewFactIntent", StringComparison.OrdinalIgnoreCase))
            {
                spaceReqType = SpaceRequestType.GetFact;
            }
          
            return spaceReqType;
        }


        internal static bool IsRepromptRequest(WebhookRequest request)
        {
            bool isRepromptRequest = false;

            ListValue inputs = request.OriginalDetectIntentRequest.Payload?.Fields?["inputs"]?.ListValue;

            if (inputs != null)
            {

                bool isIntentFound = false;

                int valCount = 0;
                while (!isIntentFound && valCount < inputs.Values.Count)
                {

                    Value val = inputs.Values[valCount];

                    if (val.StructValue != null)
                    {
                        if (val.StructValue.Fields["intent"] != null)
                        {
                            string intentName = val.StructValue.Fields["intent"].StringValue;
                            isIntentFound = true;

                            if (intentName.Equals(INTENT_NOINPUT, StringComparison.OrdinalIgnoreCase))
                            {
                                isRepromptRequest = true;

                            }
                        }
                    }

                    valCount++;

                }

            }



            return isRepromptRequest;
        }

        private static string GetUserId(WebhookRequest request)
        {

            string userId = null;
            Struct intentRequestPayload = request.OriginalDetectIntentRequest?.Payload;


            var userStruct = intentRequestPayload.Fields?["user"];

            string userStorageText = null;
            if ((userStruct?.StructValue?.Fields?.Keys.Contains("userStorage")).GetValueOrDefault(false))
            {
                userStorageText = userStruct?.StructValue?.Fields?["userStorage"]?.StringValue;
            }

            if (!string.IsNullOrWhiteSpace(userStorageText))
            {
                UserStorage userStore = JsonConvert.DeserializeObject<UserStorage>(userStorageText);

                userId  = userStore.UserId;
            }
            else
            {
                if ((userStruct?.StructValue?.Fields?.Keys.Contains("userId")).GetValueOrDefault(false))
                {
                    userId = userStruct?.StructValue?.Fields?["userId"]?.StringValue;
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    // The user Id is not provided. Generate a new one and return it.
                    userId = Guid.NewGuid().ToString("N");

                }
            }

            return userId;
        }



        public static bool IsHealthCheck(string requestBody, ILogger log)
        {

            bool isHealthCheck = false;
            ConversationRequest convReq = null;

            try
            {
                convReq = JsonConvert.DeserializeObject<ConversationRequest>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogInformation(ex, "Web hook request is not a health check. Cannot deserialize request as a health check.");
            }


            var firstInput = convReq?.Inputs?.FirstOrDefault(x => (x.Intent?.Equals(INTENT_MAIN)).GetValueOrDefault(false));
            
            if (firstInput != null)
            {
                var arg = firstInput.Arguments?.FirstOrDefault(x => (x.Name?.Equals("is_health_check")).GetValueOrDefault(false));

                if (arg != null)
                {
                    isHealthCheck = arg.BoolValue;
                }
            }

            return isHealthCheck;
        }
    }
}
