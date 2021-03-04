using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkRequestProcessors
{
    public sealed class PipeDelimitedLinkRequestProcessorTests
    {
        private readonly PipeDelimitedLinkRequestProcessor sut;

        public PipeDelimitedLinkRequestProcessorTests()
        {
            sut = new PipeDelimitedLinkRequestProcessor();
        }

        public static IEnumerable<object[]> LinkRequestsWithId = new List<object[]>
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
        public void ProcessShouldThrowArgumentExceptionWhenRequestIsNullOrEmpty(string request)
        {
            Func<LinkRequest> func = () => sut.Process(request);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'request' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(LinkRequestsWithId))]
        public void ProcessShouldReturnLinkRequestWithIdWhenKeyIs(string request, string expectedIdValue)
        {
            var result = sut.Process(request);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedIdValue);
        }

        [Theory]
        [InlineData("Id1|Id1")]
        [InlineData("Id1|Id2")]
        [InlineData("Id1|id=Id1")]
        [InlineData("Id1|id=Id2")]
        public void ProcessShouldThrowArgumentExceptionWhenRequestHasMoreThan1IdValue(string request)
        {
            Func<LinkRequest> func = () => sut.Process(request);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("Multiple values found for 'id'. A keyless value is treated as 'id'. If it is present, there is no need to supply a value with 'id' key explicitly.");
        }

        [Theory]
        [InlineData("key=value=pair")]
        [InlineData("key=value=pair=invalid")]
        [InlineData("Hello|key=value=pair")]
        [InlineData("id=Hello|key=value=pair")]
        public void ProcessShouldThrowArgumentExceptionWhenAValueOfASplitRequestHasMoreThan2Parts(string request)
        {
            Func<LinkRequest> func = () => sut.Process(request);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("A key-value pair delimited by an equal sign produced by splitting the request must contain 2 parts at the most.");
        }

        [Theory]
        [InlineData("key=Hello|key=World")]
        [InlineData("key=Hello|Id1|key=World")]
        public void ProcessShouldThrowArgumentExceptionWhenDuplicateKeysExistInASplitRequest(string request)
        {
            Func<LinkRequest> func = () => sut.Process(request);

            func.Should().Throw<ArgumentException>().Which.Message.Should().Be("A key-value pair delimited by an equal sign produced by splitting the request must have a unique key.");
        }

        [Theory]
        [InlineData("Hello", 1)]
        [InlineData("id=Hello", 1)]
        [InlineData("Hello|", 1)]
        [InlineData("Hello|key=World", 2)]
        [InlineData("id=Hello|key=World", 2)]
        [InlineData("name=Hello|key=World", 2)]
        [InlineData("name=Hello|key=World||", 2)]
        public void ProcessShouldReturnLinkRequestWithCorrectCount(string request, int expectedCount)
        {
            var result = sut.Process(request);

            result.Should().NotBeNull();
            result.Parts.Should().HaveCount(expectedCount);
        }

        [Theory]
        [InlineData("Hello|key=value")]
        [InlineData(" Hello | key = value ")]
        [InlineData("id=Hello|key=value")]
        [InlineData("id = Hello | key = value ")]
        public void ProcessShouldReturnLinkRequestWithCorrectKeyValuePairs(string request)
        {
            var result = sut.Process(request);

            result.Should().NotBeNull();
            result.Id.Should().Be("Hello");
            result.Parts.ContainsKey("id").Should().BeTrue();
            result.Parts.ContainsKey("key").Should().BeTrue();
            result.Parts["key"].Should().Be("value");
        }
    }
}
