using System.Linq;
using System.Net;
using System.Net.Http;
using FunctionalConcepts;
using Xunit;

namespace Tests.FunctionalConcepts
{
    public class ResultErrorTests
    {
        [Fact]
        public void GetErrorBody_PopulatesBody()
        {
            var jsonBody =
                "{\"message\":\"An Error Occured \",\"errors\":{\"ClientId\":[],\"ContactId\":[],\"BaseIsSell\":[],\"BaseIsDealt\":[],\"DealtAmount\":[\"LOWAMT\"],\"BaseCurrency\":[],\"TermsCurrency\":[]}}";

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(jsonBody)
            };

            var error = new ResultError(response, "error");

            Assert.NotNull(error.ValidationErrors);
            Assert.Equal(1, error.ValidationErrors.Errors.Count);
            Assert.Equal("DealtAmount", error.ValidationErrors.Errors.First().Key);
            Assert.Single(error.ValidationErrors.Errors.First().Value);
            Assert.Equal("LOWAMT", error.ValidationErrors.Errors.First().Value.First());
        }
    }
}
