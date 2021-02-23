using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;

namespace iHeartLinks.AspNetCore
{
    public static class HypermediaServiceOptionsExtension
    {
        public static HypermediaServiceOptions UseAbsoluteUrlHref(this HypermediaServiceOptions options, IUrlHelperBuilder urlHelperBuilder)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.BaseUrlProvider = new CurrentRequestBaseUrlProvider(urlHelperBuilder);

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
