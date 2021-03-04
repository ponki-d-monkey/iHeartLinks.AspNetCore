using System;
using FluentAssertions;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Enrichers
{
    public sealed class LinkDataWriterTests
    {
        private const string TestKey = "TestKey";
        private const string TestValue = "TesteValue";

        private readonly LinkFactoryContext context;
        private readonly LinkDataWriter sut;

        public LinkDataWriterTests()
        {
            context = new LinkFactoryContext();
            sut = new LinkDataWriter(context);
        }

        [Fact]
        public void CtorShouldThrowArgumentNullExceptionWhenContextIsNull()
        {
            Action action = () => new LinkDataWriter(default);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("context");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void WriteShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<LinkDataWriter> func = () => sut.Write(key, new object());

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void WriteShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            Func<LinkDataWriter> func = () => sut.Write("TestKey", default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(TestValue, typeof(string))]
        [InlineData("", typeof(string))]
        [InlineData(" ", typeof(string))]
        [InlineData(1, typeof(int))]
        [InlineData(2.1, typeof(double))]
        [InlineData(true, typeof(bool))]
        [InlineData('c', typeof(char))]
        public void WriteShouldWriteValueToUnderlyingContext(object value, Type typeOfValue)
        {
            sut.Write(TestKey, value);

            var result = context.Get(TestKey);
            result.Should().Be(value);
            result.Should().BeOfType(typeOfValue);
        }

        [Theory]
        [InlineData(TestValue, typeof(string))]
        [InlineData("", typeof(string))]
        [InlineData(" ", typeof(string))]
        [InlineData(1, typeof(int))]
        [InlineData(2.1, typeof(double))]
        [InlineData(true, typeof(bool))]
        [InlineData('c', typeof(char))]
        public void WriteShouldOverwriteValueWhenExisting(object value, Type typeOfValue)
        {
            context.Set(TestKey, new object());

            sut.Write(TestKey, value);

            var result = context.Get(TestKey);
            result.Should().Be(value);
            result.Should().BeOfType(typeOfValue);
        }

        [Fact]
        public void WriteShouldReturnSameInstanceOfLinkDataWriter()
        {
            var result = sut.Write(TestKey, TestValue);

            result.Should().BeSameAs(sut);
        }
    }
}
