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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddLinkWithConditionShouldThrowArgumentNullExceptionWhenBuilderIsNull(bool conditionHandlerResult)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddLink(TestRel, TestHref, TestMethod, doc => conditionHandlerResult);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void AddLinkWithConditionShouldThrowArgumentExceptionWhenRelIs(string rel, bool conditionHandlerResult)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(rel, TestHref, TestMethod, doc => conditionHandlerResult);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<HttpLink>()), Times.Never);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void AddLinkWithConditionShouldThrowArgumentExceptionWhenHrefIs(string href, bool conditionHandlerResult)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(TestRel, href, TestMethod, doc => conditionHandlerResult);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'href' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<HttpLink>()), Times.Never);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData(" ", false)]
        public void AddLinkWithConditionShouldThrowArgumentExceptionWhenMethodIs(string method, bool conditionHandlerResult)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(TestRel, TestHref, method, doc => conditionHandlerResult);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'method' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<HttpLink>()), Times.Never);
        }

        [Fact]
        public void AddLinkWithConditionShouldArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddLink(TestRel, TestHref, TestMethod, default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("conditionHandler");
        }

        [Fact]
        public void AddLinkWithConditionShouldInvokeHypermediaBuilderAddLinkMethodWhenConditionHandlerReturnsTrue()
        {
            sut.AddLink(TestRel, TestHref, TestMethod, doc => true);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<HttpLink>(y => y.Href == TestHref && y.Method == TestMethod && !y.Templated.HasValue)),
                Times.Once);
        }

        [Fact]
        public void AddLinkWithConditionShouldNotInvokeHypermediaBuilderAddLinkMethodWhenConditionHandlerReturnsFalse()
        {
            sut.AddLink(TestRel, TestHref, TestMethod, doc => false);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<HttpLink>(y => y.Href == TestHref && y.Method == TestMethod && !y.Templated.HasValue)),
                Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddLinkWithConditionShouldReturnSameInstanceOfHypermediaBuilder(bool conditionHandlerResult)
        {
            var result = sut.AddLink(TestRel, TestHref, TestMethod, doc => conditionHandlerResult);

            result.Should().BeSameAs(sut);
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

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteValues() == null)), Times.Never);
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

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteValues() == null)), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteTemplateShouldInvokeBuilderAddLinkMethod()
        {
            mockService
                .Setup(x => x.GetLink(It.Is<LinkRequest>(y => y.GetRouteName() == TestRouteName && y.IsTemplated())))
                .Returns(new HttpLink(TestHref, TestMethod)
                {
                    Templated = true
                });

            sut.AddRouteTemplate(TestRel, TestRouteName);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(y => y.GetRouteName() == TestRouteName && y.GetRouteValues() == null && y.IsTemplated())), Times.Once);

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
