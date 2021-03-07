using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class HttpLinkFactoryTests
    {
        private readonly HttpLinkFactory sut;

        public HttpLinkFactoryTests()
        {
            sut = new HttpLinkFactory();
        }

        [Fact]
        public void CreateShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Func<Link> func = () => sut.Create(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void CreateShouldReturnHttpLinkWithAllPropertiesWithValue()
        {
            var baseUrl = "https://iheartlinks.example.com";
            var urlPath = "/person/1";

            var context = new LinkFactoryContext();
            context.SetBaseUrl(new Uri(baseUrl, UriKind.Absolute));
            context.SetUrlPath(new Uri(urlPath, UriKind.Relative));
            context.Set(HttpMethodEnricher.HttpMethodKey, "GET");
            context.Set(IsTemplatedEnricher.TemplatedKey, true);

            var result = sut.Create(context);
            result.Should().NotBeNull();
            result.Should().BeOfType<HttpLink>();
            result.Should().BeAssignableTo<Link>();

            result.Href.Should().Be($"{baseUrl}{urlPath}");

            var httpLink = result.As<HttpLink>();
            httpLink.Method.Should().Be("GET");
            httpLink.Templated.Should().BeTrue();
        }

        [Fact]
        public void CreateShouldReturnHttpLinkWithNullTemplatedPropertyWhenValueDoesNotExist()
        {
            var baseUrl = "https://iheartlinks.example.com";
            var urlPath = "/person/1";

            var context = new LinkFactoryContext();
            context.SetBaseUrl(new Uri(baseUrl, UriKind.Absolute));
            context.SetUrlPath(new Uri(urlPath, UriKind.Relative));
            context.Set(HttpMethodEnricher.HttpMethodKey, "GET");

            var result = sut.Create(context);
            var httpLink = result.As<HttpLink>();
            httpLink.Templated.Should().BeNull();
        }

        [Fact]
        public void CreateShouldReturnHttpLinkWithNullTemplatedPropertyWhenValueIsNotTrue()
        {
            var baseUrl = "https://iheartlinks.example.com";
            var urlPath = "/person/1";

            var context = new LinkFactoryContext();
            context.SetBaseUrl(new Uri(baseUrl, UriKind.Absolute));
            context.SetUrlPath(new Uri(urlPath, UriKind.Relative));
            context.Set(HttpMethodEnricher.HttpMethodKey, "GET");
            context.Set(IsTemplatedEnricher.TemplatedKey, false);

            var result = sut.Create(context);
            var httpLink = result.As<HttpLink>();
            httpLink.Templated.Should().BeNull();
        }
    }
}
