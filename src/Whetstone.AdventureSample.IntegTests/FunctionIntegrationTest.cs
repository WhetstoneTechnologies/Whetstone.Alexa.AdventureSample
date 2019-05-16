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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.AdventureSample.Configuration;
using Xunit;
using Whetstone.AdventureSample.CoreFunction.Alexa;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Whetstone.Alexa.Security;
using Whetstone.Ngrok.ApiClient;
using Whetstone.Ngrok.ApiClient.Models;

namespace Whetstone.AdventureSample.IntegTests.Alexa
{
    public class FunctionIntegrationTest
    {

        private const string TESTAPPGUID = "32AB72A4-34C6-4BC3-A8A7-39261F6242E9";
        

        [Trait("Type", "UnitTest")]
        [Fact]
        public async Task FireLaunchRequestTest()
        {

            string userId = Guid.NewGuid().ToString();

            HttpRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

            IServiceProvider servProv = await GetIntegratedProvider();

            var advProcessor = servProv.GetService<IAdventureSampleProcessor>();

            var alexaVerifier = servProv.GetService<IAlexaRequestVerifier>();

            AlexaResponse resp = null;

            ILogger logger = GetLogger();

            await SendAlexaRequestAsync(req, logger, alexaVerifier, advProcessor);

            // No email returned
            //Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

            //// No directives should be returned since the request does not support the Echo Show
            //Assert.Null(resp.Response.Directives);

            req = GenerateIntentRequest(userId, "BeginIntent");

            await SendAlexaRequestAsync(req, logger, alexaVerifier, advProcessor);

            req = GenerateIntentRequest(userId, "LeftIntent");
            await SendAlexaRequestAsync(req, logger, alexaVerifier, advProcessor);

            //Assert.Equal("TrollInPath", currentNode);


            req = GenerateIntentRequest(userId, "PunchIntent");
            await SendAlexaRequestAsync(req, logger, alexaVerifier, advProcessor);

        }

        private async Task SendAlexaRequestAsync(HttpRequest req, ILogger logger, IAlexaRequestVerifier alexaVerifier, IAdventureSampleProcessor advSampleProcessor)
        {
            try
            {
                AlexaFunction alexaFunc = new AlexaFunction(alexaVerifier, advSampleProcessor);

                IActionResult res = await alexaFunc.Run(req, logger);

                ProcessActionResult(res);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }



        //[Trait("Type", "UnitTest")]
        //[Fact]
        //public async Task LeftPathTest()
        //{
        //    string userId = Guid.NewGuid().ToString();


        //   HttpRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

        //    IServiceProvider servProv = GetIntegratedProvider();

        //    ConsoleLoggerProvider conProv;


        //    AlexaResponse resp = null;

        //    try
        //    {
        //        resp = await AlexaFunction.Run(req,  )
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }

        //    // No email returned
        //    Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

        //    // No directives should be returned since the request does not support the Echo Show
        //    Assert.Null(resp.Response.Directives);



        //    req = GenerateIntentRequest(userId, "BeginIntent");
        //    string currentNode = null;
        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //        currentNode = (string)resp.SessionAttributes["currentnode"];

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }

        //    Assert.Equal("PathStart", currentNode);

