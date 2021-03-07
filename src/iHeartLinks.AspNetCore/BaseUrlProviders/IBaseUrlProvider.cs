using System;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public interface IBaseUrlProvider
    {
        Uri Provide();
    }
}
