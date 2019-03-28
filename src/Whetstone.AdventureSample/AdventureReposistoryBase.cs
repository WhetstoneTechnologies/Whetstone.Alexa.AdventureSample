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
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whetstone.AdventureSample.Configuration;
using Whetstone.AdventureSample.Models;
using YamlDotNet.Serialization;

namespace Whetstone.AdventureSample
{



    public abstract class AdventureReposistoryBase
    {
        private const string ADVENTURE_CACHE_KEY = "adventureconfig";
        private const string ADVENTURE_FILE_NAME = "adventure.yaml";

        protected readonly AdventureSampleConfig _adventureConfig;
        protected readonly IDistributedCache _cache;
        protected readonly string _configContainerName;
       
        public AdventureReposistoryBase(IOptions<AdventureSampleConfig> config, IDistributedCache cache)
        {

            if (config == null)
                throw new ArgumentException("config cannot be null");

            if (config.Value == null)
                throw new ArgumentException("config.Value cannot be null");

            _adventureConfig = config.Value;


            if (cache == null)
                throw new ArgumentNullException("cache cannot be null");

            _cache = cache;


            if (string.IsNullOrWhiteSpace(_adventureConfig.ConfigContainerName))
                throw new Exception("ConfigContainerName missing from configuration");
            _configContainerName = _adventureConfig.ConfigContainerName;
            
        }

        protected async Task<Adventure> GetCachedAdventureAsync()
        {
            Adventure retAdventure = null;
           string adventureText = await _cache.GetStringAsync(ADVENTURE_CACHE_KEY);

            if (!string.IsNullOrWhiteSpace(adventureText))
            {
                Deserializer yamlDeser = new Deserializer();
                retAdventure = yamlDeser.Deserialize<Adventure>(adventureText);
            }

            return retAdventure;
        }


        protected async Task SetCachedAdventureAsync(Adventure adv)
        {
            Serializer yamlSer = new Serializer();

            string adventureText = yamlSer.Serialize(adv);

            DistributedCacheEntryOptions cachOpts = new DistributedCacheEntryOptions();
            cachOpts.SlidingExpiration = new TimeSpan(1, 0, 0);

            await _cache.SetStringAsync(ADVENTURE_CACHE_KEY, adventureText, cachOpts);

        }

        protected string GetConfigPath()
        {
            string configPath = _adventureConfig.ConfigPath;

            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = ADVENTURE_FILE_NAME;
            }
            else
            {
                if (configPath[configPath.Length - 1] != '/')
                    configPath = string.Concat(configPath, '/');

                configPath = string.Concat(configPath, ADVENTURE_FILE_NAME);
            }

            return configPath;


        }


        protected Adventure DeserializeAdventure(string adventureText)
        {
            Deserializer deser = new Deserializer();
            Adventure adv = null;

             adv = deser.Deserialize<Adventure>(adventureText);
            

            return adv;
        }

    }
}
