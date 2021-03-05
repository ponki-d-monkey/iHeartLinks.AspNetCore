using System;
using System.Collections.Generic;
using System.ComponentModel;
using FluentAssertions;
using iHeartLinks.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests.Extensions
{
    public sealed class QueryNameSelectorTests
    {
        private readonly QueryNameSelector sut;

        public QueryNameSelectorTests()
        {
            sut = new QueryNameSelector();
        }

        [Fact]
        public void SelectShouldThrowArgumentNullExceptionWhenParameterPropertiesIsNull()
        {
            Func<IEnumerable<string>> func = () => sut.Select(default);

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("parameterProperties");
        }

        [Fact]
        public void SelectShouldReturnNamesOfProperties()
        {
            var result = sut.Select(typeof(TestQuery).GetProperties());

            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Filter");
            result.Should().Contain("VersionNumber");
        }

        [Fact]
        public void SelectShouldReturnNamesOfFromQueryAttributeAppliedToProperties()
        {
            var result = sut.Select(typeof(TestQueryWithFromQueryAttribute).GetProperties());

            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("f");
            result.Should().Contain("v_num");
        }

        [Fact]
        public void SelectShouldReturnNamesOfPropertiesOrFromQueryAttributeAppliedToPropeties()
        {
            var result = sut.Select(typeof(TestQueryWithMixedPropertyNames).GetProperties());

            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("Filter");
            result.Should().Contain("v_num");
        }

        [Fact]
        public void SelectShouldReturnNamesOfFromQueryAttributeAppliedToPropertiesWithMoreThan1Attribute()
        {
            var result = sut.Select(typeof(TestQueryWithMoreThan1Attribute).GetProperties());

            result.Should().NotBeNullOrEmpty();
            result.Should().Contain("f");
            result.Should().Contain("v_num");
        }

        public class TestQuery
        {
            public string Filter { get; set; }

            public string VersionNumber { get; set; }
        }

        public class TestQueryWithFromQueryAttribute
        {
            [FromQuery(Name = "f")]
            public string Filter { get; set; }

            [FromQuery(Name = "v_num")]
            public string VersionNumber { get; set; }
        }

        public class TestQueryWithMixedPropertyNames
        {
            public string Filter { get; set; }

            [FromQuery(Name = "v_num")]
            public string VersionNumber { get; set; }
        }

        public class TestQueryWithMoreThan1Attribute
        {
            [Description("Filter parameter")]
            [FromQuery(Name = "f")]
            public string Filter { get; set; }

            [FromQuery(Name = "v_num")]
            [Description("Version Number parameter")]
            public string VersionNumber { get; set; }
        }
    }
}
