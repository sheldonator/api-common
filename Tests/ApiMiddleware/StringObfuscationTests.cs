using ApiExtensions;
using ApiMiddleware;
using Xunit;

namespace Tests.ApiMiddleware
{
    public class StringObfuscationTests
    {
        private const string JsonContentType = "application / json";
        private const string JsonContentTypeWithCharset = "application/json; charset=utf-8";
        private const string FormContentType = "application/x-www-form-urlencoded";

        [Fact]
        public void BodyIsNull_BodyReturned()
        {
            string request = null;

            var newbody = StringObfuscation.RemovePropertyValues(request, JsonContentType);

            Assert.Equal(request, newbody);
        }

        [Theory]
        [InlineData("{\"password\":\"password\"}", "{\"password\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"oldpassword\":\"password\"}", "{\"oldpassword\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"newpassword\":\"password\"}", "{\"newpassword\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character1\":\"1\"}", "{\"character1\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character2\":\"2\"}", "{\"character2\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character3\":\"3\"}", "{\"character3\":\"[NOT LOGGED]\"}")]
        public void JsonRemovePropertyValues_ValueIsRemoved(string request, string expected)
        {
            var newbody = StringObfuscation.RemovePropertyValues(request, JsonContentType);

            Assert.Equal(expected.StripWhiteSpace(), newbody.StripWhiteSpace());
        }
        [Fact]
        public void StripWhiteSpace_ContentType_null_survives()
        {
            var request = "ThisISSome Text";
            var expected = request;

            var newbody = StringObfuscation.RemovePropertyValues(request, null);

            Assert.Equal(expected, newbody);
        }


        [Theory]
        [InlineData("{\"password\":\"password\"}", "{\"password\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"oldpassword\":\"password\"}", "{\"oldpassword\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"newpassword\":\"password\"}", "{\"newpassword\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character1\":\"1\"}", "{\"character1\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character2\":\"2\"}", "{\"character2\":\"[NOT LOGGED]\"}")]
        [InlineData("{\"character3\":\"3\"}", "{\"character3\":\"[NOT LOGGED]\"}")]
        public void JsonWithCharsetRemovePropertyValues_ValueIsRemoved(string request, string expected)
        {
            var newbody = StringObfuscation.RemovePropertyValues(request, JsonContentTypeWithCharset);

            Assert.Equal(expected.StripWhiteSpace(), newbody.StripWhiteSpace());
        }

        [Fact]
        public void JsonRemovePropertyValues_UsernameIsNotRemoved()
        {
            var request = "{\"username\":\"user\"}";

            var newbody = StringObfuscation.RemovePropertyValues(request, JsonContentType);

            Assert.Equal(request.StripWhiteSpace(), newbody.StripWhiteSpace());
        }

        [Theory]
        [InlineData("?password=password", "?password=%5BNOT%20LOGGED%5D")]
        [InlineData("?oldpassword=password", "?oldpassword=%5BNOT%20LOGGED%5D")]
        [InlineData("?newpassword=password", "?newpassword=%5BNOT%20LOGGED%5D")]
        [InlineData("?character1=1", "?character1=%5BNOT%20LOGGED%5D")]
        [InlineData("?character2=2", "?character2=%5BNOT%20LOGGED%5D")]
        [InlineData("?character3=3", "?character3=%5BNOT%20LOGGED%5D")]
        public void FormRemovePropertyValues_ValueIsRemoved(string request, string expected)
        {
            var newbody = StringObfuscation.RemovePropertyValues(request, FormContentType);

            Assert.Equal(expected.StripWhiteSpace(), newbody.StripWhiteSpace());
        }

        [Fact]
        public void FormRemovePropertyValues_UsernameIsNotRemoved()
        {
            var request = "?username=user";

            var newbody = StringObfuscation.RemovePropertyValues(request, FormContentType);

            Assert.Equal(request.StripWhiteSpace(), newbody.StripWhiteSpace());
        }
    }
}
