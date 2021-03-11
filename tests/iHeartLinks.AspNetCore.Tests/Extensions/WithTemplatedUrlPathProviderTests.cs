using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class WithTemplatedUrlPathProviderTests
    {
        private const string TestRouteName = "TestRouteName";
        private const string TestRouteTemplate = "person/{id}";
        private const string TestRouteUrl = "/person/1";

        private readonly List<ParameterDescriptor> parameters;
        private readonly Mock<IQueryNameSelector> mockSelector;
        private readonly Mock<IUrlHelper> mockUrlHelper;
        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;
        private readonly Mock<IActionDescriptorCollectionProvider> mockProvider;

        private readonly LinkRequest templatedRequest;
        private readonly LinkRequest nonTemplatedRequest;

        private readonly WithTemplatedUrlPathProvider sut;

        public WithTemplatedUrlPathProviderTests()
        {
            parameters = new List<ParameterDescriptor>();
            mockSelector = new Mock<IQueryNameSelector>();
            mockSelector
                .Setup(x => x.Select(It.IsAny<PropertyInfo[]>()))
                .Returns(new List<string>(new[] { "Filter", "VersionNumber" }));

            mockProvider = new Mock<IActionDescriptorCollectionProvider>();
            SetupProvider();

            mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);

            templatedRequest = LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetIsTemplated();
            nonTemplatedRequest = LinkRequestBuilder.CreateWithRouteName(TestRouteName);

            sut = new WithTemplatedUrlPathProvider(
                mockSelector.Object,
                mockProvider.Object,
                mockUrlHelperBuilder.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenSelectorIsNull()
        {
            Action action = () => new WithTemplatedUrlPathProvider(default, mockProvider.Object, mockUrlHelperBuilder.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("selector");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenProviderIsNull()
        {
            Action action = () => new WithTemplatedUrlPathProvider(mockSelector.Object, default, mockUrlHelperBuilder.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("provider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperProviderIsNull()
        {
            Action action = () => new WithTemplatedUrlPathProvider(mockSelector.Object, mockProvider.Object, default);

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
                { IsTemplatedEnricher.TemplatedKey, true }
            });

            Func<Uri> func = () => sut.Provide(request);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Parameter 'request' must contain a 'routeName' value.");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldThrowKeyNotFoundExceptionWhenKeyForActionDescriptorDoesNotExistAndRequestHasTemplatedValueTrue()
        {
            Func<Uri> func = () => sut.Provide(LinkRequestBuilder
                .CreateWithRouteName("NonExistingRouteName")
                .SetIsTemplated());

            func.Should().Throw<KeyNotFoundException>().Which.Message.Should().Be("The given 'routeName' to retrieve the URL template does not exist. Value of 'routeName': NonExistingRouteName");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenRequestHasTemplatedValueTrueAndUrlTemplateIs(string template)
        {
            var attributeRouteInfo = new AttributeRouteInfo
            {
                Name = TestRouteName,
                Template = template
            };

            SetupProvider(attributeRouteInfo);

            Func<Uri> func = () => sut.Provide(templatedRequest);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL template returned a null or empty value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlTemplateWhenRequestHasTemplatedValueTrue()
        {
            var result = sut.Provide(templatedRequest);

            result.Should().NotBeNull();
            result.ToString().Should().Be($"/{TestRouteTemplate}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlTemplateWithQueryTemplateWhenRequestHasTemplatedValueTrueAndQueryParametersExist()
        {
            parameters.Add(new ParameterDescriptor
            {
                ParameterType = typeof(object),
                BindingInfo = new BindingInfo
                {
                    BindingSource = new BindingSource("Query", "Query", false, false)
                }
            });

            var result = sut.Provide(templatedRequest);

            result.Should().NotBeNull();
            result.ToString().Should().Be($"/{TestRouteTemplate}{{?Filter,VersionNumber}}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url format")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenRequestDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            Func<Uri> func = () => sut.Provide(nonTemplatedRequest);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL did not provide a valid value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("invalid url format", false)]
        [InlineData(null, "untrue")]
        [InlineData("", "untrue")]
        [InlineData(" ", "untrue")]
        [InlineData("invalid url format", "untrue")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenRequestHasTemplatedValueNotTrueAndUrlHelperReturns(string url, object templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            Func<Uri> func = () => sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .Set(IsTemplatedEnricher.TemplatedKey, templatedValue));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL did not provide a valid value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlWhenRequestDoesNotHaveTemplatedValue()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var result = sut.Provide(nonTemplatedRequest);

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(false)]
        [InlineData("untrue")]
        public void ProvideShouldReturnUrlWhenRequestHasTemplatedValueNotTrue(object templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var result = sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .Set(IsTemplatedEnricher.TemplatedKey, templatedValue));

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
        public void ProvideWithRouteValuesShouldThrowInvalidOperationExceptionWhenRequestDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
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

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        [InlineData("invalid url format", false)]
        [InlineData(null, "untrue")]
        [InlineData("", "untrue")]
        [InlineData(" ", "untrue")]
        [InlineData("invalid url format", "untrue")]
        public void ProvideWithRouteValuesShouldThrowInvalidOperationExceptionWhenRequestHasTemplatedValueNotTrueAndUrlHelperReturns(string url, object templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            Func<Uri> func = () => sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(new object())
                .Set(IsTemplatedEnricher.TemplatedKey, templatedValue));

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'routeName' to retrieve the URL did not provide a valid value. Value of 'routeName': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }

        [Fact]
        public void ProvideWithRouteValuesShouldReturnUrlWhenRequestDoesNotHaveTemplatedValue()
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

        [Fact]
        public void ProvideWithRouteValuesShouldReturnUrlWhenRequestHasTemplatedValueFalse()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var result = sut.Provide(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .SetRouteValuesIfNotNull(new object())
                .Set(IsTemplatedEnricher.TemplatedKey, false));

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }

        private void SetupProvider()
        {
            var attributeRouteInfo = new AttributeRouteInfo
            {
                Name = TestRouteName,
                Template = TestRouteTemplate
            };

            SetupProvider(attributeRouteInfo);
        }

        private void SetupProvider(AttributeRouteInfo attributeRouteInfo)
        {
            var actionDescriptor = new ActionDescriptor
            {
                AttributeRouteInfo = attributeRouteInfo,
                Parameters = parameters
            };

            var actionContext = new ActionContext
            {
                ActionDescriptor = actionDescriptor
            };

            mockProvider
                .Setup(x => x.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(new List<ActionDescriptor>
                {
                    actionDescriptor
                }, 1));
        }
    }
}
