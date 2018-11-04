using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.Alexa.AdventureSample.WebApi.Security;
using Whetstone.Alexa.AdventureSample.Configuration;

namespace Whetstone.Alexa.AdventureSample.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddAdventureSampleServices(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }



            //  app.Map("/api/alexa", alexaApp => { alexaApp.UseAlexaValidation(); });
            // If not in development, then apply strict certificate validation
            // checking.
            app.UseWhen(context => context.Request.Path.ToString().EndsWith("alexa"),
                branch =>
                {
                    branch.UseAlexaValidation();
                });


            app.UseMvc();
        }
    }
}
