using System;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
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

            services.AddHateoas((o, h) => o.UseAbsoluteUrlHref(h));

            return services;
        }

        public static IServiceCollection AddHateoas(this IServiceCollection services, Action<HypermediaServiceOptions> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            TryAddDependencies(services);

            services.Configure(configureOptions);

            return services;
        }

        public static IServiceCollection AddHateoas(this IServiceCollection services, Action<HypermediaServiceOptions, IHttpContextAccessor> configureOptions)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureOptions == null)
            {
                throw new ArgumentNullException(nameof(configureOptions));
            }

            TryAddDependencies(services);

            services
                .AddOptions<HypermediaServiceOptions>()
                .Configure(configureOptions);

            return services;
        }

        private static void TryAddDependencies(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddTransient<IUrlHelperBuilder, UrlHelperBuilder>();
            services.TryAddTransient<IHypermediaService, HypermediaService>();
        }
    }
}
