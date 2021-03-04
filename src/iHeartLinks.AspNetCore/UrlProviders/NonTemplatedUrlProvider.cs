using System;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.UrlProviders
{
    public sealed class NonTemplatedUrlProvider : IUrlProvider
    {
        private readonly Lazy<IUrlHelper> urlHelper;

        public NonTemplatedUrlProvider(IUrlHelperBuilder urlHelperBuilder)
        {
            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = new Lazy<IUrlHelper>(() => urlHelperBuilder.Build());
        }

        public Uri Provide(UrlProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var id = context.LinkRequest.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"Parameter '{nameof(context)}.{nameof(context.LinkRequest)}' must contain a value for '{LinkRequest.IdKey}'.");
            }

            var url = context.Args == null ? 
                urlHelper.Value.RouteUrl(id) : 
                urlHelper.Value.RouteUrl(id, context.Args);

            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                throw new InvalidOperationException($"The given '{LinkRequest.IdKey}' to retrieve the URL did not provide a valid value. Value of '{LinkRequest.IdKey}': {id}");
            }

            return new Uri(url, UriKind.RelativeOrAbsolute);
        }
    }
}
