using System;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.UrlPathProviders
{
    public sealed class NonTemplatedUrlPathProvider : IUrlPathProvider
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

        public Uri Provide(UrlPathProviderContext context)
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
