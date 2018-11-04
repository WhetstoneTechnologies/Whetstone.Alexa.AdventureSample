using System.Threading.Tasks;

namespace Whetstone.Alexa.AdventureSample
{
    public interface IAdventureSampleProcessor
    {

        Task<AlexaResponse> ProcessAdventureRequestAsync(AlexaRequest req);
    }
}