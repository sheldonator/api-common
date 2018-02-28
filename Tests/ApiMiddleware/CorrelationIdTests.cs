using System.IO;
using System.Text;
using ApiMiddleware;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Tests.ApiMiddleware
{
    public class CorrelationIdTests
    {
        private const string CorrelationIdHeaderName = "X-Correlation-Id";
        private const string JsonContentType = "application / json";

        [Fact]
        public void CorrelationIdInHeader_ReturnsCorrelationId()
        {
            var context = A.Fake<HttpContext>();
            var body = "";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            var headers = new HeaderDictionary();
            headers.Add(CorrelationIdHeaderName, "purple");

            A.CallTo(() => context.Request.ContentType).Returns(JsonContentType);
            A.CallTo(() => context.Request.Body).Returns(stream);
            A.CallTo(() => context.Request.Headers).Returns(headers);
            A.CallTo(() => context.TraceIdentifier).Returns("trace");

            var correlation = new CorrelationId(context);

            var correlationId = correlation.GetCorrelationId();

            Assert.Equal("purple", correlationId);
        }

        [Fact]
        public void NoPassedCorrelationId_ReturnsTrace()
        {
            var context = A.Fake<HttpContext>();
            var body = "";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            var headers = new HeaderDictionary();

            A.CallTo(() => context.Request.ContentType).Returns(JsonContentType);
            A.CallTo(() => context.Request.Body).Returns(stream);
            A.CallTo(() => context.Request.Headers).Returns(headers);
            A.CallTo(() => context.TraceIdentifier).Returns("trace");

            var correlation = new CorrelationId(context);

            var correlationId = correlation.GetCorrelationId();

            Assert.Equal("trace", correlationId);
        }

        [Fact]
        public void NotJsonBody_DoesNotError()
        {
            var context = A.Fake<HttpContext>();
            var body = "1a161b4e-2287-4c75-99ef-44b07793d7fc";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            var headers = new HeaderDictionary();

            A.CallTo(() => context.Request.ContentType).Returns(JsonContentType);
            A.CallTo(() => context.Request.Body).Returns(stream);
            A.CallTo(() => context.Request.Headers).Returns(headers);
            A.CallTo(() => context.TraceIdentifier).Returns("trace");

            var correlation = new CorrelationId(context);

            var correlationId = correlation.GetCorrelationId();

            Assert.Equal("trace", correlationId);
        }
    }
}
