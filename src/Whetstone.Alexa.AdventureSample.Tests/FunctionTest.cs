using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Whetstone.Alexa.AdventureSample;
using System.Diagnostics;
using Whetstone.Alexa.AdventureSample.Lambda;
using Moq;
using Whetstone.Alexa.AdventureSample.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Whetstone.Alexa.AdventureSample.Tests
{
    public class FunctionTest
    {


        [Trait("Type", "UnitTest")]
        [Fact]
        public async Task FireLaunchRequestTest()
        {
            AlexaRequest req = GenerateRequest(RequestType.LaunchRequest);
            IServiceProvider serProv = GetTestProvider();
            var function = new Function(serProv);
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
        }



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

            // IAdventureSampleProcessor adventureProcessor = new AdventureSampleProcessor(mockConfig, emailLogger, userManager, progMan, sqsService);

            IAdventureSampleProcessor adventureProcessor = new AdventureSampleProcessor(mockConfig, adventureLogger, advRep, curRep);

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
