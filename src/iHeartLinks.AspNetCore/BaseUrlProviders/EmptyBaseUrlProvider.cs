namespace iHeartLinks.AspNetCore.BaseUrlProviders
{
    public sealed class EmptyBaseUrlProvider : IBaseUrlProvider
    {
        public string Provide()
        {
            return string.Empty;
        }
    }
}
