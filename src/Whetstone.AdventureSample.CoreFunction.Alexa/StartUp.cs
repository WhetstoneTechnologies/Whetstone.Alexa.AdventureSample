using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;
using Whetstone.AdventureSample.Configuration;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading.Tasks;

namespace Whetstone.AdventureSample.CoreFunction.Alexa
{
    internal class StartUp : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {

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


            //add the key vault to the configuration builder
            //configurationBuilder.AddAzureKeyVault(vaultUrl, vaultClientId, vaultClientSecret);
            //build the config again so it has the key vault provider

            config = configurationBuilder.Build();
            //replace the existing config with the new one
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            //add the ConfigProvider if you want to use IConfiguration in your function
            //the ConfigProvider is just an implementation of IExtensionConfigProvider to give you access to the current IConfiguration
            //builder.AddExtension<ConfigProvider>();

            this.Config = config;
            builder.AddDependencyInjection(ConfigureServices);

        }

        private IConfiguration Config { get; set; }

        private void ConfigureServices(IServiceCollection services)
        {

            var task = Task.Run(async () =>
            {
                await services.AddAdventureSampleServicesAsync(Config);
            });

            task.Wait();

           
        
        }
    }
}
