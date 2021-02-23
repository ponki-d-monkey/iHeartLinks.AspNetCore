using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProviderTests
    {
        private const string TestScheme = "https";
        private const string TestHost = "iheartlinks.example.com";

        private readonly Mock<HttpRequest>  mockHttpRequest;
        private readonly Mock<IUrlHelper> mockUrlHelper;
        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;

        private readonly CurrentRequestBaseUrlProvider sut;

        public CurrentRequestBaseUrlProviderTests()
        {
            mockHttpRequest = new Mock<HttpRequest>();
            mockUrlHelper = new Mock<IUrlHelper>();
            SetupUrlHelper();

            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);

            sut = new CurrentRequestBaseUrlProvider(mockUrlHelperBuilder.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperBuilderIsNull()
        {
            Action action = () => new CurrentRequestBaseUrlProvider(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperBuilder");
        }

        [Fact]
        public void CtorShouldInvokeUrlHelperBuilderBuildMethod()
        {
            mockUrlHelperBuilder.Verify(x => x.Build(), Times.Once);
        }

        [Fact]
        public void GetBaseUrlShouldBuildUrlFromRequest()
        {
            var result = sut.GetBaseUrl();

            result.Should().Be($"{TestScheme}://{TestHost}");
            mockHttpRequest.Verify(x => x.Scheme, Times.Once);
            mockHttpRequest.Verify(x => x.Host, Times.Once);
        }

        private void SetupUrlHelper()
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

            var actionContext = new ActionContext
            {
                HttpContext = mockHttpContext.Object
            };

            mockUrlHelper
                .Setup(x => x.ActionContext)
                .Returns(actionContext);
        }
    }
}
