using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkKeyProcessors
{
    public sealed class LinkKeyTests
    {
        private const string TestIdValue = "TestIdValue";

        public static IEnumerable<object[]> NullOrEmptyParts => new List<object[]>
        {
            new[] { default(Dictionary<string, string>) },
            new[] { new Dictionary<string, string>() }
        };

        [Theory]
        [MemberData(nameof(NullOrEmptyParts))]
        public void CtorShouldThrowArgumentExceptionWhenPartsIs(IDictionary<string, string> parts)
        {
            Action action = () => new LinkKey(parts);

            var exception = action.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'parts' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void IdShouldReturnIdValueFromPartsWhenCasingIsCorrect()
        {
            var sut = new LinkKey(new Dictionary<string, string>
            {
                { "id", TestIdValue }
            });

            sut.Id.Should().Be(TestIdValue);
        }

        [Fact]
        public void IdShouldNotReturnIdValueFromPartsWhenCasingIsIncorrect()
        {
            var sut = new LinkKey(new Dictionary<string, string>
            {
                { "ID", TestIdValue }
            });

            sut.Id.Should().BeNull();
        }

        [Fact]
        public void IdShouldNotReturnIdValueFromPartsWhenItDoesNotExist()
        {
            var sut = new LinkKey(new Dictionary<string, string>
            {
                { "key", TestIdValue }
            });

            sut.Id.Should().BeNull();
        }
    }
}
