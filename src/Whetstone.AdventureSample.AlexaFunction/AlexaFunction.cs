using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.Alexa;
using Whetstone.Alexa.Security;

namespace Whetstone.AdventureSample.AlexaFunction
{
    public static class AlexaFunction
    {
        [FunctionName("AlexaAdventureFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string textContent = null;

            IAlexaRequestVerifier reqVerifier = new AlexaCertificateVerifier();
            bool isValid = false;

            try
            {
                isValid = await reqVerifier.IsCertificateValidAsync(req);
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
            catch (Exception ex)
            {
                log.LogError(ex, $"Error processing alexa request: {textContent}");

            }


            IAdventureSampleProcessor adventureProcessor = null;

            var alexaResponse = await adventureProcessor.ProcessAdventureRequestAsync(alexaRequest);
            return (ActionResult)new OkObjectResult(alexaResponse);

        }
    }
}
