using System.Reflection;
using System.Threading.Tasks;
using ApiExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace ApiMiddleware
{
    public class AddLogParametersToLogContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SerilogConfig _serilogConfig;

        public AddLogParametersToLogContextMiddleware(RequestDelegate next,
            IOptions<SerilogConfig> serilogConfig)
        {
            _next = next;
            _serilogConfig = serilogConfig.Value;
        }

        public Task Invoke(HttpContext context)
        {
            var correlationId = new CorrelationId(context);
            using (LogContext.PushProperty("ContactId", context.User.ContactId()))
            using (LogContext.PushProperty("Username", context.User.Username()))
            using (LogContext.PushProperty("CorrelationId", correlationId.GetCorrelationId()))
            using (LogContext.PushProperty("applicationName", GetApplicationName(context.Request.Path)))
            using (LogContext.PushProperty("moduleName", _serilogConfig.ModuleName))
            using (LogContext.PushProperty("assemblyVersion",
                    Assembly.GetEntryAssembly()?
                        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                        .InformationalVersion))
            {
                return _next(context);
            }
        }

        private string GetApplicationName(PathString path)
        {
            if (path.ToString().ToLower() == "/logging/addlog")
                return _serilogConfig.AppApplicationName;

            return _serilogConfig.ApplicationName;
        }
    }

    public static class AddLogParametersToLogContextMiddlewareExtensions
    {
        public static IApplicationBuilder UseAddLogParametersToLogContextMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AddLogParametersToLogContextMiddleware>();
        }
    }
}