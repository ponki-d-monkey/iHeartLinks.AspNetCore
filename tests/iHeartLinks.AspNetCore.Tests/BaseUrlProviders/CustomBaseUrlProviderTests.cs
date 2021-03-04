using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.BaseUrlProviders
{
    public sealed class CustomBaseUrlProviderTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("some random text")]
        public void CtorShouldThrowArgumentExceptionWhenBaseUrlIs(string baseUrl)
        {
            Action action = () => new CustomBaseUrlProvider(baseUrl);

            var exception = action.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'baseUrl' must not be null or empty and must be a valid URL.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [InlineData("https://iheartlinks.example.com")]
        [InlineData("https://iheartlinks.example.com/")]
        [InlineData("https://iheartlinks.example.com//")]
        public void ProvideShouldReturnBaseUrl(string baseUrl)
        {
            var sut = new CustomBaseUrlProvider(baseUrl);

            var result = sut.Provide();

            result.Should().Be(baseUrl);
        }
    }
}
