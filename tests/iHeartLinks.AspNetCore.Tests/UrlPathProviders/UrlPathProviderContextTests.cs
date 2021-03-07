using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.UrlPathProviders;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.UrlPathProviders
{
    public sealed class UrlPathProviderContextTests
    {
        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkRequestIsNull()
        {
            Action action = () => new UrlPathProviderContext(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkRequest");
        }
    }
}
