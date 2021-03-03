using System;

namespace iHeartLinks.AspNetCore.UrlProviders
{
    public interface IUrlProvider
    {
        Uri Provide(UrlProviderContext context);
    }
}
