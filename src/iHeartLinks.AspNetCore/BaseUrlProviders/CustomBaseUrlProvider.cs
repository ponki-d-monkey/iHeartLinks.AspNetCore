using System;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public class CustomBaseUrlProvider : IBaseUrlProvider
    {
        private readonly Uri baseUrl;

        public CustomBaseUrlProvider(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out this.baseUrl))
            {
                throw new ArgumentException($"Parameter '{nameof(baseUrl)}' must not be null or empty and must be a valid base URL.");
            }
        }

        public Uri Provide()
        {
            return baseUrl;
        }
    }
}
