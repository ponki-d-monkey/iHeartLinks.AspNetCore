using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlPathProviders;
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

            return UseBaseUrlProvider<CurrentRequestBaseUrlProvider>();
        }

        public HypermediaServiceBuilder UseRelativeUrlHref()
        {
            return UseBaseUrlProvider<EmptyBaseUrlProvider>();
        }

        public HypermediaServiceBuilder UseCustomBaseUrlHref(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute))
            {
                throw new ArgumentException($"Parameter '{nameof(baseUrl)}' must not be null or empty and must be a valid URL.");
            }

            return UseBaseUrlProvider(x => new CustomBaseUrlProvider(baseUrl));
        }

        public HypermediaServiceBuilder UseBaseUrlProvider<T>()
            where T : class, IBaseUrlProvider
        {
            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider, T>());

            return this;
        }

        public HypermediaServiceBuilder UseBaseUrlProvider<T>(Func<IServiceProvider, T> implementationFactory)
            where T : class, IBaseUrlProvider
        {
            Services.Replace(ServiceDescriptor.Transient<IBaseUrlProvider, T>(implementationFactory));

            return this;
        }

        public HypermediaServiceBuilder UseUrlPathProvider<T>()
            where T : class, IUrlPathProvider
        {
            Services.Replace(ServiceDescriptor.Transient<IUrlPathProvider, T>());

            return this;
        }

        public HypermediaServiceBuilder UseUrlPathProvider<T>(Func<IServiceProvider, T> implementationFactory)
            where T : class, IUrlPathProvider
        {
            Services.Replace(ServiceDescriptor.Transient<IUrlPathProvider, T>(implementationFactory));

            return this;
        }

        public HypermediaServiceBuilder UseLinkFactory<T>()
            where T : class, ILinkFactory
        {
            Services.Replace(ServiceDescriptor.Transient<ILinkFactory, T>());

            return this;
        }

        public HypermediaServiceBuilder UseLinkFactory<T>(Func<IServiceProvider, T> implementationFactory)
            where T : class, ILinkFactory
        {
            Services.Replace(ServiceDescriptor.Transient<ILinkFactory, T>(implementationFactory));

            return this;
        }

        public HypermediaServiceBuilder AddLinkDataEnricher<T>()
            where T : class, ILinkDataEnricher
        {
            Services.AddTransient<ILinkDataEnricher, T>();

            return this;
        }

        public HypermediaServiceBuilder AddLinkDataEnricher<T>(Func<IServiceProvider, T> implementationFactory)
            where T : class, ILinkDataEnricher
        {
            Services.AddTransient<ILinkDataEnricher, T>(implementationFactory);

            return this;
        }
    }
}
