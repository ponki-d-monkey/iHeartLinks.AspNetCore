using System;
using Microsoft.AspNetCore.Http;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProvider : IBaseUrlProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentRequestBaseUrlProvider(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
            {
                throw new ArgumentNullException(nameof(httpContextAccessor));
            }

            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            var request = httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.ToUriComponent()}";
        }
    }
}
