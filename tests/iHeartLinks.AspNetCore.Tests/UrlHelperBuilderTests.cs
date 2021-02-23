using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class UrlHelperBuilderTests
    {
        private readonly Mock<IActionContextAccessor> mockAccessor;
        private readonly Mock<IUrlHelperFactory> mockFactory;

        private readonly UrlHelperBuilder sut;

        public UrlHelperBuilderTests()
        {
            mockAccessor = new Mock<IActionContextAccessor>();
            mockFactory = new Mock<IUrlHelperFactory>();

            sut = new UrlHelperBuilder(
                mockAccessor.Object,
                mockFactory.Object);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenActionContextAccessorIsNull()
        {
            Action action = () => new UrlHelperBuilder(null, mockFactory.Object);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("actionContextAccessor");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenUrlHelperFactoryIsNull()
        {
            Action action = () => new UrlHelperBuilder(mockAccessor.Object, null);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("urlHelperFactory");
        }

        [Fact]
        public void BuildShouldGetActionContext()
        {
            sut.Build();

            mockAccessor.Verify(x => x.ActionContext, Times.Once);
        }

        [Fact]
        public void BuildShouldInvokeGetUrlHelperMethod()
        {
            sut.Build();

            mockFactory.Verify(x => x.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
        }
    }
}
