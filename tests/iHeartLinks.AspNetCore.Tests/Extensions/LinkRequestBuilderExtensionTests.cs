using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class LinkRequestBuilderExtensionTests
    {
        [Fact]
        public void SetIsTemplatedShouldThrowArgumentNullExceptionWhenBuilderIsNull()
        {
            Func<LinkRequestBuilder> func = () => default(LinkRequestBuilder).SetIsTemplated();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("builder");
        }

        [Fact]
        public void SetIsTemplatedShouldReturnLinkRequestBuilderThanCanBuildALinkRequestWithTemplatedValueTrue()
        {
            var result = LinkRequestBuilder.CreateWithRouteName("TestRouteName").SetIsTemplated();

            result.Should().NotBeNull();

            var request = result.Build();
            request.Should().NotBeNull();
            request.IsTemplated().Should().BeTrue();
        }
    }
}
