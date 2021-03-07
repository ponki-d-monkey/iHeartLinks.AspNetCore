using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaServiceBuilder
    {
        public HypermediaServiceBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }

        public HypermediaServiceBuilder UseAbsoluteUrlHref()
        {
            Services.AddHttpContextAccessor();
            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>());

            return this;
        }

        public HypermediaServiceBuilder UseRelativeUrlHref()
        {
            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider, EmptyBaseUrlProvider>());

            return this;
        }

        public HypermediaServiceBuilder UseCustomBaseUrlHref(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                throw new ArgumentException($"Parameter '{nameof(baseUrl)}' must not be null or empty and must be a valid URL.");
            }

            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider>(x => new CustomBaseUrlProvider(baseUrl)));

            return this;
        }

        public HypermediaServiceBuilder AddLinkEnricher<TEnricher>()
            where TEnricher : class, ILinkDataEnricher
        {
            Services.AddTransient<ILinkDataEnricher, TEnricher>();

            return this;
        }
    }
}
