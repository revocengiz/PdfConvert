using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfConvert.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfService pdfService;

        public PdfController(PdfService pdfService)
        {
            this.pdfService = pdfService;
        }

        [HttpGet("adasko")]
        public async Task<IActionResult> WithAdaskoTheBeAsT()
        {
            var html = await System.IO.File.ReadAllBytesAsync("html.html");
            var stream = await pdfService.WithAdaskoTheBeAsT(html, HttpContext.RequestAborted);
            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "result.pdf",
            };
        }

        [HttpGet("process")]
        public async Task<IActionResult> WithProcess()
        {
            var html = await System.IO.File.ReadAllBytesAsync("html.html");
            var stream = await pdfService.WithProcess(html, HttpContext.RequestAborted);
            return new FileStreamResult(stream, "application/pdf")
            {
                FileDownloadName = "result.pdf",
            };
        }
    }
}
