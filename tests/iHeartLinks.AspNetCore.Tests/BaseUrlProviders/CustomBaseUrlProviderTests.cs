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
        public void CtorShouldThrowArgumentExceptionWhenCustomUrlIs(string customUrl)
        {
            Action action = () => new CustomBaseUrlProvider(customUrl);

            var exception = action.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'customUrl' must not be null or empty and must be a valid URL.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [InlineData("https://iheartlinks.example.com")]
        [InlineData("https://iheartlinks.example.com/")]
        [InlineData("https://iheartlinks.example.com//")]
        public void ProvideShouldReturnCustomUrl(string customUrl)
        {
            var sut = new CustomBaseUrlProvider(customUrl);

            var result = sut.Provide();

            result.Should().Be(customUrl);
        }
    }
}
