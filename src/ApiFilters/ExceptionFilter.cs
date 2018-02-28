using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace ApiFilters
{
    public class ExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            HttpError apiError;
            if (context.Exception is ApiException)
            {
                var ex = context.Exception as ApiException;
                context.Exception = null;
                apiError = new HttpError(ex.GetBaseException().Message)
                {
                    Errors = ex.Errors
                };

                context.HttpContext.Response.StatusCode = ex.StatusCode;
                LogError(ex);
            }
            else
            {
#if !DEBUG
                var msg = "An error occurred.";  //TODO: pull from resource / CMS              
#else
                var msg = context.Exception.GetBaseException().Message;
#endif
                apiError = new HttpError(msg);
                context.HttpContext.Response.StatusCode = 500;
                LogError(context.Exception);
            }

            context.Result = new JsonResult(apiError);
            base.OnException(context);
        }

        private void LogError(Exception exception)
        {
            Log.Logger.Error($"{exception.Message} {Environment.NewLine} {exception.StackTrace}");
        }
    }

    public class ExceptionFilterFactory : IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new ExceptionFilter();
        }
    }
}