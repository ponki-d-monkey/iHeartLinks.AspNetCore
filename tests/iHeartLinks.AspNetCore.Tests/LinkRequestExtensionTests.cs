using System;
using System.Collections.Generic;
using FluentAssertions;
using iHeartLinks.Core;
using Xunit;

namespace iHeartLinks.AspNetCore.Tests
{
    public sealed class LinkRequestExtensionTests
    {
        private const string TestRouteName = "TestRouteName";

        [Fact]
        public void GetRouteNameShouldThrowArgumentNullExceptionWhenRequestIsNull()
        {
            Func<string> func = () => default(LinkRequest).GetRouteName();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void GetRouteNameShouldReturnRouteNameWhenRouteNameExists()
        {
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName);

            var result = request.GetRouteName();
            result.Should().Be(TestRouteName);
        }

        [Fact]
        public void GetRouteNameShouldReturnNullWhenRouteNameDoesNotExist()
        {
            // Valid use of LinkRequest ctor to simulate a request w/o a routeName value.
            var request = new LinkRequest(new Dictionary<string, object>
            {
                { "Filler", "filler" }
            });

            var result = request.GetRouteName();
            result.Should().BeNull();
        }

        [Fact]
        public void GetRouteValuesShouldThrowArgumentNullExceptionWhenRequestIsNull()
        {
            Func<object> func = () => default(LinkRequest).GetRouteValues();

            func.Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("request");
        }

        [Fact]
        public void GetRouteValuesShouldReturnRouteValuesWhenRouteValuesExists()
        {
            var routeValues = new object();
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName).SetRouteValuesIfNotNull(routeValues);

            var result = request.GetRouteValues();
            result.Should().Be(routeValues);
        }

        [Fact]
        public void GetRouteValuesShouldReturnNullWhenRouteValuesDoesNotExist()
        {
            LinkRequest request = LinkRequestBuilder.CreateWithRouteName(TestRouteName);

            var result = request.GetRouteValues();
            result.Should().BeNull();
        }
    }
}
