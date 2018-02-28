using Microsoft.AspNetCore.Http;

namespace ApiMiddleware
{
    public class CorrelationId
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";

        private readonly HttpContext _context;

        public CorrelationId(HttpContext context)
        {
            _context = context;
        }

        public string GetCorrelationId()
        {
            if (_context.Request.Headers.ContainsKey(CorrelationIdHeaderName))
                return _context.Request.Headers[CorrelationIdHeaderName];

            return _context.TraceIdentifier;
        }
    }
}
