using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.Alexa.AdventureSample.Models;

namespace Whetstone.Alexa.AdventureSample
{
    public interface IAdventureRepository
    {

        Task<Adventure> GetAdventureAsync();

    }
}
