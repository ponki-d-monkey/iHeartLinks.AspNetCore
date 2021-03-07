﻿using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlPathProviders;
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

        [Fact]
        public void ProvideShouldThrowKeyNotFoundExceptionWhenKeyForActionDescriptorDoesNotExistAndLinkRequestHasTemplatedValueTrue()
        {
            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", "NonExistingRouteName" },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlPathProviderContext(linkRequest);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<KeyNotFoundException>().Which.Message.Should().Be("The given 'id' to retrieve the URL template does not exist. Value of 'id': NonExistingRouteName");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkRequestHasTemplatedValueTrueAndUrlTemplateIs(string template)
        {
            var attributeRouteInfo = new AttributeRouteInfo
            {
                Name = TestRouteName,
                Template = template
            };

            SetupProvider(attributeRouteInfo);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlPathProviderContext(linkRequest);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL template returned a null or empty value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlTemplateWhenLinkRequestHasTemplatedValueTrue()
        {
            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlPathProviderContext(linkRequest);
            var result = sut.Provide(context);

            result.Should().NotBeNull();
            result.ToString().Should().Be($"/{TestRouteTemplate}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlTemplateWithQueryTemplateWhenLinkRequestHasTemplatedValueTrueAndQueryParametersExist()
        {
            parameters.Add(new ParameterDescriptor
            {
                ParameterType = typeof(object),
                BindingInfo = new BindingInfo
                {
                    BindingSource = new BindingSource("Query", "Query", false, false)
                }
            });

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlPathProviderContext(linkRequest);
            var result = sut.Provide(context);

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
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkRequestDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
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

        [Theory]
        [InlineData(null, "false")]
        [InlineData("", "false")]
        [InlineData(" ", "false")]
        [InlineData("invalid url format", "false")]
        [InlineData(null, "untrue")]
        [InlineData("", "untrue")]
        [InlineData(" ", "untrue")]
        [InlineData("invalid url format", "untrue")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkRequestHasTemplatedValueNotTrueAndUrlHelperReturns(string url, string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
            });

            var context = new UrlPathProviderContext(linkRequest);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlWhenLinkRequestDoesNotHaveTemplatedValue()
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
        [InlineData("false")]
        [InlineData("untrue")]
        public void ProvideShouldReturnUrlWhenLinkRequestHasTemplatedValueNotTrue(string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
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
        public void ProvideWithArgsShouldThrowInvalidOperationExceptionWhenLinkRequestDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
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

        [Theory]
        [InlineData(null, "false")]
        [InlineData("", "false")]
        [InlineData(" ", "false")]
        [InlineData("invalid url format", "false")]
        [InlineData(null, "untrue")]
        [InlineData("", "untrue")]
        [InlineData(" ", "untrue")]
        [InlineData("invalid url format", "untrue")]
        public void ProvideWithArgsShouldThrowInvalidOperationExceptionWhenLinkRequestHasTemplatedValueNotTrueAndUrlHelperReturns(string url, string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
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
        public void ProvideWithArgsShouldReturnUrlWhenLinkRequestDoesNotHaveTemplatedValue()
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

        [Theory]
        [InlineData("false")]
        [InlineData("untrue")]
        public void ProvideWithArgsShouldReturnUrlWhenLinkRequestHasTemplatedValueNotTrue(string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var linkRequest = new LinkRequest(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
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