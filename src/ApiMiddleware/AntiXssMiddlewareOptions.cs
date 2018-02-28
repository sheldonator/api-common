using System;
using Microsoft.AspNetCore.Http;

namespace ApiMiddleware
{
    public class AntiXssMiddlewareOptions
    {
        public bool ThrowExceptionForXssRequest { get; private set; }
        public string ErrorMessage { get; private set; }

        public Func<HttpContext, bool> ExcludeFromXss { get; private set; }

        public AntiXssMiddlewareOptions(string errorMessage, bool throwExceptionForXssRequest, Func<HttpContext, bool> excludeFromXssMiddlware = null)
        {
            ErrorMessage = errorMessage;
            ThrowExceptionForXssRequest = throwExceptionForXssRequest;

            if (excludeFromXssMiddlware == null)
            {
                excludeFromXssMiddlware = context => false;
            }

            ExcludeFromXss = excludeFromXssMiddlware;
        }
    }
}