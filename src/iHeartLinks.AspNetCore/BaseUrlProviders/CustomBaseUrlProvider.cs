using System;

namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class CustomBaseUrlProvider : IBaseUrlProvider
    {
        private readonly string customUrl;

        public CustomBaseUrlProvider(string customUrl)
        {
            if (string.IsNullOrWhiteSpace(customUrl) || 
                !Uri.IsWellFormedUriString(customUrl, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException($"Parameter '{nameof(customUrl)}' must not be null or empty and must be a valid URL.");
            }

            this.customUrl = customUrl;
        }

        public string GetBaseUrl()
        {
            return customUrl;
        }
    }
}
