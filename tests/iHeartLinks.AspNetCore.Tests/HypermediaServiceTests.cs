using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class HypermediaServiceTests
    {
        private const string TestRouteName = "TestRouteName";
        private const string TestRouteUrl = "/person/1";
        private const string TestBaseUrl = "https://iheartlinks.example.com";

        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;
        private readonly Mock<IBaseUrlProvider> mockBaseUrlProvider;
        private readonly Mock<IUrlPathProvider> mockUrlPathProvider;
        private readonly Mock<ILinkDataEnricher> mockLinkDataEnricher;
        private readonly Mock<ILinkFactory> mockLinkFactory;

        private readonly HypermediaService sut;

        public HypermediaServiceTests()
        {
            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            SetupUrlHelperBuilder();

            mockBaseUrlProvider = new Mock<IBaseUrlProvider>();
            mockBaseUrlProvider
                .Setup(x => x.Provide())
                .Returns(new Uri(TestBaseUrl, UriKind.Absolute));

            mockUrlPathProvider = new Mock<IUrlPathProvider>();
            mockUrlPathProvider
                .Setup(x => x.Provide(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName)))
                .Returns(new Uri(TestRouteUrl, UriKind.RelativeOrAbsolute));

            mockLinkDataEnricher = new Mock<ILinkDataEnricher>();
            mockLinkFactory = new Mock<ILinkFactory>();

            sut = new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockBaseUrlProvider.Object,
                mockUrlPathProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);
        }

        public static IEnumerable<object[]> TestRouteValues = new List<object[]>
        {
            new[] { default(object) },
            new[] { new object() },
            new[] { new Dictionary<string, string>() }
        };

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperBuilderIsNull()
        {
            Action action = () => new HypermediaService(
                default,
                mockBaseUrlProvider.Object,
                mockUrlPathProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperBuilder");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenBaseUrlProviderIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                default,
                mockUrlPathProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("baseUrlProvider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlProviderIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockBaseUrlProvider.Object,
                default,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlPathProvider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenCollectionOfLinkDataEnrichersIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockBaseUrlProvider.Object,
                mockUrlPathProvider.Object,
                default,
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkDataEnrichers");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkFactoryIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockBaseUrlProvider.Object,
                mockUrlPathProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkFactory");
        }

        [Fact]
        public void GetLinkShouldBuildUrlHelper()
        {
            sut.GetLink();

            mockUrlHelperBuilder.Verify(x => x.Build(), Times.Once);
        }

        [Fact]
        public void GetLinkShouldProvideBaseUrl()
        {
            sut.GetLink();

            mockBaseUrlProvider.Verify(x => x.Provide(), Times.Once);
        }

        [Fact]
        public void GetLinkShouldProvideUrl()
        {
            sut.GetLink();

            mockUrlPathProvider.Verify(x => x.Provide(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == null)), Times.Once);
        }

        [Fact]
        public void GetLinkShouldProvideUrlWhenRouteValuesIsNotNull()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "q1", new StringValues("v1") },
                { "q2", new StringValues(new[] { "v2.1", "v2.2" }) }
            });

            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest
                .Setup(x => x.Query)
                .Returns(query);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(mockHttpRequest.Object);

            SetupUrlHelperBuilder(mockHttpContext.Object);

            sut.GetLink();

            mockUrlPathProvider.Verify(x => x.Provide(It.Is<LinkRequest>(x =>
                x.GetRouteName() == TestRouteName &&
                x.GetRouteValues() != null &&
                x.GetRouteValues() is RouteValueDictionary)),
            Times.Once);
        }

        [Fact]
        public void GetLinkShouldPassCorrectKeyValuesToUrlProvider()
        {
            var query = new QueryCollection(new Dictionary<string, StringValues>
            {
                { "q1", new StringValues("v1") },
                { "q2", new StringValues(new[] { "v2.1", "v2.2" }) }
            });

            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest
                .Setup(x => x.Query)
                .Returns(query);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(mockHttpRequest.Object);

            SetupUrlHelperBuilder(mockHttpContext.Object);

            var expectedQuery = default(RouteValueDictionary);
            mockUrlPathProvider
                .Setup(x => x.Provide(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() is RouteValueDictionary)))
                .Returns(new Uri(TestRouteUrl, UriKind.RelativeOrAbsolute))
                .Callback<LinkRequest>(x => expectedQuery = x.GetRouteValues() as RouteValueDictionary);

            sut.GetLink();

            expectedQuery.Should().NotBeNull();
            expectedQuery.Should().Contain("q1", new StringValues("v1"));
            expectedQuery.Should().Contain("q2", new StringValues(new[] { "v2.1", "v2.2" }));
        }

        [Fact]
        public void GetLinkShouldEnrichLinkData()
        {
            sut.GetLink();

            mockLinkDataEnricher.Verify(x => x.Enrich(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName), It.IsNotNull<LinkDataWriter>()), Times.Once);
        }

        [Fact]
        public void GetLinkShouldCreateLink()
        {
            var href = $"{TestBaseUrl}{TestRouteUrl}";
            var result = sut.GetLink();

            mockLinkFactory.Verify(x => x.Create(It.Is<LinkFactoryContext>(x => x.GetHref().ToString() == href)), Times.Once);
        }

        [Fact]
        public void GetLinkWithRequestShouldThrowArgumentNullExceptionWhenRequestIsNull()
        {
            Func<Link> func = () => sut.GetLink(default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void GetLinkWithRequestShouldThrowArgumentExceptionWhenRequestDoesNotContainRouteName()
        {
            // Valid use of LinkRequest ctor to simulate a request w/o a routeName value.
            var request = new LinkRequest(new Dictionary<string, object>
            {
                { "Filler", "Filler" }
            });

            Func<Link> func = () => sut.GetLink(request);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'request' must contain a 'routeName' value.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetLinkWithRequestShouldThrowArgumentExceptionWhenRequestRouteNameIs(string routeName)
        {
            // Valid use of LinkRequest ctor to simulate a request with invalid route name
            var request = new LinkRequest(new Dictionary<string, object>
            {
                { LinkRequestBuilder.RouteNameKey, routeName }
            });

            Func<Link> func = () => sut.GetLink(request);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("The 'routeName' item in the 'request' parameter must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldThrowInvalidOperationExceptionWhenBaseUrlIsNull(object routeValues)
        {
            mockBaseUrlProvider
                .Setup(x => x.Provide())
                .Returns(default(Uri));

            Func<Link> func = () => sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be("The base URL provider must not return a null value.");
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldProvideBaseUrl(object routeValues)
        {
            sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            mockBaseUrlProvider.Verify(x => x.Provide(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldThrowInvalidOperationExceptionWhenUrlPathIsNull(object routeValues)
        {
            mockUrlPathProvider
                .Setup(x => x.Provide(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName)))
                .Returns(default(Uri));

            Func<Link> func = () => sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be("The URL path provider must not return a null value.");
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldProvideUrl(object routeValues)
        {
            sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            mockUrlPathProvider.Verify(x => x.Provide(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == routeValues)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldEnrichLinkData(object routeValues)
        {
            sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            mockLinkDataEnricher.Verify(x => x.Enrich(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName), It.IsNotNull<LinkDataWriter>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestRouteValues))]
        public void GetLinkWithRequestShouldCreateLink(object routeValues)
        {
            var href = $"{TestBaseUrl}{TestRouteUrl}";
            var result = sut.GetLink(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(routeValues));

            mockLinkFactory.Verify(x => x.Create(It.Is<LinkFactoryContext>(x => x.GetHref().ToString() == href)), Times.Once);
        }

        private void SetupUrlHelperBuilder()
        {
            var mockHttpRequest = new Mock<HttpRequest>();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(mockHttpRequest.Object);

            SetupUrlHelperBuilder(mockHttpContext.Object);
        }

        private void SetupUrlHelperBuilder(HttpContext context)
        {
            var attributeRouteInfo = new AttributeRouteInfo
            {
                Name = TestRouteName
            };

            var actionDescriptor = new ActionDescriptor
            {
                AttributeRouteInfo = attributeRouteInfo
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor,
                HttpContext = context
            };

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.ActionContext)
                .Returns(actionContext);

            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);
        }
    }
}
