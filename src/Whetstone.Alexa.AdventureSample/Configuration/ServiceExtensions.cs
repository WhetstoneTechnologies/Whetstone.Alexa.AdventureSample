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
using System.Collections.Generic;
using System.Text;
using Whetstone.Alexa.ProgressiveResponse;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Whetstone.Alexa.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
//using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Caching.Memory;
using Whetstone.Alexa.AdventureSample;

namespace Whetstone.Alexa.AdventureSample.Configuration
{
    public static class ServiceExtensions
    {
        public static readonly string CONFIG_PATH= "AdventurePath";
        public static readonly string BUCKET_CONFIG = "BucketName";
        internal const string LOG_LEVEL_CONFIG = "LogLevel";
        internal const string REDISSERVER_CONFIG = "RedisServer";
        internal const string REDISSERVERINTANCE_CONFIG = "RedisServerInstance";
        internal const string AWSREGION_CONFIG = "AwsRegion";
        public const string DYNAMODB_CONFIG = "DynamoDbTable";

        public static void AddAdventureSampleServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions();


            string awsRegion = config.GetValue<string>(AWSREGION_CONFIG);

            //services.Configure<SqsConfiguration>(
            //    options =>
            //    {
            //        options.QueueName = config.GetValue<string>(SQSQUEUENAME_CONFIG);
            //        options.QueueUrl = config.GetValue<string>(SQSQUEUEURL_CONFIG);
            //        options.AwsRegion = string.IsNullOrWhiteSpace(awsRegion) ? "us-east-1" : awsRegion;

            //    });

            string redisServer = config.GetValue<string>(REDISSERVER_CONFIG);


            //if (string.IsNullOrWhiteSpace(redisServer))
            //{
            //    // use in memory distributed cache
            //    services.AddDistributedMemoryCache();
            //}
            //else
            //{
            //    string instanceNameConfig = config.GetValue<string>(REDISSERVERINTANCE_CONFIG);
            //    string instanceName = string.IsNullOrWhiteSpace(instanceNameConfig) ?
            //                                "emailchecker" :
            //                                instanceNameConfig;

            //    services.AddDistributedRedisCache(opts =>
            //    {
            //        opts.Configuration = redisServer;
            //        opts.InstanceName = instanceNameConfig;
            //    });
            //}

           string dynamoDb = config.GetValue<string>(DYNAMODB_CONFIG);

            services.Configure<AdventureSampleConfig>(
            options =>
            {
                options.ConfigBucket = config.GetValue<string>(BUCKET_CONFIG);
                options.ConfigPath = config.GetValue<string>(CONFIG_PATH);
                options.AwsRegion = string.IsNullOrWhiteSpace(awsRegion) ? "us-east-1" : awsRegion;
                options.DynamoDb = dynamoDb;
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


            services.AddTransient<IAdventureSampleProcessor, AdventureSampleProcessor>();
            services.AddTransient<IProgressiveResponseManager, ProgressiveResponseManager>();
            services.AddTransient<IAlexaUserDataManager, AlexaUserDataManager>();
            services.AddTransient<IAdventureRepository, S3AdventureRepository>();


            // services.AddTransient<ICurrentNodeRepository, SessionNodeRepository>();

            services.AddTransient<ICurrentNodeRepository, DynamoDbCurrentNodeRepository>();
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
