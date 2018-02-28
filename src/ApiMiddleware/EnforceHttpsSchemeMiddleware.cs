using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace ApiMiddleware
{
    public class EnforceHttpsSchemeMiddleware
    {
        private readonly IConfigurationRoot _config;
        private readonly RequestDelegate _next;

        public EnforceHttpsSchemeMiddleware(RequestDelegate next, IConfigurationRoot config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Request.Scheme = "https";

            await _next.Invoke(httpContext);
        }
    }

    public static class EnforceHttpsSchemeMiddlewareExtensions
    {
        public static IApplicationBuilder UseEnforceHttpsSchemeMiddleware(this IApplicationBuilder builder, bool enforceHttps)
        {
            if (enforceHttps)
            {
                builder.UseMiddleware<EnforceHttpsSchemeMiddleware>();
            }

            return builder;
        }
    }
}