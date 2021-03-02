using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfConvert;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PdfConvertBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
             string path = Directory.GetCurrentDirectory();
            BenchmarkRunner.Run<BenchmarkHtmlToPdf>();
        }
    }

    [SimpleJob(launchCount: 2, warmupCount: 2, targetCount: 10)]
    public class BenchmarkHtmlToPdf
    {
        readonly byte[] html;
        readonly StartupFixture fixture;
        readonly PdfService pdfService;
        public BenchmarkHtmlToPdf()
        {
            html = File.ReadAllBytes("html.html");
            fixture = new StartupFixture();
            pdfService = fixture.ServiceProvider.GetRequiredService<PdfService>();
        }
        [Benchmark(Baseline = true)]
        public async Task SingleProcessWithRateLimiting()
        {
            using (var str = await pdfService.WithProcess(html, CancellationToken.None))
            {

            }
        }

        [Benchmark]
        public async Task WithAdaskoTheBeAsT()
        {
            using (var str = await pdfService.WithAdaskoTheBeAsT(html, CancellationToken.None))
            {

            }
        }
    }
}
