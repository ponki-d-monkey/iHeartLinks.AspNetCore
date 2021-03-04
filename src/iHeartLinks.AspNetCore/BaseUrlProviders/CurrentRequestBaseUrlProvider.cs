using System;
using Microsoft.AspNetCore.Http;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class CurrentRequestBaseUrlProvider : IBaseUrlProvider
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentRequestBaseUrlProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string Provide()
        {
            var request = httpContextAccessor.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.ToUriComponent()}";
        }
    }
}
