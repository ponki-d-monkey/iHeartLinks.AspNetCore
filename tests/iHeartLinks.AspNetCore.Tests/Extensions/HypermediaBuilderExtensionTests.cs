using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.Core;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class HypermediaBuilderExtensionTests
    {
        private const string TestHref = "https://iheartlinks.example.com";
        private const string TestMethod = "GET";
        private const string TestRel = "TestRel";
        private const string TestRouteName = "TestRouteName";

        private readonly Mock<IHypermediaService> mockService;

        private readonly Mock<IHypermediaBuilder<IHypermediaDocument>> mockSut;
        private readonly IHypermediaBuilder<IHypermediaDocument> sut;

        public HypermediaBuilderExtensionTests()
        {
            mockService = new Mock<IHypermediaService>();
            mockSut = new Mock<IHypermediaBuilder<IHypermediaDocument>>();
            mockSut
                .Setup(x => x.Service)
                .Returns(mockService.Object);

            sut = mockSut.Object;
        }

        [Fact]
        public void AddLinkShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddLink(TestRel, TestHref, TestMethod);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddLinkShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(rel, TestHref, TestMethod);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<string>(), It.Is<object>(x => x == null)), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<HttpLink>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddLinkShouldThrowArgumentExceptionWhenHrefIs(string href)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(TestRel, href, TestMethod);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'href' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<HttpLink>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddLinkShouldThrowArgumentExceptionWhenMethodIs(string method)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(TestRel, TestHref, method);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'method' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<HttpLink>()), Times.Never);
        }

        [Fact]
        public void AddLinkShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddLink(TestRel, TestHref, TestMethod);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<HttpLink>(y => y.Href == TestHref && y.Method == TestMethod && !y.Templated.HasValue)),
                Times.Once);
        }

        [Fact]
        public void AddLinkShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddLink(TestRel, TestHref, TestMethod);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteTemplateShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteTemplate(TestRel, TestRouteName);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteTemplateShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteTemplate(rel, TestRouteName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<string>(), It.Is<object>(x => x == null)), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteTemplateShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteTemplate(TestRel, routeName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteTemplateShouldInvokeBuilderAddLinkMethod()
        {
            var request = $"{TestRouteName}|templated={bool.TrueString.ToLower()}";

            mockService
                .Setup(x => x.GetLink(It.Is<string>(y => y == request), It.Is<object>(x => x == null)))
                .Returns(new HttpLink(TestHref, TestMethod)
                {
                    Templated = true
                });

            sut.AddRouteTemplate(TestRel, TestRouteName);

            mockService.Verify(x => x.GetLink(It.Is<string>(x => x == request), It.Is<object>(x => x == null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<HttpLink>(y => y.Href == TestHref && y.Method == TestMethod && y.Templated.Value)),
                Times.Once);
        }

        [Fact]
        public void AddRouteTemplateShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddRouteTemplate(TestRel, TestRouteName);

            result.Should().BeSameAs(sut);
        }
    }
}
