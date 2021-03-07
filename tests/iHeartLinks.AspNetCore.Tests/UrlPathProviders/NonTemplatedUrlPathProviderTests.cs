using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlPathProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.UrlPathProviders
{
    public sealed class NonTemplatedUrlPathProviderTests
    {
        private const string TestRouteName = "TestRouteName";
        private const string TestRouteUrl = "/person/1";

        private readonly Mock<IUrlHelper> mockUrlHelper;
        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;

        private readonly NonTemplatedUrlPathProvider sut;

        public NonTemplatedUrlPathProviderTests()
        {
            mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);

            sut = new NonTemplatedUrlPathProvider(mockUrlHelperBuilder.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperBuilderIsNull()
        {
            Action action = () => new NonTemplatedUrlPathProvider(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperBuilder");
        }

        [Fact]
        public void ProvideShouldThrowArgumentNullExceptionWhenUrlProviderContextIsNull()
        {
            Func<Uri> func = () => sut.Provide(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldThrowArgumentExceptionWhenLinkRequestDoesNotContainAnId()
        {
            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "templated", "true" }
            });

            var context = new UrlPathProviderContext(linkRequest);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Parameter 'context.LinkRequest' must contain a value for 'id'.");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url format")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlPathProviderContext(linkRequest);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrl()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlPathProviderContext(linkRequest);
            var result = sut.Provide(context);

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url format")]
        public void ProvideWithArgsShouldThrowInvalidOperationExceptionWhenUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlPathProviderContext(linkRequest)
            {
                Args = new object()
            };

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }

        [Fact]
        public void ProvideWithArgsShouldReturnUrl()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlPathProviderContext(linkRequest)
            {
                Args = new object()
            };

            var result = sut.Provide(context);

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }
    }
}
