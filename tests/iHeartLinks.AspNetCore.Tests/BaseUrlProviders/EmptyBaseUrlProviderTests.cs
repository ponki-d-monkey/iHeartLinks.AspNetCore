using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.BaseUrlProviders
{
    public sealed class EmptyBaseUrlProviderTests
    {
        private readonly EmptyBaseUrlProvider sut;

        public EmptyBaseUrlProviderTests()
        {
            sut = new EmptyBaseUrlProvider();
        }

        [Fact]
        public void GetBaseUrlShouldReturnEmptyString()
        {
            var result = sut.GetBaseUrl();

            result.Should().BeEmpty();
        }
    }
}
