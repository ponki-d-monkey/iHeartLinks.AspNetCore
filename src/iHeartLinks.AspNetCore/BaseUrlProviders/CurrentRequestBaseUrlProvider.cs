using System;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProvider : IBaseUrlProvider
    {
        private readonly IUrlHelper urlHelper;

        public CurrentRequestBaseUrlProvider(IUrlHelper urlHelper)
        {
            this.urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
        }

        public string GetBaseUrl()
        {
            var request = urlHelper.ActionContext.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.ToUriComponent()}";
        }
    }
}
