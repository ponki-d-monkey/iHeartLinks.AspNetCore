using iHeartLinks.AspNetCore.BaseUrlProviders;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaServiceOptions
    {
        public IBaseUrlProvider BaseUrlProvider { get; set; }
    }
}
