using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class LinkRequestBuilderTests
    {
        private const string TestRouteName = "TestRouteName";
        private const string TestKey = "TestKey";
        private const string TestValue = "TestValue";

        public static IEnumerable<object[]> TestInvalidKeyValuePairs = new List<object[]>
        {
            new[] { default(string), default(object) },
            new[] { string.Empty, default(object) },
            new[] { " ", default(object) },
            new[] { default(string), new object() },
            new[] { string.Empty, new object() },
            new[] { " ", new object() },
        };

        public static IEnumerable<object[]> TestValidKeyValuePairs = new List<object[]>
        {
            new[] { TestKey, default(object) },
            new[] { TestKey, TestValue },
        };

        public static IEnumerable<object[]> TestValues = new List<object[]>
        {
            new[] { default(object) },
            new[] { new object() },
        };

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void CreateWithRouteNameShouldThrowArgumentExceptionWhenRouteNameIs(string routeName)
        {
            Func<LinkRequestBuilder> func = () => LinkRequestBuilder.CreateWithRouteName(routeName);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'routeName' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void CreateWithRouteNameShouldReturnLinkRequestBuilderThatCanBuildALinkRequestWithRouteName()
        {
            var result = LinkRequestBuilder.CreateWithRouteName(TestRouteName);

            result.Should().NotBeNull();

            var request = result.Build();
            request.Should().NotBeNull();
            request.GetRouteName().Should().Be(TestRouteName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void SetShouldThrowArgumentExceptionWhenKeyIs(string key)
        {
            Func<LinkRequestBuilder> func = () => LinkRequestBuilder.CreateWithRouteName(TestRouteName).Set(key, new object());

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Fact]
        public void SetShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            Func<LinkRequestBuilder> func = () => LinkRequestBuilder.CreateWithRouteName(TestRouteName).Set(TestKey, default);

            func.Should().Throw<ArgumentException>().Which.ParamName.Should().Be("value");
        }

        [Fact]
        public void SetShouldReturnLinkRequestBuilderThatCanBuildALinkRequestWithValueSet()
        {
            var result = LinkRequestBuilder.CreateWithRouteName(TestRouteName).Set(TestKey, TestValue);

            result.Should().NotBeNull();

            var request = result.Build();
            request.Should().NotBeNull();
            request.GetValueOrDefault(TestKey).Should().Be(TestValue);
        }

        [Theory]
        [MemberData(nameof(TestInvalidKeyValuePairs))]
        public void SetIfNotNullShouldThrowArgumentExceptionWhenKeyIs(string key, object value)
        {
            Func<LinkRequestBuilder> func = () => LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetIfNotNull(key, value);

            var exception = func.Should().Throw<ArgumentException>().Which;
            exception.Message.Should().Be("Parameter 'key' must not be null or empty.");
            exception.ParamName.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(TestValidKeyValuePairs))]
        public void SetIfNotNullShouldReturnLinkRequestBuilderThatCanBuildALinkRequestWithExpectedValue(string key, object value)
        {
            var result = LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetIfNotNull(key, value);

            result.Should().NotBeNull();

            var request = result.Build();
            request.Should().NotBeNull();
            request.GetValueOrDefault(key).Should().Be(value);
        }

        [Theory]
        [MemberData(nameof(TestValues))]
        public void SetRouteValuesIfNotNullShouldReturnLinkRequestBuilderThatCanBuildALinkRequestWithExpectedValue(object value)
        {
            var result = LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetRouteValuesIfNotNull(value);

            result.Should().NotBeNull();

            var request = result.Build();
            request.Should().NotBeNull();
            request.GetRouteValues().Should().Be(value);
        }

        [Fact]
        public void LinkRequestBuilderShouldHaveImplicitConversionToLinkRequest()
        {
            typeof(LinkRequestBuilder).Should().HaveImplicitConversionOperator<LinkRequestBuilder, LinkRequest>();
        }
    }
}
