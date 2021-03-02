using AdaskoTheBeAsT.WkHtmlToX;
using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Engine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using PdfConvert;
using System;
using System.IO;

namespace PdfConvertBenchmark
{
    public class StartupFixture
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public StartupFixture()
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

            var services = new ServiceCollection();
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

            ServiceProvider = services.BuildServiceProvider();
        }
    }

    public class MockWebHostEnvironment : IWebHostEnvironment
    {
        public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string ContentRootPath
        {
            get
            {
                string path = Directory.GetCurrentDirectory();

                return Path.Combine(path,"");

            }
            set => throw new NotImplementedException();
        }
        public string EnvironmentName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
