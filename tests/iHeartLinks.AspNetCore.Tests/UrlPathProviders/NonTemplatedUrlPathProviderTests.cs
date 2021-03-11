using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
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
        public void ProvideShouldThrowArgumentNullExceptionWhenRequestIsNull()
        {
            Func<Uri> func = () => sut.Provide(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldThrowArgumentExceptionWhenRequestDoesNotContainRouteName()
        {
            // Valid use of LinkRequest ctor to simulate a request w/o a routeName value.
            var request = new LinkRequest(new Dictionary<string, object>
            {
                { "Filler", "filler" }
            });

            Func<Uri> func = () => sut.Provide(request);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Parameter 'request' must contain a 'routeName' value.");

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

            Func<Uri> func = () => sut.Provide(LinkRequestBuilder.CreateWithRouteName(TestRouteName));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL did not provide a valid value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrl()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var result = sut.Provide(LinkRequestBuilder.CreateWithRouteName(TestRouteName));

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
        public void ProvideWithRouteValuesShouldThrowInvalidOperationExceptionWhenUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            Func<Uri> func = () => sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(new object()));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL did not provide a valid value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }

        [Fact]
        public void ProvideWithRouteValuesShouldReturnUrl()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var result = sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(new object()));

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }
    }
}
