using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FunctionalConcepts
{
    public class Result
    {
        public bool IsSuccess { get; }
        public ResultError Error { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, ResultError error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException();
            if (!isSuccess && error == null)
                throw new InvalidOperationException();

            IsSuccess = isSuccess;
            Error = error;
        }
   
        public static Result Fail(string message)
        {
            return new Result(false, new ResultError(message));
        }

        public static Result<T> Fail<T>(string message)
        {
            return new Result<T>(default(T), false, new ResultError(message));
        }

        public static Result Fail(string message, ErrorType type)
        {
            return new Result(false, new ResultError(message, type));
        }

        public static Result<T> Fail<T>(string message, ErrorType type)
        {
            return new Result<T>(default(T), false, new ResultError(message, type));
        }

        public static Result<T> Fail<T>(string message, ErrorType type, ValidationErrors validationErrors)
        {
            return new Result<T>(default(T), false, new ResultError(message, type, validationErrors));
        }

        public static Result Fail(ResultError error)
        {
            return new Result(false, error);
        }

        public static Result<T> Fail<T>(ResultError error)
        {
            return new Result<T>(default(T), false, error);
        }

        public static Result Ok()
        {
            return new Result(true, null);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, null);
        }

        public static Result Combine(params Result[] results)
        {
            foreach (var result in results)
            {
                if (result.IsFailure)
                    return result;
            }

            return Ok();
        }

        public Result Log(string logMessage = "")
        {
            return Log(logMessage, IsSuccess);
        }
        public Result Log(string logMessage, Exception exception)
        {
            return Log(logMessage, false, exception);
        }


        public Result LogAsInfo(string logMessage)
        {
            return Log(logMessage, true);
        }

        public Result LogOnSuccess(string logMessage)
        {
            return IsSuccess ? Log(logMessage, logAsInfo: true) : this;
        }

        public Result LogOnFailure(string logMessage = "", bool logAsInfo = false)
        {
            return IsFailure ? Log(logMessage, logAsInfo) : this;
        }

        private Result Log(string logMessage, bool logAsInfo, Exception exception = null)
        {
            var logger = Serilog.Log.Logger;

            if (IsFailure)
                logMessage = string.IsNullOrEmpty(logMessage) ? Error.Message : Error.Message + "-" + logMessage;

            if (logAsInfo)
                logger.Information(logMessage);
            else
                logger.Error(logMessage);
            if (exception != null)
                logger.Error(exception,logMessage);
            return this;
        }

        public static async Task<Result<T>> FromHttpResponse<T>(
            HttpResponseMessage response, 
            string message,
            bool useErrorMessageFromResponse = false)
        {
            var logMessage = $"{response.RequestMessage?.RequestUri} call failed with http status code {response.StatusCode}{Environment.NewLine}{Environment.StackTrace}";

            var responseString = response.Content == null ? string.Empty : await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return Fail<T>(new ResultError(response, 
                    useErrorMessageFromResponse && !string.IsNullOrWhiteSpace(responseString) 
                    ? responseString 
                    : message)).Log(logMessage);

            }
            var value = JsonConvert.DeserializeObject<T>(responseString);

            return value == null
                ? Fail<T>(message)
                : Ok(value);
        }

        public static Result FromHttpResponse(HttpResponseMessage response, string message)
        {
            return response.IsSuccessStatusCode ? Ok() : Fail(new ResultError(response, message));
        }
    }

    public class Result<T> : Result
    {
        private readonly T _value;

        public T Value => !IsSuccess ? default(T) : _value;
        public bool HasValue => Value != null;

        protected internal Result(T value, bool isSuccess, ResultError error)
            : base(isSuccess, error)
        {
            _value = value;
        }

        public new Result<T> Log(string logMessage = "")
        {
            base.Log(logMessage);
            return this;
        }

        public new Result<T> LogAsInfo(string logMessage = "")
        {
            base.LogAsInfo(logMessage);
            return this;
        }

        public new Result<T> LogOnSuccess(string logMessage)
        {
            base.LogOnSuccess(logMessage);
            return this;
        }

        public new Result<T> LogOnFailure(string logMessage = "", bool logAsInfo = false)
        {
            base.LogOnFailure(logMessage);
            return this;
        }
    }
}
