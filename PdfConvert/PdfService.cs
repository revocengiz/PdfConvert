using AdaskoTheBeAsT.WkHtmlToX.Abstractions;
using AdaskoTheBeAsT.WkHtmlToX.Documents;
using AdaskoTheBeAsT.WkHtmlToX.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PdfConvert
{
    public class PdfService
    {
        private readonly IPdfConverter pdfConverter;
        private readonly IWebHostEnvironment hostEnvironment;

        public PdfService(IPdfConverter pdfConverter)
        {
            this.pdfConverter = pdfConverter;
            //this.hostEnvironment = hostEnvironment;
        }
        public async Task<Stream> WithAdaskoTheBeAsT(byte[] html, CancellationToken cancellationToken = default)
        {
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = AdaskoTheBeAsT.WkHtmlToX.Utils.PaperKind.A4,
                },
                ObjectSettings =
                {
                    new PdfObjectSettings
                    {
                        HtmlContentByteArray=html,
                        PagesCount = true,
                        WebSettings ={DefaultEncoding = "utf-8"},
                        FooterSettings =
                        {
                            FontSize = 9, Center = "[page]/[toPage]"
                        }
                    },
                },
            };

            Stream stream = null;
            var converted = await pdfConverter.ConvertAsync(
                doc,
                length =>
                {
                    stream =
                    new RecyclableMemoryStreamManager().GetStream(
                        Guid.NewGuid(),
                        "wkhtmltox",
                        length);
                    return stream;
                },
                cancellationToken);
            stream!.Position = 0;
            if (converted)
            {
                return stream;
            }
            throw new Exception("Failed to convert.");
        }

        //Tüm switcher => https://wkhtmltopdf.org/usage/wkhtmltopdf.txt
        const string convertswitches = "-q"
            + " --footer-center [page]/[toPage]"
            + " --margin-bottom 10mm --margin-left 10mm --margin-right 10mm --margin-top 10mm"
            + " --javascript-delay 200 --page-offset 0"
            + " --enable-smart-shrinking - -";
        public async Task<MemoryStream> WithProcess(byte[] html, CancellationToken cancellationToken = default)
        {
            var workingDirectory = Directory.GetCurrentDirectory();// hostEnvironment.ContentRootPath;
            var fileName = Path.Combine(workingDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "wkhtmltopdf.exe" : "wkhtmltopdf");

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(fileName, convertswitches)
                {
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardInputEncoding = Encoding.UTF8,
                };
                process.Start();
                using (var stdIn = process.StandardInput)
                using (var htmlStream = new MemoryStream(html))
                using (var reader = new StreamReader(htmlStream, Encoding.UTF8))
                {
                    await stdIn.WriteAsync(await reader.ReadToEndAsync());
                }

                var str = new MemoryStream();
                using (var stdOut = process.StandardOutput.BaseStream)
                {
                    await process.StandardOutput.BaseStream.CopyToAsync(str, cancellationToken);
                }
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = await process.StandardError.ReadToEndAsync();
                    throw new Exception(error);
                }
                str.Seek(0, SeekOrigin.Begin);
                return str;
            }
        }
    }
}
