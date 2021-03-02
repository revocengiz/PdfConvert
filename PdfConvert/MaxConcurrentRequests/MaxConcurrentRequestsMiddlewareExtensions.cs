using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace PdfConvert.MaxConcurrentRequests
{
    public static class MaxConcurrentRequestsMiddlewareExtensions
    {
        public static IApplicationBuilder UseMaxConcurrentRequests(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<MaxConcurrentRequestsMiddleware>();
        }

        public static IApplicationBuilder UseMaxConcurrentRequests(this IApplicationBuilder app, Action<MaxConcurrentRequestsOptions> optionsExpression)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var expr = optionsExpression ?? delegate { };
            var options = new MaxConcurrentRequestsOptions();
            expr(options);

            return app.UseMiddleware<MaxConcurrentRequestsMiddleware>(Options.Create(options));
        }
    }
}
