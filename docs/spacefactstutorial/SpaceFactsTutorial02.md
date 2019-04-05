# SpaceFacts -- Module 2

## Introduction

The original Alexa Space Facts tutorial uses the Amazon Web Services and a Lambda. This tutorial uses C#, Visual Studio, and and Azure Function.

## Part One - Create the Azure Function

1. Create a new project in Visual Studio. Select Cloud under Visual C# in the Add New Project dialog. Select Azure Function. Name the project AlexaDemo.SpaceFacts or something similar.

  <img src="/docs/images/AzureFuncCreate01.png?raw=true" width="50%"/>
  
2. Select Http trigger and select OK.

  <img src="/docs/images/AzureFuncCreate02.png?raw=true" width="50%"/>
  
  This creates a new Azure Http Function:

  ``` C#
  namespace AlexaDemo.SpaceFacts
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
```

3. Add references to the following NuGet packages:

   Newtonsoft.Json
   Whetstone.Alexa

4. Rename the class and the file, including the FunctionName attribute, to AlexaSpaceFactsFunction.

``` C#
        [FunctionName("AlexaSpaceFactsFunction")]
        public static async Task<IActionResult> Run(
```

5. Add the following code to the AlexaSpaceFactsFunction class. This is used to retrieve a random fact. The generation of the random function 

``` C#
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
```

7. Add the following string constants to the class.

``` C#
        private const string GET_FACT_MESSAGE = "Here's your fact: ";
        private const string HELP_MESSAGE = "You can say tell me a space fact, or, you can say exit... What can I help you with?";
        private const string HELP_REPROMPT = "What can I help you with?";
        private const string STOP_MESSAGE = "Goodbye!";
```

6. Update the function body to include logic to derialize the body of the request to an AlexaRequest instance:

``` C#
        [FunctionName("AlexaSpaceFactsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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


            return null;
        }
```

7. Add the SpaceRequestType enum to the AlexaSpaceFactsFunction class.

``` C#
        internal enum SpaceRequestType
        {
            GetFact,
            Help,
            Stop
        }
```

8. Add the GetSpaceRequestType function to the AlexaSpaceFunction class. This is used to determine what the user requests and drive the response.

``` C#
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

```
