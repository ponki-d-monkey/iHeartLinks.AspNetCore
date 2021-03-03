using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkKeyProcessors
{
    public sealed class PipeDelimitedLinkKeyProcessorTests
    {
        private readonly PipeDelimitedLinkKeyProcessor sut;

        public PipeDelimitedLinkKeyProcessorTests()
        {
            sut = new PipeDelimitedLinkKeyProcessor();
        }

        public static IEnumerable<object[]> LinkKeysWithId = new List<object[]>
        {
            new[] { "Hello", "Hello" },
            new[] { "Hello ", "Hello" },
            new[] { " Hello", "Hello" },
            new[] { " Hello ", "Hello" },
            new[] { "Hello|", "Hello" },
            new[] { "Hello |", "Hello" },
            new[] { " Hello|", "Hello" },
            new[] { " Hello |", "Hello" },
            new[] { "Hello=", "Hello" },
            new[] { "id=Hello", "Hello" },
            new[] { "id = Hello", "Hello" },
            new[] { "id=Hello ", "Hello" },
            new[] { "id= Hello", "Hello" },
            new[] { "Id=World|Hello", "Hello" },
            new[] { "Id=World|id=Hello", "Hello" },
            new[] { "Hello|key=value", "Hello" },
            new[] { "id=Hello|key=value", "Hello" },
            new[] { "Hello=|key=value", "Hello" },
            new[] { "Hello;", "Hello;" },
            new[] { "Hello;World", "Hello;World" },
            new[] { "Hello World", "Hello World" },
        };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ProcessShouldThrowArgumentExceptionWhenKeyIsNullOrEmpty(string key)
        {
            Func<LinkKey> func = () => sut.Process(key);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(LinkKeysWithId))]
        public void ProcessShouldReturnLinkKeyWithIdWhenKeyIs(string key, string expectedIdValue)
        {
            var result = sut.Process(key);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedIdValue);
        }

        [Theory]
        [InlineData("Id1|Id1")]
        [InlineData("Id1|Id2")]
        [InlineData("Id1|id=Id1")]
        [InlineData("Id1|id=Id2")]
        public void ProcessShouldThrowArgumentExceptionWhenKeyHasMoreThan1IdValue(string key)
        {
            Func<LinkKey> func = () => sut.Process(key);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Multiple values found for 'id'. A keyless value is treated as 'id'. If it is present, there is no need to supply a value with 'id' key explicitly.");
        }

        [Theory]
        [InlineData("key=value=pair")]
        [InlineData("key=value=pair=invalid")]
        [InlineData("Hello|key=value=pair")]
        [InlineData("id=Hello|key=value=pair")]
        public void ProcessShouldThrowArgumentExceptionWhenAValueOfASplitKeyHasMoreThan2Parts(string key)
        {
            Func<LinkKey> func = () => sut.Process(key);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("A key-value pair delimited by an equal sign produced by splitting the key must contain 2 parts at the most.");
        }

        [Theory]
        [InlineData("key=Hello|key=World")]
        [InlineData("key=Hello|Id1|key=World")]
        public void ProcessShouldThrowArgumentExceptionWhenDuplicateKeysExist(string key)
        {
            Func<LinkKey> func = () => sut.Process(key);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("A key-value pair delimited by an equal sign produced by splitting the key must have a unique key.");
        }

        [Theory]
        [InlineData("Hello", 1)]
        [InlineData("id=Hello", 1)]
        [InlineData("Hello|", 1)]
        [InlineData("Hello|key=World", 2)]
        [InlineData("id=Hello|key=World", 2)]
        [InlineData("name=Hello|key=World", 2)]
        [InlineData("name=Hello|key=World||", 2)]
        public void ProcessShouldReturnLinkKeyWithCorrectCount(string key, int expectedCount)
        {
            var result = sut.Process(key);

            result.Should().NotBeNull();
            result.Parts.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData("Hello|key=value")]
        [InlineData(" Hello | key = value ")]
        [InlineData("id=Hello|key=value")]
        [InlineData("id = Hello | key = value ")]
        public void ProcessShouldReturnLinkKeyWithCorrectKeyValuePairs(string key)
        {
            var result = sut.Process(key);

            result.Should().NotBeNull();
            result.Id.Should().Be("Hello");
            result.Parts.ContainsKey("id").Should().BeTrue();
            result.Parts.ContainsKey("key").Should().BeTrue();
            result.Parts["key"].Should().Be("value");
        }
    }
}
