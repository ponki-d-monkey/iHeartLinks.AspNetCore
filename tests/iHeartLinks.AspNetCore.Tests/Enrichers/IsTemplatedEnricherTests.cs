using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Enrichers
{
    public sealed class IsTemplatedEnricherTests
    {
        private const string TemplatedKey = "templated";

        private readonly IDictionary<string, string> keyParts;
        private readonly LinkRequest linkRequest;

        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter writer;

        private readonly IsTemplatedEnricher sut;

        public IsTemplatedEnricherTests()
        {
            keyParts = new Dictionary<string, string>
            {
                { LinkRequest.IdKey, "TestId" }
            };

            linkRequest = new LinkRequest(keyParts);

            context = new LinkFactoryContext();
            writer = new LinkDataWriter(context);

            sut = new IsTemplatedEnricher();
        }

        [Fact]
        public void EnrichShouldThrowArugmentNullExceptionWhenLinkRequestIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkRequest");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(linkRequest, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteTemplatedValueWhenLinkRequestHasTemplatedValueTrue()
        {
            keyParts.Add(TemplatedKey, bool.TrueString.ToLower());

            sut.Enrich(linkRequest, writer);

            var result = context.Get(TemplatedKey);
            result.Should().Be(true);
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkRequestDoesNotHaveTemplatedValue()
        {
            sut.Enrich(linkRequest, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkRequestHasTemplatedValueNotAValidBoolean()
        {
            keyParts.Add(TemplatedKey, "random string");

            sut.Enrich(linkRequest, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkRequestHasTemplatedValueFalse()
        {
            keyParts.Add(TemplatedKey, bool.FalseString.ToLower());

            sut.Enrich(linkRequest, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }
    }
}