        //    req = GenerateIntentRequest(userId, "RightIntent");
        //    req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
        //    req.Session.Attributes.Add("currentnode", currentNode);

        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //        currentNode = (string)resp.SessionAttributes["currentnode"];

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }


        //    Assert.Equal("HedgeNode", currentNode);


        //    req = GenerateIntentRequest(userId, "SearchHedgeIntent");
        //    req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
        //    req.Session.Attributes.Add("currentnode", currentNode);


        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //        currentNode = (string)resp.SessionAttributes["currentnode"];
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }

        //    Assert.Equal("SearchHedge", currentNode);


        //    req = GenerateIntentRequest(userId, "WalkIntent");
        //    req.Session.Attributes = new System.Collections.Generic.Dictionary<string, dynamic>();
        //    req.Session.Attributes.Add("currentnode", currentNode);


        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //        currentNode = (string)resp.SessionAttributes["currentnode"];
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }

        //    Assert.Equal("OutOfWoods", currentNode);


        //    Assert.True(resp.Response.ShouldEndSession);

        //}


        //[Trait("Type", "UnitTest")]
        //[Fact]
        //public async Task FireBadBeginRequestTest()
        //{

        //    string userId = Guid.NewGuid().ToString();

        //    AlexaRequest req = GenerateRequest(userId, RequestType.LaunchRequest);

        //    IServiceProvider servProv = GetIntegratedProvider();

        //    var function = new AdventureFunction(servProv);
        //    var context = new TestLambdaContext();

        //    AlexaResponse resp = null;

        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }

        //    // No email returned
        //    Assert.Contains("Welcome to the Adventure Sample", resp.Response.OutputSpeech.Text);

        //    // No directives should be returned since the request does not support the Echo Show
        //    Assert.Null(resp.Response.Directives);

        //    req = GenerateIntentRequest(userId, "WalkIntent");
        //    string currentNode = null;
        //    try
        //    {
        //        resp = await function.FunctionHandlerAsync(req, context);
        //        currentNode = (string)resp.SessionAttributes["currentnode"];

        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex);
        //        throw;
        //    }


        //}

        private async Task<IServiceProvider> GetIntegratedProvider()
        {
            string mediacontainer = "skillsmedia";

            //System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_CONTAINER, "dev-custom");
            //System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_PATH, "adventuresample");
            //System.Environment.SetEnvironmentVariable(ServiceExtensions.STATE_TABLE_NAME, "devsession");
            //System.Environment.SetEnvironmentVariable(ServiceExtensions.AZURE_CONTAINER_CONFIG, "UseDevelopmentStorage=true");

            // Local config
            System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_CONTAINER, "skillsconfig");
            System.Environment.SetEnvironmentVariable(ServiceExtensions.CONFIG_PATH, "adventuresample");
            System.Environment.SetEnvironmentVariable(ServiceExtensions.STATE_TABLE_NAME, "devsession");
            System.Environment.SetEnvironmentVariable(ServiceExtensions.AZURE_CONTAINER_CONFIG, "UseDevelopmentStorage=true");

            System.Environment.SetEnvironmentVariable(ServiceExtensions.MEDIA_CONTAINER, mediacontainer);
            System.Environment.SetEnvironmentVariable(ServiceExtensions.USE_LOCAL_STORE, "true");


            var configBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var config = configBuilder.Build();

            //  IServiceProvider serProv 

            IServiceCollection servCol = new ServiceCollection();

            await servCol.AddAdventureSampleServicesAsync(config);

            IServiceProvider prov = servCol.BuildServiceProvider();


            return prov;

        }

        private HttpRequest GenerateIntentRequest(string userId, string intentName)
        {
            HttpRequest req = GenerateRequest(userId, RequestType.IntentRequest, intentName);


            return req;
        }

        private HttpRequest GenerateRequest(string userId, RequestType reqType)
        {
            return GenerateRequest(userId, reqType, null);

        }

        private HttpRequest GenerateRequest(string userId, RequestType reqType, string intentName)
        {

            var headers = new Mock<IHeaderDictionary>();

            headers.SetupGet(r => r["SignatureCertChainUrl"]).Returns("");
            headers.SetupGet(r => r["Signature"]).Returns("");

            headers.Setup(x => x.ContainsKey(It.IsAny<string>()))
            .Returns((string key) =>
            {
                if (key.Equals("SignatureCertChainUrl"))
                    return true;

                if (key.Equals("Signature"))
                    return true;


                return false;
            });


            //headers.Setup(x => x.Any(It.IsAny<Func<KeyValuePair<string, StringValues>, bool>>()))
            //    .Returns(false);



            //headers.Setup(x => x.Any(It.IsAny<Func<KeyValuePair<string, StringValues>, bool>>()))
            //     .Returns((Func<KeyValuePair<string, StringValues>> keyFunc) =>
            //     {

            //         return true;
            //     });


            var httpReqMock = new Mock<HttpRequest>();

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
                },
                Application = new ApplicationAttributes()
                {
                     ApplicationId = TESTAPPGUID

                }
                 
            };

            if(!string.IsNullOrWhiteSpace(intentName))
            {
                req.Request.Intent = new IntentAttributes()
                {
                    Name = intentName,
                    Slots = new List<SlotAttributes>()
                };

            }

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

            string reqText = JsonConvert.SerializeObject(req);


            MemoryStream memString =
                    new MemoryStream(ASCIIEncoding.Default.GetBytes(reqText));


            httpReqMock.SetupGet<String>(x => x.Method).Returns("POST");

            httpReqMock.SetupGet<Stream>(x => x.Body).Returns(memString);

            var headerObj = headers.Object;

            httpReqMock.SetupGet<IHeaderDictionary>(x => x.Headers).Returns(headerObj);
           


            return httpReqMock.Object;
        }

        private ILogger GetLogger()
        {

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
            //.AddFilter(level => level >= LogLevel.Information)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();


            ILogger logger = loggerFactory.CreateLogger<FunctionIntegrationTest>();

            return logger;
        }


        public void WriteResponse(AlexaResponse resp)
        {
            if (resp == null)
            {
                Debug.WriteLine("Null response");
            }
            else
            {
                var cardResp = resp.Response?.Card;

                if (cardResp != null)
                {
                    Debug.WriteLine(string.Format("Card Response: {0}", cardResp.Title));
                    Debug.WriteLine("------");
                    Debug.WriteLine(cardResp.Content);
                    if (cardResp.ImageAttributes != null)
                    {
                        if (!string.IsNullOrWhiteSpace(cardResp.ImageAttributes.LargeImageUrl))
                            Debug.WriteLine(string.Format("Large Image Url: {0}", cardResp.ImageAttributes.LargeImageUrl));

                        if (!string.IsNullOrWhiteSpace(cardResp.ImageAttributes.SmallImageUrl))
                            Debug.WriteLine(string.Format("Small Image Url: {0}", cardResp.ImageAttributes.SmallImageUrl));

                    }
                }


                var outType = resp.Response?.OutputSpeech?.Type;

                if (outType.HasValue)
                {
                    if (outType.Value == OutputSpeechType.PlainText)
                    {

                        Debug.WriteLine("Plain Text Response");
                        Debug.WriteLine(" --------------");
                        Debug.WriteLine(resp.Response.OutputSpeech.Text);
                    }
                    else if (outType.Value == OutputSpeechType.Ssml)
                    {
                        Debug.WriteLine("SSML Response");
                        Debug.WriteLine(" --------------");
                        Debug.WriteLine(resp.Response.OutputSpeech.Ssml);
                    }
                }

                var reprompt = resp.Response?.Reprompt;

                if (reprompt != null)
                {
                    var repromptType = reprompt.OutputSpeech?.Type;

                    if (repromptType.HasValue)
                    {
                        if (repromptType.Value == OutputSpeechType.PlainText)
                        {

                            Debug.WriteLine("Plain Text Reprompt");
                            Debug.WriteLine(" --------------");
                            Debug.WriteLine(resp.Response.Reprompt.OutputSpeech.Text);
                        }
                        else if (repromptType.Value == OutputSpeechType.Ssml)
                        {
                            Debug.WriteLine("SSML Reprompt");
                            Debug.WriteLine(" --------------");
                            Debug.WriteLine(resp.Response.Reprompt.OutputSpeech.Ssml);
                        }


                    }

                }

                bool? shouldEndSession = resp.Response?.ShouldEndSession;


                if (shouldEndSession.HasValue)
                {
                    if (shouldEndSession.Value)
                        Debug.WriteLine("Session is ended");
                    else
                        Debug.WriteLine("Session remains open");

                }
                else
                    Debug.WriteLine("ShouldEndSession is missing");


                Debug.WriteLine("");
            }


        }

        private void ProcessActionResult(IActionResult res)
        {
            ProcessActionResult(res, null);

        }


        private void ProcessActionResult(IActionResult res, string responseContent)
        {
            if (res is OkObjectResult)
            {
                OkObjectResult okRes = (OkObjectResult)res;

                if (okRes.Value != null)
                {
                    if (okRes.Value is AlexaResponse)
                    {
                        AlexaResponse alexaResp = (AlexaResponse)okRes.Value;
                        WriteResponse(alexaResp);

                        if (!string.IsNullOrWhiteSpace(responseContent))
                        {
                            // check if the string is in the response


                        }

                    }
                    else
                        Debug.Fail("IActionResult is OkObjectResult, Value is not an AlexaResponse");
                }
                else
                    Debug.Fail("IActionResult is OkObjectResult, but Value is null");
            }
            else
                Debug.Fail("IActionResult is not OkObjectResult");
        }
    }

}
