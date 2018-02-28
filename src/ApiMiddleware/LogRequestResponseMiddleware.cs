using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace ApiMiddleware
{
    public class LogRequestResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<string> _propertyNamesToIgnore;
        private const int MaxLogCharacters = 3000;

        public LogRequestResponseMiddleware(RequestDelegate next,
            IEnumerable<string> propertyNamesToIgnore = null)
        {
            _next = next;
            _propertyNamesToIgnore = propertyNamesToIgnore;
        }

        public async Task Invoke(HttpContext httpContext,
            ILogger<LogRequestResponseMiddleware> logger)
        {
            if (!IsIgnored(httpContext.Request))
            {
                await LogRequest(httpContext, logger);
                await LogResponse(httpContext, logger);
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }

        private async Task LogRequest(HttpContext httpContext,
            ILogger<LogRequestResponseMiddleware> logger)
        {
            var method = httpContext.Request.Method;
            var url = httpContext.Request.GetDisplayUrl();
            var body = string.Empty;

            if (IsPost(method) && !IsMultipartContentType(httpContext.Request.ContentType))
            {
                var requestBodyStream = new MemoryStream();
                await httpContext.Request.Body.CopyToAsync(requestBodyStream);

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(requestBodyStream, Encoding.UTF8, false, 4000, true))
                {
                    var requestBodyText = await reader.ReadToEndAsync();
                    body = StringObfuscation.RemovePropertyValues(requestBodyText, httpContext.Request.ContentType,
                        _propertyNamesToIgnore);
                }

                requestBodyStream.Seek(0, SeekOrigin.Begin);
                httpContext.Request.Body = requestBodyStream;
            }
            logger.LogInformation($"{method} request to {url} {Environment.NewLine} {body}");
        }
        
        private async Task LogResponse(HttpContext httpContext, 
            ILogger<LogRequestResponseMiddleware> logger)
        {   
            using (var memoryStream = new MemoryStream())
            {
                var bodyStream = httpContext.Response.Body;
                httpContext.Response.Body = memoryStream;

                await _next.Invoke(httpContext);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var bufferReader = new StreamReader(memoryStream))
                {
                    var url = httpContext.Request.GetDisplayUrl();
                    var statusCode = httpContext.Response.StatusCode.ToString();
                    string body = await bufferReader.ReadToEndAsync();
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    if (memoryStream.Length > 0)
                    {
                        await memoryStream.CopyToAsync(bodyStream);
                        httpContext.Response.Body = bodyStream;
                        logger.LogInformation($"Response to {url} with status code {statusCode} {Environment.NewLine} {body.Truncate(MaxLogCharacters)}");
                    }
                    else
                        logger.LogInformation($"Response to {url} with status code {statusCode} has no body");
                }
            }
        }

        private static bool IsPost(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsOptions(string method)
        {
            return method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsIgnored(HttpRequest request)
        {
            if (IsOptions(request.Method))
                return true;

            var path = request.Path;

            var ignoredExtensions = new List<string> { ".css", ".js", ".jpg", ".png", ".gif" };
            return 
                path.StartsWithSegments(new PathString("/lib"), StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(new PathString("/css"), StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(new PathString("/swagger"), StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(new PathString("/diagnostics/heartbeat"), StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(new PathString("/connect/checksession"), StringComparison.OrdinalIgnoreCase) ||
                path.StartsWithSegments(new PathString("/logging/addlog"), StringComparison.OrdinalIgnoreCase) ||
                ignoredExtensions.Any(s => path.ToString().Contains(s));
        }
        private bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public static class LogRequestResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogRequestResponseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogRequestResponseMiddleware>();
        }
    }
}