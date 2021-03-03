using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace iHeartLinks.AspNetCore
{
    public sealed class HypermediaServiceBuilder
    {
        public HypermediaServiceBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        private IServiceCollection Services { get; }

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
            if (string.IsNullOrWhiteSpace(baseUrl) ||
                !Uri.IsWellFormedUriString(baseUrl, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException($"Parameter '{nameof(baseUrl)}' must not be null or empty and must be a valid URL.");
            }

            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider>(x => new CustomBaseUrlProvider(baseUrl)));

            return this;
        }

        public HypermediaServiceBuilder UseHttpLink()
        {
            AllowTemplatedHref();
            AddLinkEnricher<IsTemplatedEnricher>();
            AddLinkEnricher<HttpMethodEnricher>();

            Services.Replace(ServiceDescriptor.Transient<ILinkFactory, HttpLinkFactory>());

            return this;
        }

        public HypermediaServiceBuilder AllowTemplatedHref()
        {
            Services.Replace(ServiceDescriptor.Transient<IUrlProvider, WithTemplatedUrlProvider>());

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
