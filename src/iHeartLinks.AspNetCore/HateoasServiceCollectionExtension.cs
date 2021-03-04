using System;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace iHeartLinks.AspNetCore
{
    public static class HateoasServiceCollectionExtension
    {
        public static IServiceCollection AddHateoas(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddTransient<IUrlHelperBuilder, UrlHelperBuilder>();
            services.TryAddTransient<ILinkRequestProcessor, PipeDelimitedLinkRequestProcessor>();
            services.TryAddTransient<IBaseUrlProvider, CurrentRequestBaseUrlProvider>();
            services.TryAddTransient<IUrlProvider, NonTemplatedUrlProvider>();
            services.TryAddTransient<ILinkFactory, LinkFactory>();
            services.TryAddTransient<IHypermediaService, HypermediaService>();

            return services;
        }

        public static IServiceCollection AddHateoas(this IServiceCollection services, Action<HypermediaServiceBuilder> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Invoke(new HypermediaServiceBuilder(services.AddHateoas()));

            return services;
        }
    }
}
