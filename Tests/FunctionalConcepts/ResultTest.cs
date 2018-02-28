using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FunctionalConcepts;
using Newtonsoft.Json;
using Xunit;

namespace Tests.FunctionalConcepts
{
    public class ResultTest
    {
        private const string ErrorMessage = "API";

        private HttpResponseMessage GetFakeResponse(HttpStatusCode statusCode, string body)
        {
            var response = new HttpResponseMessage {StatusCode = statusCode};
            if (!(statusCode == HttpStatusCode.BadRequest
                  || statusCode == HttpStatusCode.InternalServerError
                  || statusCode == HttpStatusCode.OK))
                return response;
            if (body != null)
                response.Content = new StringContent(body);
            return response;
        }

        private string GetFakeBody<T>(T data)
        {
            return JsonConvert.SerializeObject(data);
        }

        [Fact]
        public async Task Success_returns_data_cast_to_type()
        {
            int data = 42;
            var fakeResponse = GetFakeResponse(HttpStatusCode.OK, GetFakeBody(data));

            var result = await Result.FromHttpResponse<int>(fakeResponse, ErrorMessage);

            Assert.True(result.IsSuccess);
            Assert.Equal(data, result.Value);
        }

        [Fact]
        public async Task Success_with_null_data_returns_failure()
        {
            string data = null;
            var fakeResponse = GetFakeResponse(HttpStatusCode.OK, GetFakeBody(data));

            var result = await Result.FromHttpResponse<string>(fakeResponse, ErrorMessage);

            Assert.False(result.IsSuccess);
            Assert.Equal("API", result.Error.Message);
        }

        [Fact]
        public async Task Failure_returns_default_error()
        {
            int data = 42;
            var fakeResponse = GetFakeResponse(HttpStatusCode.InternalServerError, GetFakeBody(data));

            var result = await Result.FromHttpResponse<int>(fakeResponse, ErrorMessage);

            Assert.False(result.IsSuccess);
            Assert.Equal("API", result.Error.Message);
        }

        [Fact]
        public async Task BadRequest_returns_default_error()
        {
            int data = 42;
            var fakeResponse = GetFakeResponse(HttpStatusCode.BadRequest, GetFakeBody(data));

            var result = await Result.FromHttpResponse<int>(fakeResponse, ErrorMessage);

            Assert.False(result.IsSuccess);
            Assert.Equal("API", result.Error.Message);
        }

        [Fact]
        public async Task Failure_returns_received_error()
        {
            var fakeResponse = GetFakeResponse(HttpStatusCode.InternalServerError, "ERROR");

            var result = await Result.FromHttpResponse<int>(fakeResponse, ErrorMessage, true);

            Assert.False(result.IsSuccess);
            Assert.Equal("ERROR", result.Error.Message);
        }

        [Fact]
        public async Task Failure_without_body_returns_default_error()
        {
            var fakeResponse = GetFakeResponse(HttpStatusCode.InternalServerError, null);

            var result = await Result.FromHttpResponse<int>(fakeResponse, ErrorMessage, true);

            Assert.False(result.IsSuccess);
            Assert.Equal("API", result.Error.Message);
        }

    }
}
