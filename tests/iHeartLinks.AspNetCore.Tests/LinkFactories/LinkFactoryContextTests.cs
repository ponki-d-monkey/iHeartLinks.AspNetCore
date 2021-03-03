using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.AspNetCore.LinkFactories;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.LinkFactories
{
    public sealed class LinkFactoryContextTests
    {
        private const string TestKey = "TestKey";
        private const string TestValue = "TestValue";

        private readonly LinkFactoryContext sut;

        public LinkFactoryContextTests()
        {
            sut = new LinkFactoryContext();
        }

        public static IEnumerable<object[]> NullValues = new List<object[]>
        {
            new object[] { default(object) },
            new object[] { default(bool?) },
            new object[] { default(int?) },
            new object[] { default(float?) },
            new object[] { default(char?) },
            new object[] { default(Guid?) },
        };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<object> func = () => sut.Get(key);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void GetShouldReturnNullWhenKeyDoesNotExist()
        {
            var result = sut.Get(TestKey);

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(TestValue, typeof(string))]
        [InlineData("", typeof(string))]
        [InlineData(" ", typeof(string))]
        [InlineData(1, typeof(int))]
        [InlineData(2.1, typeof(double))]
        [InlineData(true, typeof(bool))]
        [InlineData('c', typeof(char))]
        public void GetShouldReturnValue(object value, Type typeOfValue)
        {
            // Use Set() to add value to collection
            sut.Set(TestKey, value);

            var result = sut.Get(TestKey);
            result.Should().NotBeNull();
            result.Should().BeOfType(typeOfValue);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void SetShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Action action = () => sut.Set(key, new object());

            var exception = action.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(NullValues))]
        public void SetShouldThrowArgumentNullExceptionWhenValueIs(object value)
        {
            Action action = () => sut.Set(TestKey, value);

            action.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("value");
        }

        [Theory]
        [InlineData(TestValue)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(1)]
        [InlineData(2.1)]
        [InlineData(true)]
        [InlineData('c')]
        public void SetShouldAddValueToUnderlyingCollection(object value)
        {
            sut.Set(TestKey, value);

            // Use Get() to test if value is present
            var result = sut.Get(TestKey);
            result.Should().NotBeNull();
            result.Should().Be(value);
        }

        [Theory]
        [InlineData("TestValue", "TestValueOverride")]
        [InlineData("TestValue", true)]
        public void SetShouldOverrideExistingValue(object value, object overrideValue)
        {
            sut.Set(TestKey, value);
            sut.Set(TestKey, overrideValue);

            // Use Get() to test if value is present
            var result = sut.Get(TestKey);
            result.Should().NotBeNull();
            result.Should().Be(overrideValue);
        }
    }
}
