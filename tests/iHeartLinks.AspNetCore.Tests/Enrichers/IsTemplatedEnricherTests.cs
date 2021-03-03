using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Enrichers
{
    public sealed class IsTemplatedEnricherTests
    {
        private const string TemplatedKey = "templated";

        private readonly IDictionary<string, string> keyParts;
        private readonly LinkKey linkKey;

        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter writer;

        private readonly IsTemplatedEnricher sut;

        public IsTemplatedEnricherTests()
        {
            keyParts = new Dictionary<string, string>
            {
                { LinkKey.IdKey, "TestId" }
            };

            linkKey = new LinkKey(keyParts);

            context = new LinkFactoryContext();
            writer = new LinkDataWriter(context);

            sut = new IsTemplatedEnricher();
        }

        [Fact]
        public void EnrichShouldThrowArugmentNullExceptionWhenLinkKeyIsNull()
        {
            Action action = () => sut.Enrich(default, writer);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("linkKey");
        }

        [Fact]
        public void EnrichShouldThrowArgumentNullExceptionWhenWriterIsNull()
        {
            Action action = () => sut.Enrich(linkKey, default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("writer");
        }

        [Fact]
        public void EnrichShouldWriteTemplatedValueWhenLinkKeyHasTemplatedValueTrue()
        {
            keyParts.Add(TemplatedKey, bool.TrueString.ToLower());

            sut.Enrich(linkKey, writer);

            var result = context.Get(TemplatedKey);
            result.Should().Be(true);
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkKeyDoesNotHaveTemplatedValue()
        {
            sut.Enrich(linkKey, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkKeyHasTemplatedValueNotAValidBoolean()
        {
            keyParts.Add(TemplatedKey, "random string");

            sut.Enrich(linkKey, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }

        [Fact]
        public void EnrichShouldNotWriteTemplatedValueWhenLinkKeyHasTemplatedValueFalse()
        {
            keyParts.Add(TemplatedKey, bool.FalseString.ToLower());

            sut.Enrich(linkKey, writer);

            var result = context.Get(TemplatedKey);
            result.Should().BeNull();
        }
    }
}
