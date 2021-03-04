using System;
using iHeartLinks.AspNetCore.LinkRequestProcessors;

namespace iHeartLinks.AspNetCore.UrlProviders
{
    public sealed class UrlProviderContext
    {
        public UrlProviderContext(LinkRequest linkRequest)
        {
            LinkRequest = linkRequest ?? throw new ArgumentNullException(nameof(linkRequest));
        }

        public LinkRequest LinkRequest { get; private set; }

        public object Args { get; set; }
    }
}
