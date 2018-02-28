using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FunctionalConcepts
{
    public class ResultError
    {
        public ErrorType Type { get; }
        public ValidationErrors ValidationErrors { get; }
        public string Message { get; }

        public ResultError(string message, ErrorType type, ValidationErrors validationErrors)
        {
            Type = type;
            ValidationErrors = validationErrors;
            Message = message;
        }

        public ResultError(string message, ErrorType type)
        {
            Type = type;
            Message = message;
        }

        public ResultError(string message)
        {
            Message = message;
            Type = ErrorType.Unknown;
        }

        public ResultError(HttpResponseMessage response, string message)
        {
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    Type = ErrorType.AuthorizationFailed;
                    break;
                case HttpStatusCode.GatewayTimeout:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.NotFound:
                    Type = ErrorType.NotFound;
                    break;
                case HttpStatusCode.BadRequest:
                    Type = ErrorType.BadRequest;
                    ValidationErrors = GetErrorBody(response);
                    break;
                default:
                    Type = ErrorType.Unknown;
                    break;
            }
            Message = message;
        }

        private static ValidationErrors GetErrorBody(HttpResponseMessage response)
        {
            if (response.Content == null) return null;

            try
            {
                var jsonValidationError = ReadAsString(response.Content);
                var validationObject = JsonConvert.DeserializeObject<ValidationErrors>(jsonValidationError);
                var errors = validationObject.Errors?.Where(e => e.Value.Any()).ToDictionary(e => e.Key, e => e.Value);
                
                return errors == null ? null : new ValidationErrors{Message = validationObject.Message, Errors = errors};
            }
            catch (Exception e)
            {
                var logger = Serilog.Log.Logger;
                logger.Error($"Failed to serialize error body ot validation error with exception: {e.Message}");
                return null;
            }
        }

        private static string ReadAsString(HttpContent content)
        {
            var taskFactory = new
                TaskFactory(CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskContinuationOptions.None,
                    TaskScheduler.Default);

            return taskFactory
                .StartNew(content.ReadAsStringAsync)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }

    public enum ErrorType
    {
        Unknown,
        AuthorizationFailed,
        NotFound,
        BadRequest,
    }

    public class ValidationErrors
    {
        public string Message { get; set; }
        public IDictionary<string, IEnumerable<string>> Errors { get; set; }
    }
}
