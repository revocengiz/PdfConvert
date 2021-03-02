using AdaskoTheBeAsT.WkHtmlToX;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PdfConvert.MaxConcurrentRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfConvert
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<MaxConcurrentRequestsOptions>(Configuration.GetSection(nameof(MaxConcurrentRequestsOptions)));
            services.AddControllers();
            services.AddSingleton(sp =>
            {
                return new WkHtmlToXConfiguration((int)Environment.OSVersion.Platform, null); ;
            });
            services.AddSingleton<IWkHtmlToXEngine, WkHtmlToXEngine>(sp =>
            {
                var config = sp.GetRequiredService<WkHtmlToXConfiguration>();
                var engine = new WkHtmlToXEngine(config);
                engine.Initialize();
                return engine;
            });
            services.AddSingleton<IPdfConverter, PdfConverter>();
            services.AddSingleton<PdfService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseMaxConcurrentRequests();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
