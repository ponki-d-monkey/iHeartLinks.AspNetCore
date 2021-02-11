using System;
using FluentAssertions;
using iHeartLinks.Core;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class HypermediaBuilderExtensionTests
    {
        private const string TestHref = "https://iheartlinks.example.com";
        private const string TestMethod = "GET";
        private const string TestRel = "TestRel";
        private const string TestRouteName = "TestRouteName";
        private const string SelfRel = "self";

        private readonly Mock<IHypermediaService> mockService;

        private readonly Mock<IHypermediaBuilder<IHypermediaDocument>> mockSut;
        private readonly IHypermediaBuilder<IHypermediaDocument> sut;

        public HypermediaBuilderExtensionTests()
        {
            mockService = new Mock<IHypermediaService>();
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)))
                .Returns(TestHref);

            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.IsAny<object>()))
                .Returns(TestHref);

            mockService
                .Setup(x => x.GetUrlTemplate(It.Is<string>(y => y == TestRouteName)))
                .Returns(TestHref);

            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(TestMethod);

            mockSut = new Mock<IHypermediaBuilder<IHypermediaDocument>>();
            mockSut
                .Setup(x => x.Service)
                .Returns(mockService.Object);

            sut = mockSut.Object;
        }

        [Fact]
        public void AddRouteLinkShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddRouteLink(TestRel, TestRouteName);

            mockSut.Verify(x => 
                x.AddLink(
                    It.Is<string>(y => y == TestRel), 
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)), 
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenBuildIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithConditionShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddRouteLink(TestRel, TestRouteName, doc => true);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddRouteLink(TestRel, TestRouteName, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddRouteLinkWithConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName, doc => conditionResult);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteLinkWithArgsShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithArgsShouldThrowArgumentNullExceptionWhenArgsIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default(object));

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("args");

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.IsAny<object>()))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithArgsShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithArgsShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenBuildIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsAndConditionShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsAndConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenArgsIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default, doc => true);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("args");

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsAndConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.IsAny<object>()))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithArgsAndConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithArgsAndConditionShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithArgsAndConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddRouteLinkWithArgsAndConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => conditionResult);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddSelfRouteLink(TestRouteName);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddSelfRouteLink(TestRouteName);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName, new { id = 1 });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsShouldThrowArgumentNullExceptionWhenArgsIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default(object));

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("args");

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.IsAny<object>()))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenBuildIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddSelfRouteLink(TestRouteName, doc => true);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddSelfRouteLink(TestRouteName, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddSelfRouteLinkWithConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
        {
            var result = sut.AddSelfRouteLink(TestRouteName, doc => conditionResult);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenBuildIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenArgsIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default, doc => true);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("args");

            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.IsAny<object>()))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithArgsAndConditionShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.Is<string>(y => y == TestRouteName), It.Is<object>(y => y != null)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsAndConditionShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithArgsAndConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetUrl(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddSelfRouteLinkWithArgsAndConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
        {
            var result = sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => conditionResult);

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

            mockService.Verify(x => x.GetUrlTemplate(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
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

            mockService.Verify(x => x.GetUrlTemplate(It.IsAny<string>()), Times.Never);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteTemplateShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetUrlTemplateMethodReturns(string href)
        {
            mockService
                .Setup(x => x.GetUrlTemplate(It.Is<string>(y => y == TestRouteName)))
                .Returns(href);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteTemplate(TestRel, TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No href value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrlTemplate(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteTemplateShouldThrowInvalidOperationExceptionWhenHypermediaServiceGetMethodMethodReturns(string method)
        {
            mockService
                .Setup(x => x.GetMethod(It.Is<string>(y => y == TestRouteName)))
                .Returns(method);

            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteTemplate(TestRel, TestRouteName);

            func.Should().Throw<InvalidOperationException>().Which.Message.Should().Be($"No HTTP method value exists with the given route name. Value of 'routeName': {TestRouteName}");

            mockService.Verify(x => x.GetUrlTemplate(It.Is<string>(y => y == TestRouteName)), Times.Once);
            mockService.Verify(x => x.GetMethod(It.IsAny<string>()), Times.Once);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteTemplateShouldInvokeBuilderAddLinkMethod()
        {
            sut.AddRouteTemplate(TestRel, TestRouteName);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref && y.Method == TestMethod && y.Templated.Value)),
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
