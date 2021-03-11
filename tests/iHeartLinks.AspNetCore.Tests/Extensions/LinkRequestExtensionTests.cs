using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class LinkRequestExtensionTests
    {
        private const string TestRouteName = "TestRouteName";

        [Fact]
        public void IsTemplatedShouldThrowArgumentNullExceptionWhenRequestIsNull()
        {
            Func<bool> func = () => default(LinkRequest).IsTemplated();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void IsTemplatedShouldReturnFalseWhenRequestDoesNotContainAValueForTemplated()
        {
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName);

            var result = request.IsTemplated();
            result.Should().BeFalse();
        }

        [Fact]
        public void IsTemplatedShouldReturnFalseWhenRequestContainsAnInvalidTemplatedValue()
        {
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName).Set(IsTemplatedEnricher.TemplatedKey, "yes");

            var result = request.IsTemplated();
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsTemplatedShouldReturnCorrectValue(bool isTemplated)
        {
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName).Set(IsTemplatedEnricher.TemplatedKey, isTemplated);

            var result = request.IsTemplated();
            result.Should().Be(isTemplated);
        }
    }
}
