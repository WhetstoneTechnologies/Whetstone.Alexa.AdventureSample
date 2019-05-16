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
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.Alexa;
using Whetstone.Alexa.Security;

[assembly: FunctionsStartup(typeof(Whetstone.AdventureSample.CoreFunction.Alexa.StartUp))]

namespace Whetstone.AdventureSample.CoreFunction.Alexa
{

    public class AlexaFunction
    {
        private IAlexaRequestVerifier _reqVerifier;

        private IAdventureSampleProcessor _adventureProcessor;

        public AlexaFunction(IAlexaRequestVerifier reqVerifier, IAdventureSampleProcessor adventureProcessor)
        {
            _reqVerifier = reqVerifier ?? throw new ArgumentNullException($"{nameof(reqVerifier)} cannot be null");

            _adventureProcessor = adventureProcessor ??
                                  throw new ArgumentNullException($"{nameof(adventureProcessor)} cannot be null");


        }




        [FunctionName("AlexaFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string textContent = null;

            bool isValid = false;

            try
            {
                isValid = await _reqVerifier.IsCertificateValidAsync(req);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing certificate");

            }

            if (!isValid)
                return new BadRequestResult();
            
            AlexaRequest alexaRequest = null;

            try
            {
                using (StreamReader sr = new StreamReader(req.Body))
                {
                    //This allows you to do one Read operation.
                    textContent = sr.ReadToEnd();
                }

                alexaRequest = JsonConvert.DeserializeObject<AlexaRequest>(textContent);
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Error processing alexa request: {textContent}");

            }


            var alexaResponse = await _adventureProcessor.ProcessAdventureRequestAsync(alexaRequest);
            return (ActionResult)new OkObjectResult(alexaResponse);
            
        }
    }
}
