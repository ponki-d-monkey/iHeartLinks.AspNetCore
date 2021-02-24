using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using Microsoft.AspNetCore.Http;

namespace iHeartLinks.AspNetCore
{
    public static class HypermediaServiceOptionsExtension
    {
        public static HypermediaServiceOptions UseAbsoluteUrlHref(this HypermediaServiceOptions options, IHttpContextAccessor httpContextAccessor)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.BaseUrlProvider = new CurrentRequestBaseUrlProvider(httpContextAccessor);

            return options;
        }

        public static HypermediaServiceOptions UseRelativeUrlHref(this HypermediaServiceOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.BaseUrlProvider = new EmptyBaseUrlProvider();

            return options;
        }

        public static HypermediaServiceOptions UseCustomUrlHref(this HypermediaServiceOptions options, string customUrl)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.BaseUrlProvider = new CustomBaseUrlProvider(customUrl);

            return options;
        }
    }
}
