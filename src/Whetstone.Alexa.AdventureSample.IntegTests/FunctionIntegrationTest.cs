using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.Alexa.AdventureSample.Configuration;
using Whetstone.Alexa.AdventureSample.Lambda;
using Xunit;

namespace Whetstone.Alexa.AdventureSample.IntegTests
{
    public class FunctionIntegrationTest
    {

        [Trait("Type", "UnitTest")]
        [Fact]
        public async Task FireLaunchRequestTest()
        {

            string userId = Guid.NewGuid().ToString();

            AlexaRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

            IServiceProvider servProv = GetIntegratedProvider();

            var function = new Function(servProv);
            var context = new TestLambdaContext();

            AlexaResponse resp = null;

            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            // No email returned
            Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

            // No directives should be returned since the request does not support the Echo Show
            Assert.Null(resp.Response.Directives);

            req = GenerateIntentRequest(userId, "BeginIntent");
            string currentNode = null;
            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)  resp.SessionAttributes["currentnode"];

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            Assert.Equal("PathStart", currentNode);

            req = GenerateIntentRequest(userId, "LeftIntent");
            req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
            req.Session.Attributes.Add("currentnode", currentNode);

            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }


            Assert.Equal("TrollInPath", currentNode);


            req = GenerateIntentRequest(userId, "PunchIntent");
            req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
            req.Session.Attributes.Add("currentnode", currentNode);


            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }



        }


        [Trait("Type", "UnitTest")]
        [Fact]
        public async Task LeftPathTest()
        {
            string userId = Guid.NewGuid().ToString();


            AlexaRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

            IServiceProvider servProv = GetIntegratedProvider();

            var function = new Function(servProv);
            var context = new TestLambdaContext();

            AlexaResponse resp = null;

            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            // No email returned
            Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

            // No directives should be returned since the request does not support the Echo Show
            Assert.Null(resp.Response.Directives);



            req = GenerateIntentRequest(userId,"BeginIntent");
            string currentNode = null;
            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            Assert.Equal("PathStart", currentNode);

            req = GenerateIntentRequest(userId, "RightIntent");
            req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
            req.Session.Attributes.Add("currentnode", currentNode);

            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }


            Assert.Equal("HedgeNode", currentNode);


            req = GenerateIntentRequest(userId, "SearchHedgeIntent");
            req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
            req.Session.Attributes.Add("currentnode", currentNode);


            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            Assert.Equal("SearchHedge", currentNode);


            req = GenerateIntentRequest(userId, "WalkIntent");
            req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
            req.Session.Attributes.Add("currentnode", currentNode);


            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            Assert.Equal("OutOfWoods", currentNode);


            Assert.True(resp.Response.ShouldEndSession);

        }


        [Trait("Type", "UnitTest")]
        [Fact]
        public async Task FireBadBeginRequestTest()
        {

            string userId = Guid.NewGuid().ToString();

            AlexaRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

            IServiceProvider servProv = GetIntegratedProvider();

            var function = new Function(servProv);
            var context = new TestLambdaContext();

            AlexaResponse resp = null;

            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

            // No email returned
            Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

            // No directives should be returned since the request does not support the Echo Show
            Assert.Null(resp.Response.Directives);

            req = GenerateIntentRequest(userId, "WalkIntent");
            string currentNode = null;
            try
            {
                resp = await function.FunctionHandlerAsync(req, context);
                currentNode = (string)resp.SessionAttributes["currentnode"];

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }


        }

            private IServiceProvider GetIntegratedProvider()
        {

            System.Environment.SetEnvironmentVariable(ServiceExtensions.BUCKET_CONFIG, "dev-custom");
            System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_PATH, "adventuresample");
            System.Environment.SetEnvironmentVariable(ServiceExtensions.DYNAMODB_CONFIG, "dev-session");

            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            //  IServiceProvider serProv 

            IServiceCollection servCol = new ServiceCollection();

            servCol.AddAdventureSampleServices(config);

            IServiceProvider prov = servCol.BuildServiceProvider();


            return prov;

        }

        private AlexaRequest GenerateIntentRequest(string userId, string intentName)
        {
            AlexaRequest intReq = GenerateRequest(userId, RequestType.IntentRequest);

            intReq.Request.Intent = new IntentAttributes()
            {
                Name = intentName

            };

            return intReq;
        }

        private AlexaRequest GenerateRequest(string userId, RequestType reqType)
        {


            AlexaRequest req = new AlexaRequest();

            req.Version = "1.0";

            req.Request = new RequestAttributes()
            {
                RequestId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Locale = "en-US",
                Type = reqType
            };

            req.Session = new AlexaSessionAttributes()
            {
                New = true,
                SessionId = Guid.NewGuid().ToString(),
                User = new UserAttributes()
                {
                    UserId = userId
                 }
            };

            req.Context = new ContextAttributes()
            {
                System = new SystemAttributes()
                {
                    ApiEndpoint = "https://api.amazonalexa.com",
                    ApiAccessToken = "SOMETOKEN",
                    Device = new DeviceAttributes()
                    {
                        SupportedInterfaces = new SupportedInterfacesAttributes()
                    }
                }
            };

            return req;
        }


    }
}
