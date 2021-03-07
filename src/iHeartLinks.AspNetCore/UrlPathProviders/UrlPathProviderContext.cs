using System;
using iHeartLinks.AspNetCore.LinkRequestProcessors;

namespace iHeartLinks.AspNetCore.UrlPathProviders
{
    public sealed class UrlPathProviderContext
    {
        public UrlPathProviderContext(LinkRequest linkRequest)
        {
            LinkRequest = linkRequest ?? throw new ArgumentNullException(nameof(linkRequest));
        }

        public LinkRequest LinkRequest { get; private set; }

        public object Args { get; set; }
    }
}
