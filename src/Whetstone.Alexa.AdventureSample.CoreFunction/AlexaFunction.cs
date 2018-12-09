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
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.Alexa.AdventureSample.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;


namespace Whetstone.Alexa.AdventureSample.CoreFunction
{

    public static class AlexaFunction
    {

        private static readonly Lazy<IServiceProvider> _serviceProvider = new Lazy<IServiceProvider>(() =>
        {

            var builder = new ConfigurationBuilder()
              .AddEnvironmentVariables();

            IConfiguration config = builder.Build();

            IServiceCollection servCol = new ServiceCollection();


            servCol.AddAdventureSampleServices(config, HostTypeEnum.Azure);

            return servCol.BuildServiceProvider();

        });

        [FunctionName("AlexaFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string textContent = null;
            if (req.Body != null)
            {
                using (StreamReader sr = new StreamReader(req.Body))
                {
                    //This allows you to do one Read operation.
                    textContent = sr.ReadToEnd();
                }
            }


            AlexaRequest alexaRequest = JsonConvert.DeserializeObject<AlexaRequest>(textContent);

            var servProv = _serviceProvider.Value;

           var alexaProcessor = servProv.GetService<IAdventureSampleProcessor>();

            var alexaResponse = await alexaProcessor.ProcessAdventureRequestAsync(alexaRequest);


            return (ActionResult) new OkObjectResult(alexaResponse);
        }
    }
}
