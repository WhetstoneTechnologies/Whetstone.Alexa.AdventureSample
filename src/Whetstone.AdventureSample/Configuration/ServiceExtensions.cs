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
using Microsoft.Extensions.DependencyInjection;
using System;
using Whetstone.Alexa.ProgressiveResponse;
using Whetstone.Alexa.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Whetstone.Ngrok.ApiClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Whetstone.Ngrok.ApiClient.Models;
using System.Linq;

namespace Whetstone.AdventureSample.Configuration
{

    public static class ServiceExtensions
    {
        public static readonly string CONFIG_CONTAINER = "ConfigContainerName";
        public static readonly string CONFIG_PATH = "ConfigPath";
        public static readonly string MEDIA_CONTAINER = "MediaContainerName";
        public static readonly string MEDIA_PATH = "MediaPath";
        public static readonly string MEDIA_CONTAINER_ACCOUNT = "MediaContainerAccountName";


        internal const string REDISSERVER_CONFIG = "RedisServer";
        internal const string REDISSERVERINTANCE_CONFIG = "RedisServerInstance";
        public static readonly string STATE_TABLE_NAME = "SessionStateTable";
        public const string AZURE_CONTAINER_CONFIG = "ConfigConnectionString";

        public const string USE_LOCAL_STORE = "UseLocalStore";

        public static async Task AddAdventureSampleServicesAsync(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions();
            string redisServer = config.GetValue<string>(REDISSERVER_CONFIG);

            string azureCon = config.GetValue<string>(AZURE_CONTAINER_CONFIG);


            string configContainerName = config.GetValue<string>(CONFIG_CONTAINER);
            string configPath = config.GetValue<string>(CONFIG_PATH);


            string mediaContainerName = config.GetValue<string>(MEDIA_CONTAINER);
            string mediaRootPath = config.GetValue<string>(MEDIA_PATH);


            bool? useLocalStore = config.GetValue<bool?>(USE_LOCAL_STORE);
            string localStoreServer = null;

            if (useLocalStore.GetValueOrDefault(false))
                localStoreServer = await GetLocalMediaUrlAsync();


            services.Configure<AdventureSampleConfig>(
            options =>
            {
                options.ConfigContainerName = configContainerName;
                options.ConfigPath = configPath;

                options.MediaContainerName = string.IsNullOrWhiteSpace(mediaContainerName) ? configContainerName : mediaContainerName;
                options.MediaPath = string.IsNullOrWhiteSpace(mediaRootPath) ? configPath : mediaRootPath;
                options.MediaContainerAccountName = config.GetValue<string>(MEDIA_CONTAINER_ACCOUNT);

                options.AzureConfigConnectionString = config.GetValue<string>(AZURE_CONTAINER_CONFIG);

                options.UserStateTableName = config.GetValue<string>(STATE_TABLE_NAME);
                options.LocalStoreServer = localStoreServer;
            });



            services.AddTransient<IDistributedCache, MemoryDistributedCache>();
            services.AddTransient<IAlexaRequestVerifier, AlexaCertificateVerifier>();

            services.AddTransient<IAdventureRepository, BlobAdventureRepository>();
            services.AddTransient<ICurrentNodeRepository, TableCurrentNodeRepository>();
            services.AddTransient<IMediaLinkProcessor, BlobMediaLinkProcessor>();

            services.AddTransient<IProgressiveResponseManager, ProgressiveResponseManager>();
            services.AddTransient<IAlexaUserDataManager, AlexaUserDataManager>();
            services.AddTransient<IAdventureSampleProcessor, AdventureSampleProcessor>();
        }








        private static async Task<string> GetLocalMediaUrlAsync()
        {
          //  ILogger logger = GetLogger();

            NgrokClient clientCurClient = new NgrokClient();
            string mediaTunnelUrl = null;
            List<Tunnel> tunnels = null;

            try
            {
                tunnels = await clientCurClient.GetTunnelListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Must launch ngrok for this integration test. See the startngrok.bat file in Whetstone.AdventureSample.Integ.Tests");
            }


            if (tunnels == null)
            {
                throw new Exception("Must launch ngrok for this integration test. See the startngrok.bat file in Whetstone.AdventureSample.Integ.Tests");
            }
            else
            {
                // This tunnel was started by manually running the startngrok.bat
                string mediaTunnelName = "mediatunnel";
                Tunnel mediaTunnel = tunnels.FirstOrDefault(x => x.Name.Equals(mediaTunnelName, StringComparison.OrdinalIgnoreCase));
                if (mediaTunnel == null)
                    throw new Exception($"ngrok started, but expected {mediaTunnelName} not found");

                mediaTunnelUrl = mediaTunnel.PublicUrl;

            }

            return mediaTunnelUrl;
        }


        //private static ILogger GetLogger()
        //{

        //    IServiceCollection serviceCollection = new ServiceCollection();
        //    serviceCollection.AddLogging(builder => builder
        //        .AddConsole()
        //        .AddDebug()
        //    //.AddFilter(level => level >= LogLevel.Information)
        //    );
        //    var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();


        //    ILogger logger = loggerFactory.CreateLogger("Whetstone.AdventureSample.Configuration.ServiceExtensions");

        //    return logger;
        //}



    }
}
