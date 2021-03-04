using System;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public class CustomBaseUrlProvider : IBaseUrlProvider
    {
        private readonly string baseUrl;

        public CustomBaseUrlProvider(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || 
                !Uri.IsWellFormedUriString(baseUrl, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException($"Parameter '{nameof(baseUrl)}' must not be null or empty and must be a valid URL.");
            }

            this.baseUrl = baseUrl;
        }

        public string Provide()
        {
            return baseUrl;
        }
    }
}
