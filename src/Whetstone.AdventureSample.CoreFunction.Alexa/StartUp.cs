using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;
using Whetstone.AdventureSample.Configuration;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Whetstone.AdventureSample.CoreFunction.Alexa
{
    internal class StartUp : FunctionsStartup
    {

        internal const string LOG_LEVEL_CONFIG = "LogLevel";

        public override void Configure(IFunctionsHostBuilder builder)
        {

         //   builder.Services.AddHttpClient();

            //Get the current config and merge it into a new ConfigurationBuilder to keep the old settings
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables();
            

            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            //build the config in order to access the appsettings for getting the key vault connection settings
            var config = configurationBuilder.Build();


            this.Config = config;

            ConfigureServices(builder.Services);

        }

        private IConfiguration Config { get; set; }

        private void ConfigureServices(IServiceCollection services)
        {

            var task = Task.Run(async () =>
            {
                await services.AddAdventureSampleServicesAsync(Config);
            });

            task.Wait();

            services.AddLogging(logBuilder =>
            {
                LogLevel logLevelVal = GetLogLevel(Config);
                logBuilder.SetMinimumLevel(logLevelVal);
                logBuilder.AddConsole();

            });

      }


        private LogLevel GetLogLevel(IConfiguration config)
        {
            string logLevel = config.GetValue<string>("Logging:LogLevel:Default");

            LogLevel logLevelVal = LogLevel.Debug;

            if (string.IsNullOrWhiteSpace(logLevel))
                logLevel = config.GetValue<string>(LOG_LEVEL_CONFIG);

            if (!string.IsNullOrWhiteSpace(logLevel))
                logLevelVal = (LogLevel)Enum.Parse(typeof(LogLevel), logLevel);


            return logLevelVal;
        }
    }
}
