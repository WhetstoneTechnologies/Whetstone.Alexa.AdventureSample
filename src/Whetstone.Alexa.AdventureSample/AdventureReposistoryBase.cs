// Copyright (c) 2018 Whetstone Technologies. All rights reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using Whetstone.Alexa.AdventureSample.Configuration;
using Whetstone.Alexa.AdventureSample.Models;
using YamlDotNet.Serialization;

namespace Whetstone.Alexa.AdventureSample
{
    public abstract class AdventureReposistoryBase
    {
        protected AdventureSampleConfig _adventureConfig;

        protected string GetConfigPath()
        {
            string configPath = _adventureConfig.MediaPath;

            if (string.IsNullOrWhiteSpace(configPath))
            {
                configPath = "adventure.yaml";
            }
            else
            {
                if (configPath[configPath.Length - 1] != '/')
                    configPath = string.Concat(configPath, '/');

                configPath = string.Concat(configPath, "adventure.yaml");
            }

            return configPath;


        }


        protected Adventure DeserializeAdventure(string adventureText)
        {
            Deserializer deser = new Deserializer();


            Adventure adv = deser.Deserialize<Adventure>(adventureText);

            return adv;
        }

    }
}
