using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ApiMiddleware
{
    public class SetIdentityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ClaimsPrincipal _user;

        public SetIdentityMiddleware(RequestDelegate next, ClaimsPrincipal user)
        {
            _next = next;
            _user = user;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.User = _user;
            return _next(httpContext);
        }
    }

    public static class SetIdentityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSetIdentityMiddleware(this IApplicationBuilder builder, ClaimsPrincipal user)
        {
            return user == null ? builder : builder.UseMiddleware<SetIdentityMiddleware>(user);
        }
    }
}