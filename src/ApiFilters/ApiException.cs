using System;
using System.Collections.Generic;

namespace ApiFilters
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public IDictionary<string, IEnumerable<string>> Errors { get; }

        public ApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(Exception ex, int statusCode) : base(ex.Message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, int statusCode, IDictionary<string, IEnumerable<string>> errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
}
