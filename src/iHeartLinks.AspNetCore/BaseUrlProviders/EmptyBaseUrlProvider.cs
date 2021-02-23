namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class EmptyBaseUrlProvider : IBaseUrlProvider
    {
        public string GetBaseUrl()
        {
            return string.Empty;
        }
    }
}
