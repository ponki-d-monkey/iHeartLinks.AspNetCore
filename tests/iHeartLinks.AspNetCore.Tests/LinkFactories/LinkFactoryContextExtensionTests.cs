using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkFactories
{
    public sealed class LinkFactoryContextExtensionTests
    {
        private const string TestBaseUrl = "https://iheartlinks.example.com";
        private const string TestUrlPath = "/person/1";

        private readonly LinkFactoryContext sut;

        public LinkFactoryContextExtensionTests()
        {
            sut = new LinkFactoryContext();
        }

        [Fact]
        public void GetBaseUrlShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<string> func = () => default(LinkFactoryContext).GetBaseUrl();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetBaseUrlShouldReturnBaseUrl()
        {
            sut.Set(LinkFactoryContext.BaseUrlKey, TestBaseUrl);

            var result = sut.GetBaseUrl();
            result.Should().Be(TestBaseUrl);
        }

        [Fact]
        public void GetBaseUrlShouldReturnNullWhenItDoesNotExist()
        {
            var result = sut.GetBaseUrl();
            result.Should().BeNull();
        }

        [Fact]
        public void GetUrlPathShoulThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<string> func = () => default(LinkFactoryContext).GetUrlPath();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetUrlPathShouldReturnUrlPath()
        {
            sut.Set(LinkFactoryContext.UrlPathKey, TestUrlPath);

            var result = sut.GetUrlPath();
            result.Should().Be(TestUrlPath);
        }

        [Fact]
        public void GetUrlPathShouldReturnNullWhenItDoesNotExist()
        {
            var result = sut.GetUrlPath();
            result.Should().BeNull();
        }

        [Fact]
        public void GetHrefShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<string> func = () => default(LinkFactoryContext).GetHref();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetHrefShouldReturnHref()
        {
            sut.Set(LinkFactoryContext.BaseUrlKey, TestBaseUrl);
            sut.Set(LinkFactoryContext.UrlPathKey, TestUrlPath);

            var result = sut.GetHref();
            result.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }

        [Fact]
        public void GetHrefShouldReturnHrefWhenBaseUrlDoesNotExist()
        {
            sut.Set(LinkFactoryContext.UrlPathKey, TestUrlPath);

            var result = sut.GetHref();
            result.Should().Be(TestUrlPath);
        }

        [Fact]
        public void GetHrefShouldReturnHrefWhenUrlPathDoesNotExist()
        {
            sut.Set(LinkFactoryContext.BaseUrlKey, TestBaseUrl);

            var result = sut.GetHref();
            result.Should().Be(TestBaseUrl);
        }

        [Fact]
        public void GetHrefShouldReturnNullWhenBaseUrlAndUrlPathDoNotExist()
        {
            var result = sut.GetHref();
            result.Should().BeNull();
        }

        [Fact]
        public void SetBaseUrlShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkFactoryContext> func = () => default(LinkFactoryContext).SetBaseUrl(TestBaseUrl);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void SetBaseUrlShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            Func<LinkFactoryContext> func = () => sut.SetBaseUrl(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("value");
        }

        [Fact]
        public void SetBaseUrlShouldAddBaseUrl()
        {
            sut.SetBaseUrl(TestBaseUrl);

            var result = sut.GetBaseUrl();
            result.Should().Be(TestBaseUrl);
        }

        [Fact]
        public void SetBaseUrlShouldReturnSameInstanceOfContext()
        {
            var result = sut.SetBaseUrl(TestBaseUrl);

            result.Should().Be(sut);
        }

        [Fact]
        public void SetUrlPathShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkFactoryContext> func = () => default(LinkFactoryContext).SetUrlPath(TestUrlPath);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void SetUrlPathShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            Func<LinkFactoryContext> func = () => sut.SetUrlPath(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("value");
        }

        [Fact]
        public void SetUrlPathShouldAddUrlPath()
        {
            sut.SetUrlPath(TestUrlPath);

            var result = sut.GetUrlPath();
            result.Should().Be(TestUrlPath);
        }

        [Fact]
        public void SetUrlPathShouldReturnSameInstanceOfContext()
        {
            var result = sut.SetUrlPath(TestUrlPath);

            result.Should().Be(sut);
        }

        [Fact]
        public void MapToShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkMapper<Link>> func = () => default(LinkFactoryContext).MapTo(h => new Link(h));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void MapToShouldThrowArgumentNullExceptionWhenCreateHandlerIsNull()
        {
            Func<LinkMapper<Link>> func = () => sut.MapTo(default(Func<string, Link>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("createHandler");
        }

        [Fact]
        public void MapToShouldReturnLinkMapper()
        {
            sut.Set(LinkFactoryContext.BaseUrlKey, TestBaseUrl);
            sut.Set(LinkFactoryContext.UrlPathKey, TestUrlPath);

            var result = sut.MapTo(h => new Link(h));

            result.Should().NotBeNull();
            result.Context.Should().NotBeNull();
            result.Context.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Href.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }

        [Fact]
        public void MapToWithContextShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkMapper<Link>> func = () => default(LinkFactoryContext).MapTo((h, c) => new Link(h));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void MapToWithContextShouldThrowArgumentNullExceptionWhenCreateHandlerIsNull()
        {
            Func<LinkMapper<Link>> func = () => sut.MapTo(default(Func<string, LinkFactoryContext, Link>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("createHandler");
        }

        [Fact]
        public void MapToWithContextShouldReturnLinkMapper()
        {
            sut.Set(LinkFactoryContext.BaseUrlKey, TestBaseUrl);
            sut.Set(LinkFactoryContext.UrlPathKey, TestUrlPath);

            var result = sut.MapTo((h, c) => new Link(h));

            result.Should().NotBeNull();
            result.Context.Should().NotBeNull();
            result.Context.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Href.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }
    }
}
