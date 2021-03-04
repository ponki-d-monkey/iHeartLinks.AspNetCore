using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkFactories
{
    public sealed class LinkMapperTests
    {
        private const string TestHref = "https://iheartlinks.example.com/person/1";
        private const string TestHttpMethod = "GET";
        private const string TemplatedKey = "templated";

        private readonly LinkFactoryContext context;
        private readonly HttpLink httpLink;
        private readonly LinkMapper<HttpLink> sut;

        public LinkMapperTests()
        {
            context = new LinkFactoryContext();
            httpLink = new HttpLink(TestHref, TestHttpMethod);

            sut = new LinkMapper<HttpLink>(context, httpLink);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Action action = () => new LinkMapper<HttpLink>(default, new HttpLink(TestHref, TestHttpMethod));

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenLinkIsNull()
        {
            Action action = () => new LinkMapper<HttpLink>(new LinkFactoryContext(), default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("link");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void MapIfExistingShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<LinkMapper<HttpLink>> func = () => sut.MapIfExisting<bool?>(key, (l, v) => l.Templated = v);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void MapIfExistingShouldThrowArgumentNullExceptionWhenMapperIsNull()
        {
            context.Set(TemplatedKey, new bool?(true));

            Func<LinkMapper<HttpLink>> func = () => sut.MapIfExisting<bool?>(TemplatedKey, default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("mapper");
        }

        [Fact]
        public void MapIfExistingShouldMapValueWhenExisting()
        {
            context.Set(TemplatedKey, new bool?(true));

            var result = sut.MapIfExisting<bool?>(TemplatedKey, (l, v) => l.Templated = v);
            result.Should().NotBeNull();
            result.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Templated.Should().Be(new bool?(true));
        }

        [Fact]
        public void MapIfExistingShouldNotValueWhenItDoesNotExist()
        {
            var result = sut.MapIfExisting<bool?>(TemplatedKey, (l, v) => l.Templated = v);
            result.Should().NotBeNull();
            result.Should().BeSameAs(sut);
            result.Link.Should().NotBeNull();
            result.Link.Templated.Should().BeNull();
        }
    }
}
