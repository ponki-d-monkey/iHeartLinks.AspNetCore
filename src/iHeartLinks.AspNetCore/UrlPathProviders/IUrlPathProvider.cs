using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.UrlPathProviders
{
    public interface IUrlPathProvider
    {
        Uri Provide(LinkRequest request);
    }
}
