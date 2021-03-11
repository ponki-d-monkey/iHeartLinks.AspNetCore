using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.Extensions;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class IsTemplatedEnricherTests
    {
        private const string TestRouteName = "TestRouteName";

        private readonly LinkRequest request;
        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter writer;

        private readonly IsTemplatedEnricher sut;

        public IsTemplatedEnricherTests()
        {
            request = LinkRequestBuilder.CreateWithRouteName(TestRouteName);
            context = new LinkFactoryContext();
            writer = new LinkDataWriter(context);

            sut = new IsTemplatedEnricher();
        }

        [Fact]
        public void EnrichShouldThrowArugmentNullExceptionWhenRequestIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(request, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteTemplatedValueWhenRequestHasTemplatedValueTrue()
        {
            sut.Enrich(LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetIsTemplated(), writer);

            var result = context.Get(IsTemplatedEnricher.TemplatedKey);
            result.Should().Be(true);
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenRequestDoesNotHaveTemplatedValue()
        {
            sut.Enrich(request, writer);

            var result = context.Get(IsTemplatedEnricher.TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenRequestHasTemplatedValueNotAValidBoolean()
        {
            sut.Enrich(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .Set(IsTemplatedEnricher.TemplatedKey, "yes"), writer);

            var result = context.Get(IsTemplatedEnricher.TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenRequestHasTemplatedValueFalse()
        {
            sut.Enrich(LinkRequestBuilder
                .CreateWithRouteName(TestRouteName)
                .Set(IsTemplatedEnricher.TemplatedKey, false), writer);

            var result = context.Get(IsTemplatedEnricher.TemplatedKey);
            result.Should().BeNull();
        }
    }
}
