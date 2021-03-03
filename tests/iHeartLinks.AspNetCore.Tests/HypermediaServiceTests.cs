using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using iHeartLinks.AspNetCore.UrlProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
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
        private readonly Mock<ILinkKeyProcessor> mockLinkKeyProcessor;
        private readonly Mock<IBaseUrlProvider> mockBaseUrlProvider;
        private readonly Mock<IUrlProvider> mockUrlProvider;
        private readonly Mock<ILinkDataEnricher> mockLinkDataEnricher;
        private readonly Mock<ILinkFactory> mockLinkFactory;

        private readonly HypermediaService sut;

        public HypermediaServiceTests()
        {
            mockUrlHelperBuilder = new Mock<IUrlHelperBuilder>();
            mockUrlHelperBuilder
                .Setup(x => x.Build())
                .Returns(CreateUrlHelper);

            var linkKey = new LinkKey(new Dictionary<string, string>
            {
                { LinkKey.IdKey, TestRouteName }
            });

            mockLinkKeyProcessor = new Mock<ILinkKeyProcessor>();
            mockLinkKeyProcessor
                .Setup(x => x.Process(It.Is<string>(x => x == TestRouteName)))
                .Returns(linkKey);

            mockBaseUrlProvider = new Mock<IBaseUrlProvider>();
            mockBaseUrlProvider
                .Setup(x => x.Provide())
                .Returns(TestBaseUrl);

            mockUrlProvider = new Mock<IUrlProvider>();
            mockUrlProvider
                .Setup(x => x.Provide(It.Is<UrlProviderContext>(x => x.LinkKey.Id == TestRouteName)))
                .Returns(new Uri(TestRouteUrl, UriKind.RelativeOrAbsolute));

            mockLinkDataEnricher = new Mock<ILinkDataEnricher>();
            mockLinkFactory = new Mock<ILinkFactory>();

            sut = new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockLinkKeyProcessor.Object,
                mockBaseUrlProvider.Object,
                mockUrlProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);
        }

        public static IEnumerable<object[]> TestArgs = new List<object[]>
        {
            new[] { default(object) },
            new[] { new object() }
        };

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperBuilderIsNull()
        {
            Action action = () => new HypermediaService(
                default,
                mockLinkKeyProcessor.Object,
                mockBaseUrlProvider.Object,
                mockUrlProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperBuilder");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkKeyProcessorIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                default,
                mockBaseUrlProvider.Object,
                mockUrlProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkKeyProcessor");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenBaseUrlProviderIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockLinkKeyProcessor.Object,
                default,
                mockUrlProvider.Object,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("baseUrlProvider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlProviderIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockLinkKeyProcessor.Object,
                mockBaseUrlProvider.Object,
                default,
                new List<ILinkDataEnricher> { mockLinkDataEnricher.Object },
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlProvider");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenCollectionOfLinkDataEnrichersIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockLinkKeyProcessor.Object,
                mockBaseUrlProvider.Object,
                mockUrlProvider.Object,
                default,
                mockLinkFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkDataEnrichers");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkFactoryIsNull()
        {
            Action action = () => new HypermediaService(
                mockUrlHelperBuilder.Object,
                mockLinkKeyProcessor.Object,
                mockBaseUrlProvider.Object,
                mockUrlProvider.Object,
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
        public void GetLinkShouldProcessKey()
        {
            sut.GetLink();

            mockLinkKeyProcessor.Verify(x => x.Process(It.Is<string>(x => x == TestRouteName)), Times.Once);
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

            mockUrlProvider.Verify(x => x.Provide(It.Is<UrlProviderContext>(x => x.LinkKey.Id == TestRouteName && x.Args == null)), Times.Once);
        }

        [Fact]
        public void GetLinkShouldEnrichLinkData()
        {
            sut.GetLink();

            mockLinkDataEnricher.Verify(x => x.Enrich(It.Is<LinkKey>(x => x.Id == TestRouteName), It.IsNotNull<LinkDataWriter>()), Times.Once);
        }

        [Fact]
        public void GetLinkShouldCreateLink()
        {
            var href = $"{TestBaseUrl}{TestRouteUrl}";
            var result = sut.GetLink();

            mockLinkFactory.Verify(x => x.Create(It.Is<LinkFactoryContext>(x => x.GetHref() == href)), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetLinkWithParametersShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<Link> func = () => sut.GetLink(key, new object());

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void GetLinkWithParametersShouldProcessKey(object args)
        {
            sut.GetLink(TestRouteName, args);

            mockLinkKeyProcessor.Verify(x => x.Process(It.Is<string>(x => x == TestRouteName)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void GetLinkWithParametersShouldProvideBaseUrl(object args)
        {
            sut.GetLink(TestRouteName, args);

            mockBaseUrlProvider.Verify(x => x.Provide(), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void GetLinkWithParametersShouldProvideUrl(object args)
        {
            sut.GetLink(TestRouteName, args);

            mockUrlProvider.Verify(x => x.Provide(It.Is<UrlProviderContext>(x => x.LinkKey.Id == TestRouteName && x.Args == args)), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void GetLinkWithParametersShouldEnrichLinkData(object args)
        {
            sut.GetLink(TestRouteName, args);

            mockLinkDataEnricher.Verify(x => x.Enrich(It.Is<LinkKey>(x => x.Id == TestRouteName), It.IsNotNull<LinkDataWriter>()), Times.Once);
        }

        [Theory]
        [MemberData(nameof(TestArgs))]
        public void GetLinkWithParametersShouldCreateLink(object args)
        {
            var href = $"{TestBaseUrl}{TestRouteUrl}";
            var result = sut.GetLink(TestRouteName, args);

            mockLinkFactory.Verify(x => x.Create(It.Is<LinkFactoryContext>(x => x.GetHref() == href)), Times.Once);
        }

        private IUrlHelper CreateUrlHelper()
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
                ActionDescriptor = actionDescriptor
            };

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper
                .Setup(x => x.ActionContext)
                .Returns(actionContext);

            return mockUrlHelper.Object;
        }
    }
}
