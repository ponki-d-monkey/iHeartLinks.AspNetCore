using System;
using iHeartLinks.AspNetCore.LinkKeyProcessors;

namespace iHeartLinks.AspNetCore.UrlProviders
{
    public class UrlProviderContext
    {
        public UrlProviderContext(LinkKey linkKey)
        {
            LinkKey = linkKey ?? throw new ArgumentNullException(nameof(linkKey));
        }

        public LinkKey LinkKey { get; private set; }

        public object Args { get; set; }
    }
}
