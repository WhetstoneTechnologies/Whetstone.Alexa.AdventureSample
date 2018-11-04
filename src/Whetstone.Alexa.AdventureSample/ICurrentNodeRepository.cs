using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public interface ICurrentNodeRepository
    {
        Task<AdventureNode> GetCurrentNodeAsync(AlexaRequest req, IEnumerable<AdventureNode> nodes);

        Task<string> GetCurrentNodeNameAsync(AlexaRequest req);

        Task SetCurrentNodeAsync(AlexaRequest req, AlexaResponse resp, string currentNodeName);


    }
}
