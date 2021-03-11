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
                .Setup(x => x.GetLink(It.Is<LinkRequest>(y => y.GetRouteName() == TestRouteName)))
                .Returns(new Link(TestHref));

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

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
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

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkShouldGetAndAddLink()
        {
            sut.AddRouteLink(TestRel, TestRouteName);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenBuilderIsNull()
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
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
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
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldGetAndAddLink()
        {
            sut.AddRouteLink(TestRel, TestRouteName, doc => true);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddRouteLink(TestRel, TestRouteName, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
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
        public void AddRouteLinkWithRouteValuesShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithRouteValuesShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithRouteValuesShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesShouldThrowArgumentNullExceptionWhenRouteValuesIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default(object));

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("routeValues");

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesShouldGetAndAddLink()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() != null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 });

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithRouteValuesAndConditionShouldThrowArgumentExceptionWhenRelIs(string rel)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(rel, TestRouteName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'rel' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.IsAny<string>(), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddRouteLinkWithRouteValuesAndConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, routeName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenRouteValuesIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, default, doc => true);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("routeValues");

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesAndConditionShouldGetAndAddLink()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => true);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() != null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == TestRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddRouteLinkWithRouteValuesAndConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddRouteLink(TestRel, TestRouteName, new { id = 1 }, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == TestRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddRouteLinkWithRouteValuesAndConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
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

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkShouldGetAndAddLink()
        {
            sut.AddSelfRouteLink(TestRouteName);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddSelfRouteLink(TestRouteName);

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName, new { id = 1 });

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithRouteValuesShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName, new { id = 1 });

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesShouldThrowArgumentNullExceptionWhenRouteValuesIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default(object));

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("routeValues");

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesShouldGetAndAddLink()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() != null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesShouldReturnSameInstanceOfHypermediaBuilder()
        {
            var result = sut.AddSelfRouteLink(TestRouteName, new { id = 1 });

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenBuilderIsNull()
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
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldGetAndAddLink()
        {
            sut.AddSelfRouteLink(TestRouteName, doc => true);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() == null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddSelfRouteLink(TestRouteName, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
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
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => default(IHypermediaBuilder<IHypermediaDocument>).AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(routeName, new { id = 1 }, doc => true);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenRouteValuesIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, default, doc => true);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("routeValues");

            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldThrowArgumentNullExceptionWhenConditionHandlerIsNull()
        {
            Func<IHypermediaBuilder<IHypermediaDocument>> func = () => sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("conditionHandler");

            mockSut.Verify(x => x.Document, Times.Never);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldGetAndAddLink()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => true);

            mockService.Verify(x => x.GetLink(It.Is<LinkRequest>(x => x.GetRouteName() == TestRouteName && x.GetRouteValues() != null)), Times.Once);

            mockSut.Verify(x =>
                x.AddLink(
                    It.Is<string>(y => y == SelfRel),
                    It.Is<Link>(y => y.Href == TestHref)),
                Times.Once);
        }

        [Fact]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldNotExecuteWhenConditionHandlerReturnsFalse()
        {
            sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => false);

            mockSut.Verify(x => x.Document, Times.Once);
            mockService.Verify(x => x.GetLink(It.IsAny<LinkRequest>()), Times.Never);
            mockSut.Verify(x => x.AddLink(It.Is<string>(y => y == SelfRel), It.IsAny<Link>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddSelfRouteLinkWithRouteValuesAndConditionShouldReturnSameInstanceOfHypermediaBuilderWhenConditionHandlerReturns(bool conditionResult)
        {
            var result = sut.AddSelfRouteLink(TestRouteName, new { id = 1 }, doc => conditionResult);

            result.Should().BeSameAs(sut);
        }
    }
}
