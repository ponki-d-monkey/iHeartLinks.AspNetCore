using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.UrlProviders;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.UrlProviders
{
    public sealed class UrlProviderContextTests
    {
        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkKeyIsNull()
        {
            Action action = () => new UrlProviderContext(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkKey");
        }
    }
}
