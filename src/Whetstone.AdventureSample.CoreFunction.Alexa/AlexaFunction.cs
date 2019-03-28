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
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.Alexa;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Alexa;
using Microsoft.Azure.WebJobs.Hosting;
using Whetstone.AdventureSample.CoreFunction.Alexa;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(StartUp))]

namespace Whetstone.AdventureSample.CoreFunction.Alexa
{

    public static class AlexaFunction
    {

        [FunctionName("AlexaFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [Inject] IAlexaRequestVerifier alexaVerifier,
            [Inject] IAdventureSampleProcessor adventureProcessor)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string textContent = null;

            //IServiceProvider servProv = _serviceProvider.Value;

            //IAlexaRequestVerifier alexaVerifier = servProv.GetRequiredService<IAlexaRequestVerifier>();

            bool isValid = false;

            try
            {
                isValid = await alexaVerifier.IsCertificateValidAsync(req);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Error processing certificate");

            }

            if (!isValid)
                return new BadRequestResult();


            if (req.Body != null)
            {
                using (StreamReader sr = new StreamReader(req.Body))
                {
                    //This allows you to do one Read operation.
                    textContent = sr.ReadToEnd();
                }
            }

            AlexaRequest alexaRequest = null;

            try
            {
                alexaRequest = JsonConvert.DeserializeObject<AlexaRequest>(textContent);
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Error processing alexa request: {textContent}");

            }
            
            if (alexaVerifier.IsRequestValid(alexaRequest))
            {
                var alexaResponse = await adventureProcessor.ProcessAdventureRequestAsync(alexaRequest);
                return (ActionResult)new OkObjectResult(alexaResponse);
            }
            else
            {
                log.LogError($"Alexa request is not valid: {textContent}");

            }


            return new BadRequestResult();
        }
    }
}
