using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ApiMiddleware
{
    public class AntiXssMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AntiXssMiddlewareOptions _options;

        public AntiXssMiddleware(RequestDelegate next, AntiXssMiddlewareOptions options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_options.ExcludeFromXss(context))
            {
                await _next(context);
            }
            else
            {
                // Check XSS in URL
                if (!string.IsNullOrWhiteSpace(context.Request.Path.Value))
                {
                    var url = context.Request.Path.Value;

                    int matchIndex;
                    if (XssValidation.IsDangerousString(url, out matchIndex))
                    {
                        if (_options.ThrowExceptionForXssRequest)
                        {
                            throw new CrossSiteScriptingException(_options.ErrorMessage);
                        }

                        context.Response.Clear();
                        await context.Response.WriteAsync(_options.ErrorMessage);
                        return;
                    }
                }

                // Check XSS in query string
                if (!string.IsNullOrWhiteSpace(context.Request.QueryString.Value))
                {
                    var queryString = WebUtility.UrlDecode(context.Request.QueryString.Value);

                    int matchIndex;
                    if (XssValidation.IsDangerousString(queryString, out matchIndex))
                    {
                        if (_options.ThrowExceptionForXssRequest)
                        {
                            throw new CrossSiteScriptingException(_options.ErrorMessage);
                        }

                        context.Response.Clear();
                        await context.Response.WriteAsync(_options.ErrorMessage);
                        return;
                    }
                }

                // Check XSS in request content
                var originalBody = context.Request.Body;
                var path = context.Request.Path;
                try
                {
                    var content = await ReadRequestBody(context);

                    int matchIndex;
                    if ((path != "/documents/upload") && XssValidation.IsDangerousString(content, out matchIndex))
                    {
                        if (_options.ThrowExceptionForXssRequest)
                        {
                            throw new CrossSiteScriptingException(_options.ErrorMessage);
                        }

                        context.Response.Clear();
                        await context.Response.WriteAsync(_options.ErrorMessage);
                        return;
                    }

                    await _next(context);
                }
                finally
                {
                    context.Request.Body = originalBody;
                }
            }
        }

        private static async Task<string> ReadRequestBody(HttpContext context)
        {
            var buffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(buffer);
            context.Request.Body = buffer;
            buffer.Position = 0;

            var encoding = Encoding.UTF8;
            var contentType = context.Request.GetTypedHeaders().ContentType;
            if (contentType?.Charset.Value != null) encoding = Encoding.GetEncoding(contentType.Charset.Value);

            var requestContent = await new StreamReader(buffer, encoding).ReadToEndAsync();
            context.Request.Body.Position = 0;

            return requestContent;
        }
    }

    public static class AntiXssMiddlewareExtensions
    {
        public static IApplicationBuilder UseAntiXssMiddleware(this IApplicationBuilder builder, bool throwExceptionForXssRequest)
        {
            AntiXssMiddlewareOptions options = new AntiXssMiddlewareOptions("A cross site scripting attack has been detected has been detected", throwExceptionForXssRequest);
            return builder.UseMiddleware<AntiXssMiddleware>(options);
        }

        public static IApplicationBuilder UseAntiXssMiddleware(this IApplicationBuilder builder, bool throwExceptionForXssRequest, Func<HttpContext, bool> excludeFromXssMiddlware)
        {
            AntiXssMiddlewareOptions options = new AntiXssMiddlewareOptions("A cross site scripting attack has been detected has been detected", throwExceptionForXssRequest, excludeFromXssMiddlware);
            return builder.UseMiddleware<AntiXssMiddleware>(options);
        }
    }
}