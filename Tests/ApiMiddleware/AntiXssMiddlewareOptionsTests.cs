using ApiMiddleware;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Tests.ApiMiddleware
{
    public class AntiXssMiddlewareOptionsTests
    {
        [Fact]
        public void Constructor_SupplyexcludeFromXssMiddlwareFunction_CorrectlySetsFunction()
        {
            // Arrange
            var mockHttpContext = A.Fake<HttpContext>();
            var options = new AntiXssMiddlewareOptions("XXXX", true, h => true);

            // Act
            var result = options.ExcludeFromXss(mockHttpContext);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Constructor_excludeFromXssMiddlwareFunctionNotSupplied_CorrectlySetsFunction()
        {
            // Arrange
            var mockHttpContext = A.Fake<HttpContext>();
            var options = new AntiXssMiddlewareOptions("XXXX", true);

            // Act
            var result = options.ExcludeFromXss(mockHttpContext);

            // Assert
            Assert.False(result);
        }

    }
}
