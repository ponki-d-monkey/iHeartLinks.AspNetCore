using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public class HypermediaServiceOptionsExtensionTests
    {
        private const string TestCustomUrl = "https://iheartlinks.example.com";

        private readonly HypermediaServiceOptions sut;

        public HypermediaServiceOptionsExtensionTests()
        {
            sut = new HypermediaServiceOptions();
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldThrowArgumentNullExceptionWhenOptionsIsNull()
        {
            Func<HypermediaServiceOptions> func = () => default(HypermediaServiceOptions).UseAbsoluteUrlHref(Mock.Of<IHttpContextAccessor>());

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("options");
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldThrowArgumentNullExceptionWhenHttpContextAccessorIsNull()
        {
            Func<HypermediaServiceOptions> func = () => sut.UseAbsoluteUrlHref(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("httpContextAccessor");
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldSetBaseUrlProviderToCurrentRequestBaseUrlProvider()
        {
            sut.UseAbsoluteUrlHref(Mock.Of<IHttpContextAccessor>());

            sut.BaseUrlProvider.Should().BeOfType<CurrentRequestBaseUrlProvider>();
        }

        [Fact]
        public void UseAbsoluteUrlHrefShouldReturnSameInstanceOfOptions()
        {
            var result = sut.UseAbsoluteUrlHref(Mock.Of<IHttpContextAccessor>());

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseRelativeUrlHrefShouldThrowArgumentNullExceptionWhenOptionsIsNull()
        {
            Func<HypermediaServiceOptions> func = () => default(HypermediaServiceOptions).UseRelativeUrlHref();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("options");
        }

        [Fact]
        public void UseRelativeUrlHrefShouldSetBaseUrlProviderToEmptyBaseUrlProvider()
        {
            sut.UseRelativeUrlHref();

            sut.BaseUrlProvider.Should().BeOfType<EmptyBaseUrlProvider>();
        }

        [Fact]
        public void UseRelativeUrlHrefShouldReturnSameInstanceOfOptions()
        {
            var result = sut.UseRelativeUrlHref();

            result.Should().BeSameAs(sut);
        }

        [Fact]
        public void UseCustomUrlHrefShouldThrowArgumentNullExceptionWhenOptionsIsNull()
        {
            Func<HypermediaServiceOptions> func = () => default(HypermediaServiceOptions).UseCustomUrlHref(TestCustomUrl);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("options");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("some random text")]
        public void UseCustomUrlHrefShouldthrowArgumentExceptionWhenCustomUrlIs(string customUrl)
        {
            Func<HypermediaServiceOptions> func = () => sut.UseCustomUrlHref(customUrl);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'customUrl' must not be null or empty and must be a valid URL.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void UseCustomUrlHrefShouldSetBaseUrlProviderToCustomBaseUrlProvider()
        {
            sut.UseCustomUrlHref(TestCustomUrl);

            sut.BaseUrlProvider.Should().BeOfType<CustomBaseUrlProvider>();
        }

        [Fact]
        public void UseCustomUrlHrefShouldReturnSameInstanceOfOptions()
        {
            var result = sut.UseCustomUrlHref(TestCustomUrl);

            result.Should().BeSameAs(sut);
        }
    }
}
