using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkFactories
{
    public sealed class LinkFactoryTests
    {
        private readonly LinkFactory sut;

        public LinkFactoryTests()
        {
            sut = new LinkFactory();
        }

        [Fact]
        public void CreateShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<Link> func = () => sut.Create(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void CreateShouldReturnLinkWithCorrectHref()
        {
            var baseUrl = "https://iheartlinks.example.com";
            var urlPath = "/person/1";

            var context = new LinkFactoryContext();
            context.Set(LinkFactoryContext.BaseUrlKey, baseUrl);
            context.Set(LinkFactoryContext.UrlPathKey, urlPath);

            var result = sut.Create(context);
            result.Should().NotBeNull();
            result.Should().BeOfType<Link>();
            result.Href.Should().Be($"{baseUrl}{urlPath}");
        }
    }
}
