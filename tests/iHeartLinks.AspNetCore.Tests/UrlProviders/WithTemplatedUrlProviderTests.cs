using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using iHeartLinks.AspNetCore.UrlProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.UrlProviders
{
    public sealed class WithTemplatedUrlProviderTests
    {
        private const string TestRouteName = "TestRouteName";
        private const string TestRouteTemplate = "/person/{id}";
        private const string TestRouteUrl = "/person/1";

        private readonly Mock<IUrlHelper> mockUrlHelper;
        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;
        private readonly Mock<IActionDescriptorCollectionProvider> mockProvider;

        private readonly WithTemplatedUrlProvider sut;

        public WithTemplatedUrlProviderTests()
        {
            mockProvider = new Mock<IActionDescriptorCollectionProvider>();
            SetupProvider();

            mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);

            sut = new WithTemplatedUrlProvider(mockProvider.Object, mockUrlHelperBuilder.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenProviderIsNull()
        {
            Action action = () => new WithTemplatedUrlProvider(default, mockUrlHelperBuilder.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("provider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperProviderIsNull()
        {
            Action action = () => new WithTemplatedUrlProvider(mockProvider.Object, default);

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
        public void ProvideShouldThrowArgumentExceptionWhenLinkKeyDoesNotContainAnId()
        {
            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "templated", "true" }
            });

            var context = new UrlProviderContext(linkKey);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Parameter 'context.LinkKey' must contain a value for 'id'.");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldThrowKeyNotFoundExceptionWhenKeyForActionDescriptorDoesNotExistAndLinkKeyHasTemplatedValueTrue()
        {
            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", "NonExistingRouteName" },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlProviderContext(linkKey);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<KeyNotFoundException>().Which.Message.Should().Be("The given 'id' to retrieve the URL template does not exist. Value of 'id': NonExistingRouteName");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url format")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkKeyHasTemplatedValueTrueAndUrlTemplateIs(string template)
        {
            var attributeRouteInfo = new AttributeRouteInfo
            {
                Name = TestRouteName,
                Template = template
            };

            SetupProvider(attributeRouteInfo);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlProviderContext(linkKey);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL template did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlTemplateWhenLinkKeyHasTemplatedValueTrue()
        {
            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", bool.TrueString.ToLower() }
            });

            var context = new UrlProviderContext(linkKey);
            var result = sut.Provide(context);

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteTemplate);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid url format")]
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkKeyDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlProviderContext(linkKey);

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
        public void ProvideShouldThrowInvalidOperationExceptionWhenLinkKeyHasTemplatedValueNotTrueAndUrlHelperReturns(string url, string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(url);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
            });

            var context = new UrlProviderContext(linkKey);

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Fact]
        public void ProvideShouldReturnUrlWhenLinkKeyDoesNotHaveTemplatedValue()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlProviderContext(linkKey);
            var result = sut.Provide(context);

            result.Should().NotBeNull();
            result.ToString().Should().Be(TestRouteUrl);

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Once);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Never);
        }

        [Theory]
        [InlineData("false")]
        [InlineData("untrue")]
        public void ProvideShouldReturnUrlWhenLinkKeyHasTemplatedValueNotTrue(string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)))
                .Returns(TestRouteUrl);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
            });

            var context = new UrlProviderContext(linkKey);
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
        public void ProvideWithArgsShouldThrowInvalidOperationExceptionWhenLinkKeyDoesNotHaveTemplatedValueAndUrlHelperReturns(string url)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlProviderContext(linkKey)
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
        public void ProvideWithArgsShouldThrowInvalidOperationExceptionWhenLinkKeyHasTemplatedValueNotTrueAndUrlHelperReturns(string url, string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(url);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
            });

            var context = new UrlProviderContext(linkKey)
            {
                Args = new object()
            };

            Func<Uri> func = () => sut.Provide(context);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"The given 'id' to retrieve the URL did not provide a valid value. Value of 'id': {TestRouteName}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values == null)), Times.Never);
            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)), Times.Once);
        }

        [Fact]
        public void ProvideWithArgsShouldReturnUrlWhenLinkKeyDoesNotHaveTemplatedValue()
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName }
            });

            var context = new UrlProviderContext(linkKey)
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
        public void ProvideWithArgsShouldReturnUrlWhenLinkKeyHasTemplatedValueNotTrue(string templatedValue)
        {
            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(x => x.RouteName == TestRouteName && x.Values != null)))
                .Returns(TestRouteUrl);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestRouteName },
                { "templated", templatedValue }
            });

            var context = new UrlProviderContext(linkKey)
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
            var actionDescriptor = new ActionDescriptor { AttributeRouteInfo = attributeRouteInfo };
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
