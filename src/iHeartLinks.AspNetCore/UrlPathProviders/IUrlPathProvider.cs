using System;

namespace iHeartLinks.AspNetCore.UrlPathProviders
{
    public interface IUrlPathProvider
    {
        Uri Provide(UrlPathProviderContext context);
    }
}
