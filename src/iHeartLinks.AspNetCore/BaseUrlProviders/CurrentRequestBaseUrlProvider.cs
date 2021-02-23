using System;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProvider : IBaseUrlProvider
    {
        private readonly IUrlHelper urlHelper;

        public CurrentRequestBaseUrlProvider(IUrlHelperBuilder urlHelperBuilder)
        {
            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = urlHelperBuilder.Build();
        }

        public string GetBaseUrl()
        {
            var request = urlHelper.ActionContext.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.ToUriComponent()}";
        }
    }
}
