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
        public void ProvideShouldReturnEmptyString()
        {
            var result = sut.Provide();

            result.Should().BeEmpty();
        }
    }
}
