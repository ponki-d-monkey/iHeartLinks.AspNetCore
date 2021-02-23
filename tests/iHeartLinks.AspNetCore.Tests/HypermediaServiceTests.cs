using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class HypermediaServiceTests
    {
        private const string TestMethod = "GET";
        private const string TestRouteName = "TestRouteName";
        private const string TestRouteUrl = "/person/1";
        private const string TestRouteTemplate = "person/{id}";
        private const string TestBaseUrl = "https://iheartlinks.example.com";

        private readonly Mock<IBaseUrlProvider> mockBaseUrlProvider;
        private readonly Mock<IOptions<HypermediaServiceOptions>> mockOptions;
        private readonly Mock<IUrlHelper> mockUrlHelper;
        private readonly Mock<IUrlHelperBuilder> mockUrlHelperBuilder;
        private readonly Mock<IActionDescriptorCollectionProvider> mockProvider;

        private readonly HypermediaService sut;

        public HypermediaServiceTests()
        {
            mockBaseUrlProvider = new Mock<IBaseUrlProvider>();
            mockBaseUrlProvider
                .Setup(x => x.GetBaseUrl())
                .Returns(TestBaseUrl);

            mockOptions = new Mock<IOptions<HypermediaServiceOptions>>();
            mockOptions
                .Setup(x => x.Value)
                .Returns(new HypermediaServiceOptions
                {
                    BaseUrlProvider = mockBaseUrlProvider.Object
                });

            mockUrlHelper = new Mock<IUrlHelper>();
            SetupUrlHelper();

            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(mockUrlHelper.Object);

            mockProvider = new Mock<IActionDescriptorCollectionProvider>();
            SetupProvider();

            sut = new HypermediaService(
                mockOptions.Object,
                mockUrlHelperBuilder.Object, 
                mockProvider.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenHypermediaServiceOptionsIsNull()
        {
            Action action = () => new HypermediaService(default, mockUrlHelperBuilder.Object, mockProvider.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("options");
        }

        [Fact]
        public void CtorShouldInvokeBaseUrlProviderGetBaseUrlMethod()
        {
            mockBaseUrlProvider.Verify(x => x.GetBaseUrl(), Times.Once);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperBuilderIsNull()
        {
            Action action = () => new HypermediaService(mockOptions.Object, default, mockProvider.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperBuilder");
        }

        [Fact]
        public void CtorShouldInvokeUrlHelperBuilderBuildMethod()
        {
            mockUrlHelperBuilder.Verify(x => x.Build(), Times.Once);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenActionDescriptorCollectionProviderIsNull()
        {
            Action action = () => new HypermediaService(mockOptions.Object, mockUrlHelperBuilder.Object, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("provider");
        }

        [Fact]
        public void GetCurrentMethodShouldMethod()
        {
            var result = sut.GetCurrentMethod();

            result.Should().Be(TestMethod);
        }

        [Fact]
        public void GetCurrentUrlShouldReturnCurrentUrl()
        {
            var result = sut.GetCurrentUrl();

            result.Should().Be($"{TestBaseUrl}{TestRouteUrl}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == TestRouteName)), Times.Once);
        }

        [Fact]
        public void GetCurrentUrlTemplateShouldReturnUrlTemplate()
        {
            var result = sut.GetCurrentUrlTemplate();

            result.Should().Be($"{TestBaseUrl}/{TestRouteTemplate}");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetMethodShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<string> func = () => sut.GetMethod(key);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void GetMethodShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist()
        {
            var nonExistingKey = "NonExistingKey";

            Func<string> func = () => sut.GetMethod(nonExistingKey);

            func.Should().Throw<KeyNotFoundException>().Which.Message.Should().Be($"The given key to retrieve the HTTP method does not exist. Value of 'key': {nonExistingKey}");
        }

        [Fact]
        public void GetMethodShouldReturnHttpMethod()
        {
            var result = sut.GetMethod(TestRouteName);

            result.Should().Be(TestMethod);
        }

        [Fact]
        public void GetMethodShouldReturnNullWhenNoHttpMethodExist()
        {
            var endpointMetadata = new List<object>
            {
                new HttpMethodMetadata(new string[0])
            };

            SetupProvider(endpointMetadata);

            var result = sut.GetMethod(TestRouteName);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetUrlShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<string> func = () => sut.GetUrl(key);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockUrlHelper.Verify(x => x.Link(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GetUrlShouldReturnUrl()
        {
            var result = sut.GetUrl(TestRouteName);

            result.Should().Be($"{TestBaseUrl}{TestRouteUrl}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == TestRouteName)), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetUrlWithArgsShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<string> func = () => sut.GetUrl(key, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockUrlHelper.Verify(x => x.Link(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GetUrlWithArgsShouldThrowArgumentNullExceptionWhenArgsIsNull()
        {
            Func<string> func = () => sut.GetUrl(TestRouteName, null);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("args");

            mockUrlHelper.Verify(x => x.Link(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public void GetUrlWitArgsShouldReturnUrl()
        {
            var result = sut.GetUrl(TestRouteName, new { id = 1 });

            result.Should().Be($"{TestBaseUrl}{TestRouteUrl}");

            mockUrlHelper.Verify(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == TestRouteName)), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetUrlTemplateShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<string> func = () => sut.GetUrlTemplate(key);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void GetUrlTemplateShouldThrowKeyNotFoundExceptionWhenKeyDoesNotExist()
        {
            var nonExistingKey = "NonExistingKey";

            Func<string> func = () => sut.GetUrlTemplate(nonExistingKey);

            func.Should().Throw<KeyNotFoundException>().Which.Message.Should().Be($"The given key to retrieve the URL template does not exist. Value of 'key': {nonExistingKey}");
        }

        [Fact]
        public void GetUrlTemplateShouldReturnUrlTemplate()
        {
            var result = sut.GetUrlTemplate(TestRouteName);

            result.Should().Be($"{TestBaseUrl}/{TestRouteTemplate}");
        }

        private void SetupUrlHelper()
        {
            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest
                .Setup(x => x.Method)
                .Returns(TestMethod);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(x => x.Request)
                .Returns(mockHttpRequest.Object);

            var attributeRouteInfo = new AttributeRouteInfo 
            { 
                Name = TestRouteName,
                Template = TestRouteTemplate
            };

            var actionDescriptor = new ActionDescriptor { AttributeRouteInfo = attributeRouteInfo };
            var actionContext = new ActionContext 
            { 
                HttpContext = mockHttpContext.Object,
                ActionDescriptor = actionDescriptor 
            };

            mockUrlHelper
                .Setup(x => x.ActionContext)
                .Returns(actionContext);

            mockUrlHelper
                .Setup(x => x.RouteUrl(It.Is<UrlRouteContext>(y => y.RouteName == TestRouteName)))
                .Returns(TestRouteUrl);
        }

        private void SetupProvider()
        {
            var endpointMetadata = new List<object>
            {
                new HttpMethodMetadata(new string[] { TestMethod })
            };

            SetupProvider(endpointMetadata);
        }

        private void SetupProvider(List<object> endpointMetadata)
        {
            var actionDescriptors = new List<ActionDescriptor>
            {
                new ActionDescriptor
                {
                    EndpointMetadata = endpointMetadata,
                    AttributeRouteInfo = new AttributeRouteInfo 
                    { 
                        Name = TestRouteName,
                        Template = TestRouteTemplate
                    }
                }
            };

            mockProvider
                .Setup(x => x.ActionDescriptors)
                .Returns(new ActionDescriptorCollection(actionDescriptors.AsReadOnly(), 1));
        }
    }
}
