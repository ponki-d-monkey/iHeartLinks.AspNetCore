using iHeartLinks.AspNetCore.BaseUrlProviders;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaServiceOptions
    {
        public IBaseUrlProvider BaseUrlProvider { get; private set; }

        public void SetBaseUrlProvider(IBaseUrlProvider baseUrlProvider)
        {
            BaseUrlProvider = baseUrlProvider;
        }
    }
}
