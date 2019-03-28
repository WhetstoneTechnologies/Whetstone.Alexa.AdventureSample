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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

using Whetstone.Alexa.AdventureSample;
using System.Diagnostics;
using Moq;
using Whetstone.AdventureSample.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Whetstone.Alexa;

namespace Whetstone.AdventureSample.Tests
{
    public class FunctionTest
    {


        //[Trait("Type", "UnitTest")]
        //[Fact]
        //public async Task FireLaunchRequestTest()
        //{
        //    AlexaRequest req = GenerateRequest(RequestType.LaunchRequest);
        //    IServiceProvider serProv = GetTestProvider();
        //    var function = new AdventureFunction(serProv);
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
        //}



        private AlexaRequest GenerateRequest(RequestType reqType)
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
                SessionId = Guid.NewGuid().ToString()
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




        private IServiceProvider GetTestProvider()
        {

            return GetTestProvider("https://dev-custom.s3.amazonaws.com/emailchecker/image/");
        }


        private IServiceProvider GetTestProvider(string imageRoot)
        {
            AdventureSampleConfig checkerConfig = new AdventureSampleConfig();

            var serviceProvider = new Mock<IServiceProvider>();


            IOptions<AdventureSampleConfig> mockConfig = Options.Create(new AdventureSampleConfig());


            serviceProvider
                .Setup(x => x.GetService(typeof(IOptions<AdventureSampleConfig>)))
                .Returns(mockConfig);

            ILogger<AdventureSampleProcessor> adventureLogger = Mock.Of<ILogger<AdventureSampleProcessor>>();

            serviceProvider
                .Setup(x => x.GetService(typeof(ILogger<AdventureSampleProcessor>)))
                .Returns(adventureLogger);

            
            //ISqsService sqsService = Mock.Of<ISqsService>();
            //serviceProvider
            //    .Setup(x => x.GetService(typeof(ISqsService)))
            //    .Returns(sqsService);


            IAdventureRepository advRep = Mock.Of<IAdventureRepository>();

            ICurrentNodeRepository curRep = Mock.Of<ICurrentNodeRepository>();

            IMediaLinkProcessor mediaLinker = Mock.Of<IMediaLinkProcessor>();

            // IAdventureSampleProcessor adventureProcessor = new AdventureSampleProcessor(mockConfig, emailLogger, userManager, progMan, sqsService);

            IAdventureSampleProcessor adventureProcessor = new AdventureSampleProcessor(adventureLogger, advRep, curRep, mediaLinker);

            serviceProvider
                .Setup(x => x.GetService(typeof(IAdventureSampleProcessor)))
                .Returns(adventureProcessor);

            return serviceProvider.Object;

            //public EmailProcessor(IOptions<EmailCheckerConfig> emailCheckerConfig,
            //                        ILogger<EmailProcessor> logger,
            //                        IAlexaUserDataManager userDataManager,
            //                        IProgressiveResponseManager progMan,
            //                        ISqsService sqsService
            //                      )

        }

    }

}
