using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore
{
    public static class LinkRequestExtension
    {
        public static string GetRouteName(this LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.GetValueOrDefault<string>(LinkRequestBuilder.RouteNameKey);
        }

        public static object GetRouteValues(this LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.GetValueOrDefault(LinkRequestBuilder.RouteValuesKey);
        }
    }
}
