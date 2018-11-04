using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Whetstone.Alexa.AdventureSample.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlexaController : ControllerBase
    {

        //To test with ngrok drop to the command line and execute:

        //ngrok http 54768 -host-header="localhost:54768" -subdomain=whetstone


        //If the Web API project launches in debug mode to another port, substitute that port for 52280.

        //If ngrok is not installed on the local machine, download it and create a new account on ngrok.com if necessary.

        //https://ngrok.com/

        //netsh http add urlacl url=https://whetstone.ngrok.io:54768/ user=everyone


        // private IConfiguration _config;

        private const int MAXREQUESTSENDTIME = 150;

        private IAdventureSampleProcessor _adventureProcessor;
        private ILogger<AlexaController> _logger;

        public AlexaController(IAdventureSampleProcessor adventureProcessor, ILogger<AlexaController> logger)
        {
            _adventureProcessor = adventureProcessor;
            _logger = logger;
        }




        [HttpPost]
        public async Task<ActionResult> ProcessAlexaRequest([FromBody] AlexaRequest request)
        {
            // One of Alexa's policy requirements is that the request cannot take more then 150 seconds to get from the Echo to the skill processing logic.
            // This check enforces that policy
            var diff = DateTime.UtcNow - request.Request.Timestamp;
#if DEBUG
            // For the diff to be one second to satisfy conditions
            diff = new TimeSpan(0, 0, 0, 1);
#endif


            if (Math.Abs((decimal)diff.TotalSeconds) <= MAXREQUESTSENDTIME)
            {
                _logger.LogDebug("Alexa request was timestamped {0:0.00} seconds ago below the {1} second maximum.", diff.TotalSeconds, MAXREQUESTSENDTIME);
                AlexaResponse resp = await _adventureProcessor.ProcessAdventureRequestAsync(request);



                return Ok(resp);
            }
            else
            {
                _logger.LogError("Alexa request was timestamped {0:0.00} seconds ago. It exceeds the {1} second maximum", diff.TotalSeconds, MAXREQUESTSENDTIME);

                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}