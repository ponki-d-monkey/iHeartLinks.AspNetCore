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
            Func<Uri> func = () => default(LinkFactoryContext).GetBaseUrl();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetBaseUrlShouldReturnBaseUrl()
        {
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));

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
            Func<Uri> func = () => default(LinkFactoryContext).GetUrlPath();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetUrlPathShouldReturnUrlPath()
        {
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

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
            Func<Uri> func = () => default(LinkFactoryContext).GetHref();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void GetHrefShouldReturnHref()
        {
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            var result = sut.GetHref();
            result.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }

        [Fact]
        public void GetHrefShouldReturnHrefWhenBaseUrlDoesNotExist()
        {
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            var result = sut.GetHref();
            result.Should().Be(TestUrlPath);
        }

        [Fact]
        public void GetHrefShouldReturnHrefWhenUrlPathDoesNotExist()
        {
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));

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
            Func<LinkFactoryContext> func = () => default(LinkFactoryContext).SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));

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
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));

            var result = sut.GetBaseUrl();
            result.Should().Be(TestBaseUrl);
        }

        [Fact]
        public void SetBaseUrlShouldReturnSameInstanceOfContext()
        {
            var result = sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));

            result.Should().Be(sut);
        }

        [Fact]
        public void SetUrlPathShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkFactoryContext> func = () => default(LinkFactoryContext).SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

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
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            var result = sut.GetUrlPath();
            result.Should().Be(TestUrlPath);
        }

        [Fact]
        public void SetUrlPathShouldReturnSameInstanceOfContext()
        {
            var result = sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            result.Should().Be(sut);
        }

        [Fact]
        public void MapToShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkMapper<Link>> func = () => default(LinkFactoryContext).MapTo(h => new Link(h.ToString()));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void MapToShouldThrowArgumentNullExceptionWhenCreateHandlerIsNull()
        {
            Func<LinkMapper<Link>> func = () => sut.MapTo(default(Func<Uri, Link>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("createHandler");
        }

        [Fact]
        public void MapToShouldReturnLinkMapper()
        {
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            var result = sut.MapTo(h => new Link(h.ToString()));

            result.Should().NotBeNull();
            result.Context.Should().NotBeNull();
            result.Context.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Href.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }

        [Fact]
        public void MapToWithContextShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<LinkMapper<Link>> func = () => default(LinkFactoryContext).MapTo((h, c) => new Link(h.ToString()));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void MapToWithContextShouldThrowArgumentNullExceptionWhenCreateHandlerIsNull()
        {
            Func<LinkMapper<Link>> func = () => sut.MapTo(default(Func<Uri, LinkFactoryContext, Link>));

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("createHandler");
        }

        [Fact]
        public void MapToWithContextShouldReturnLinkMapper()
        {
            sut.SetBaseUrl(new Uri(TestBaseUrl, UriKind.Absolute));
            sut.SetUrlPath(new Uri(TestUrlPath, UriKind.Relative));

            var result = sut.MapTo((h, c) => new Link(h.ToString()));

            result.Should().NotBeNull();
            result.Context.Should().NotBeNull();
            result.Context.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Href.Should().Be($"{TestBaseUrl}{TestUrlPath}");
        }
    }
}
