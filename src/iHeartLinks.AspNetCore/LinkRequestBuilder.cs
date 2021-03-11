using System;
using System.Collections.Generic;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore
{
    public sealed class LinkRequestBuilder
    {
        public const string RouteNameKey = "routeName";
        public const string RouteValuesKey = "routeValues";

        private readonly IDictionary<string, object> requestParameters;

        private LinkRequestBuilder(IDictionary<string, object> requestParameters)
        {
            this.requestParameters = requestParameters;
        }

        public static LinkRequestBuilder CreateWithRouteName(string routeName)
        {
            if (string.IsNullOrWhiteSpace(routeName))
            {
                throw new ArgumentException($"Parameter '{nameof(routeName)}' must not be null or empty.");
            }

            return new LinkRequestBuilder(new Dictionary<string, object>
            {
                { RouteNameKey, routeName }
            });
        }

        public LinkRequestBuilder SetRouteValuesIfNotNull(object routeValues)
        {
            return DoSetIfNotNull(RouteValuesKey, routeValues);
        }

        public LinkRequestBuilder SetIfNotNull(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            return DoSetIfNotNull(key, value);
        }

        public LinkRequestBuilder Set(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return DoSet(key, value);
        }

        public LinkRequest Build()
        {
            return new LinkRequest(this.requestParameters);
        }

        private LinkRequestBuilder DoSetIfNotNull(string key, object value)
        {
            if (value != null)
            {
                return DoSet(key, value);
            }

            return this;
        }

        private LinkRequestBuilder DoSet(string key, object value)
        {
            requestParameters[key] = value;

            return this;
        }

        public static implicit operator LinkRequest(LinkRequestBuilder builder)
        {
            return builder.Build();
        }
    }
}
