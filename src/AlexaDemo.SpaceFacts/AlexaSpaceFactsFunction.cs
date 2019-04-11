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
using System.Collections.Generic;
using System.Threading;
using Whetstone.Alexa.Security;

namespace AlexaDemo.SpaceFacts
{
    public static class AlexaSpaceFactsFunction
    {
        private const string GET_FACT_MESSAGE = "Here's your fact: ";
        private const string HELP_MESSAGE = "You can say tell me a space fact, or, you can say exit... What can I help you with?";
        private const string HELP_REPROMPT = "What can I help you with?";
        private const string STOP_MESSAGE = "Goodbye!";


        private static readonly ThreadLocal<Random> threadLocalRandom
            = new ThreadLocal<Random>(() => new Random());

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

        internal enum SpaceRequestType
        {
            GetFact,
            Help,
            Stop
        }

        [FunctionName("AlexaSpaceFactsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


#if !DEBUG
            //https://developer.amazon.com/docs/custom-skills/host-a-custom-skill-as-a-web-service.html
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

#endif

            string textContent = null;
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

            if (alexaRequest == null)
                return new BadRequestResult();

            SpaceRequestType spaceReqType = GetSpaceRequestType(alexaRequest.Request);


            AlexaResponse alexaResp = null;
            switch(spaceReqType)
            {
                case SpaceRequestType.GetFact:
                    string spaceFact = GetRandomFact();
                    string factText = string.Concat(Whetstone.Alexa.Audio.AmazonSoundLibrary.SciFi.SCIFI_ZAP_ELECTRIC_01 , GET_FACT_MESSAGE, spaceFact);
                    alexaResp = GetAlexaResponse(factText);
                    break;
                case SpaceRequestType.Help:
                    alexaResp = GetAlexaResponse(HELP_MESSAGE, HELP_REPROMPT);
                    break;
                case SpaceRequestType.Stop:
                    alexaResp = GetAlexaResponse(STOP_MESSAGE);
                    break;
            }

            return new OkObjectResult(alexaResp);
        }


        private static AlexaResponse GetAlexaResponse(string responseText)
        {
            return GetAlexaResponse(responseText, null);
        }

        private static AlexaResponse GetAlexaResponse(string responseText, string repromptText)
        {
            AlexaResponse alexaResp = new AlexaResponse();


            alexaResp.Response = new AlexaResponseAttributes();

            alexaResp.Response.OutputSpeech = new OutputSpeechAttributes()
            {
                Type = OutputSpeechType.PlainText,
                Text = responseText

            };

            alexaResp.Response.Card = new CardAttributes()
            {
                Title = "Space Fact",
                Type = CardType.Simple,
                Content = responseText

            };
    
            if (!string.IsNullOrWhiteSpace(repromptText))
            {
                alexaResp.Response.Reprompt = new RepromptAttributes();
                alexaResp.Response.Reprompt.OutputSpeech = new OutputSpeechAttributes()
                {
                    Type = OutputSpeechType.PlainText,
                    Text = repromptText
                };
                alexaResp.Response.ShouldEndSession = false;
            }
            else
            {
                alexaResp.Response.ShouldEndSession = true;
            }
     
            return alexaResp;
        }

        private static SpaceRequestType GetSpaceRequestType(RequestAttributes reqAttributes)
        {
            SpaceRequestType spaceReqType = SpaceRequestType.Help;

            switch (reqAttributes.Type)
            {
                case RequestType.LaunchRequest:
                    spaceReqType = SpaceRequestType.GetFact;
                    break;
                case RequestType.IntentRequest:

                    string intent = reqAttributes.Intent.Name;

                    if(intent.Equals("GetNewFactIntent", StringComparison.OrdinalIgnoreCase))
                    {
                        spaceReqType = SpaceRequestType.GetFact;
                    }
                    else if (intent.Equals("AMAZON.HelpIntent", StringComparison.OrdinalIgnoreCase))
                    {
                        spaceReqType = SpaceRequestType.Help;
                    }
                    else if (intent.Equals("AMAZON.Cancel", StringComparison.OrdinalIgnoreCase) ||
                            intent.Equals("AMAZON.Stop", StringComparison.OrdinalIgnoreCase))
                    {
                        spaceReqType = SpaceRequestType.Stop;
                    }
                break;
            }

            return spaceReqType;
        }


        private static AlexaResponse GetResponse()
        {
            AlexaResponse resp = new AlexaResponse();


            resp.Response = new AlexaResponseAttributes();
            resp.Response.OutputSpeech = new OutputSpeechAttributes();
            resp.Response.OutputSpeech.Type = OutputSpeechType.PlainText;

            resp.Response.OutputSpeech.Text = "";

            return resp;

        }

    }
}
