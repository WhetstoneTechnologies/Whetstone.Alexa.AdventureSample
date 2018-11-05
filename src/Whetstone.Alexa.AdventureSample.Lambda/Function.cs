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
using System.Linq;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Amazon.Lambda.Core;
using Whetstone.Alexa.AdventureSample.Configuration;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.Alexa.AdventureSample.Lambda
{
    public class Function
    {


        private static readonly Lazy<IServiceProvider> _serviceProvider = new Lazy<IServiceProvider>(() =>
        {

            var builder = new ConfigurationBuilder()
              .AddEnvironmentVariables();

            IConfiguration config = builder.Build();

            IServiceCollection servCol = new ServiceCollection();


            servCol.AddAdventureSampleServices(config);

            return servCol.BuildServiceProvider();

        });

        public Function()
        {

        }

#if DEBUG
        private IServiceProvider _testProvider = null;

        public Function(IServiceProvider testProvider)
        {
            _testProvider = testProvider;
        }


        private IServiceProvider GetServiceProvider()
        {
            if (_testProvider != null)
                return _testProvider;


            return _serviceProvider.Value;
        }
#else

        private IServiceProvider GetServiceProvider()
        {
            return _serviceProvider.Value;
        }

#endif


        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<AlexaResponse> FunctionHandlerAsync(AlexaRequest req, ILambdaContext context)
        {
            IServiceProvider curProvider = GetServiceProvider();

            var adventureProcessor = curProvider.GetRequiredService<IAdventureSampleProcessor>();
            return await adventureProcessor.ProcessAdventureRequestAsync(req);
        }



    }
}
