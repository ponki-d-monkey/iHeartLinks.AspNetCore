using System;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.UrlPathProviders
{
    public class NonTemplatedUrlPathProvider : IUrlPathProvider
    {
        private readonly Lazy<IUrlHelper> urlHelper;

        public NonTemplatedUrlPathProvider(IUrlHelperBuilder urlHelperBuilder)
        {
            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = new Lazy<IUrlHelper>(() => urlHelperBuilder.Build());
        }

        public Uri Provide(LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var routeName = request.GetRouteName();
            if (string.IsNullOrWhiteSpace(routeName))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must contain a '{LinkRequestBuilder.RouteNameKey}' value.");
            }

            var routeValues = request.GetRouteValues();
            var url = routeValues == null ?
                urlHelper.Value.RouteUrl(routeName) :
                urlHelper.Value.RouteUrl(routeName, routeValues);

            url = FormatUrlPath(url);
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                throw new InvalidOperationException($"The given '{LinkRequestBuilder.RouteNameKey}' to retrieve the URL did not provide a valid value. Value of '{LinkRequestBuilder.RouteNameKey}': {routeName}");
            }

            return new Uri(url, UriKind.RelativeOrAbsolute);
        }

        protected virtual string FormatUrlPath(string url)
        {
            return url;
        }
    }
}
