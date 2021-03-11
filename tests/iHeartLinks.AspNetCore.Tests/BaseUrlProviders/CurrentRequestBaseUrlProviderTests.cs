using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProviderTests
    {
        private const string TestScheme = "https";
        private const string TestHost = "iheartlinks.example.com";

        private readonly Mock<HttpRequest> mockHttpRequest;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;

        private readonly CurrentRequestBaseUrlProvider sut;

        public CurrentRequestBaseUrlProviderTests()
        {
            mockHttpRequest = new Mock<HttpRequest>();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            SetupHttpContextAccessor();

            sut = new CurrentRequestBaseUrlProvider(mockHttpContextAccessor.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenHttpContextAccessorIsNull()
        {
            Action action = () => new CurrentRequestBaseUrlProvider(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("httpContextAccessor");
        }

        [Fact]
        public void ProvideShouldBuildUrlFromRequest()
        {
            var result = sut.Provide();

            result.Should().Be($"{TestScheme}://{TestHost}");

            mockHttpContextAccessor.Verify(x => x.HttpContext, Times.Once);

            mockHttpRequest.Verify(x => x.Scheme, Times.Once);
            mockHttpRequest.Verify(x => x.Host, Times.Once);
        }

        private void SetupHttpContextAccessor()
        {
            mockHttpRequest
                .Setup(x => x.Scheme)
                .Returns(TestScheme);

            mockHttpRequest
                .Setup(x => x.Host)
                .Returns(new HostString(TestHost));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(mockHttpRequest.Object);

            mockHttpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(mockHttpContext.Object);
        }
    }
}
