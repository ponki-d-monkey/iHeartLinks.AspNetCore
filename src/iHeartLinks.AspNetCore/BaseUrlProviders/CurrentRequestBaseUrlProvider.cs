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

        public Uri Provide()
        {
            var request = httpContextAccessor.HttpContext.Request;

            return new Uri($"{request.Scheme}://{request.Host.ToUriComponent()}", UriKind.Absolute);
        }
    }
}
