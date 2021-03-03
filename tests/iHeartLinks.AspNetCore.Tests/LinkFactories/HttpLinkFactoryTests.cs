﻿using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkFactories
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
            context.Set(LinkFactoryContext.BaseUrlKey, baseUrl);
            context.Set(LinkFactoryContext.UrlPathKey, urlPath);
            context.Set("httpMethod", "GET");
            context.Set("templated", true);

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
            context.Set(LinkFactoryContext.BaseUrlKey, baseUrl);
            context.Set(LinkFactoryContext.UrlPathKey, urlPath);
            context.Set("httpMethod", "GET");

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
            context.Set(LinkFactoryContext.BaseUrlKey, baseUrl);
            context.Set(LinkFactoryContext.UrlPathKey, urlPath);
            context.Set("httpMethod", "GET");
            context.Set("templated", false);

            var result = sut.Create(context);
            var httpLink = result.As<HttpLink>();
            httpLink.Templated.Should().BeNull();
        }
    }
}
