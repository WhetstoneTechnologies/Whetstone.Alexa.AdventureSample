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
using Microsoft.Extensions.DependencyInjection;
using System;
using Whetstone.Alexa.ProgressiveResponse;
using Whetstone.Alexa.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Whetstone.Alexa.AdventureSample.Configuration
{

    public enum HostTypeEnum
    {
        Aws = 1,
        Azure =2
    }

    public static class ServiceExtensions
    {
        public static readonly string CONFIG_CONTAINER = "ConfigContainerName";
        public static readonly string CONFIG_PATH = "ConfigPath";
        public static readonly string MEDIA_CONTAINER = "MediaContainerName";
        public static readonly string MEDIA_PATH = "MediaPath";
        public static readonly string MEDIA_CONTAINER_ACCOUNT = "MediaContainerAccountName";

        internal const string LOG_LEVEL_CONFIG = "LogLevel";
        internal const string REDISSERVER_CONFIG = "RedisServer";
        internal const string REDISSERVERINTANCE_CONFIG = "RedisServerInstance";
        internal const string AWSREGION_CONFIG = "AwsRegion";
        public static readonly string STATE_TABLE_NAME = "SessionStateTable";
        internal const string AZURE_CONTAINER_CONFIG = "ConfigConnectionString";

        public static void AddAdventureSampleServices(this IServiceCollection services, IConfiguration config, HostTypeEnum hostType)
        {
            services.AddOptions();


            string awsRegion = config.GetValue<string>(AWSREGION_CONFIG);


            string redisServer = config.GetValue<string>(REDISSERVER_CONFIG);

            string azureCon = config.GetValue<string>(AZURE_CONTAINER_CONFIG);


            string configContainerName = config.GetValue<string>(CONFIG_CONTAINER);
            string configPath = config.GetValue<string>(CONFIG_PATH);


            string mediaContainerName = config.GetValue<string>(MEDIA_CONTAINER);
            string mediaRootPath = config.GetValue<string>(MEDIA_PATH);

            services.Configure<AdventureSampleConfig>(
            options =>
            {
                options.ConfigContainerName = configContainerName;
                options.ConfigPath = configPath;

                options.MediaContainerName = string.IsNullOrWhiteSpace(mediaContainerName) ? configContainerName : mediaContainerName;
                options.MediaPath = string.IsNullOrWhiteSpace(mediaRootPath) ? configPath : mediaRootPath;
                options.MediaContainerAccountName = config.GetValue<string>(MEDIA_CONTAINER_ACCOUNT);
                
                options.AzureConfigConnectionString = config.GetValue<string>(AZURE_CONTAINER_CONFIG);
          
                options.AwsRegion = string.IsNullOrWhiteSpace(awsRegion) ? "us-east-1" : awsRegion;
                options.UserStateTableName = config.GetValue<string>(STATE_TABLE_NAME);
            });

            services.AddLogging(logging =>
            {
                LogLevel logLevelVal = GetLogLevel(config);
                logging.SetMinimumLevel(logLevelVal);
                logging.ClearProviders();
                logging.AddConsole(x =>
                {
                    x.IncludeScopes = false;
                });

#if DEBUG
                logging.AddDebug();
#endif               
            });

            switch(hostType)
            {
                case HostTypeEnum.Aws:
                    services.AddTransient<IAdventureRepository, S3AdventureRepository>();
                    services.AddTransient<ICurrentNodeRepository, DynamoDbCurrentNodeRepository>();
                    services.AddTransient<IMediaLinkProcessor, S3MediaLinkProcessor>();
                    break;
                case HostTypeEnum.Azure:
                    services.AddTransient<IAdventureRepository, BlobAdventureRepository>();
                    services.AddTransient<ICurrentNodeRepository, TableCurrentNodeRepository>();
                    services.AddTransient<IMediaLinkProcessor, BlobMediaLinkProcessor>();
                    break;
            }

            services.AddTransient<IProgressiveResponseManager, ProgressiveResponseManager>();
            services.AddTransient<IAlexaUserDataManager, AlexaUserDataManager>();
            services.AddTransient<IAdventureSampleProcessor, AdventureSampleProcessor>();
        }

        private static LogLevel GetLogLevel(IConfiguration config)
        {
            string logLevel = config.GetValue<string>("Logging:LogLevel:Default");

            LogLevel logLevelVal = LogLevel.Warning;

            if (string.IsNullOrWhiteSpace(logLevel))
                logLevel = config.GetValue<string>(LOG_LEVEL_CONFIG);

            if (!string.IsNullOrWhiteSpace(logLevel))
                logLevelVal = (LogLevel)Enum.Parse(typeof(LogLevel), logLevel);


            return logLevelVal;
        }



    }
}
