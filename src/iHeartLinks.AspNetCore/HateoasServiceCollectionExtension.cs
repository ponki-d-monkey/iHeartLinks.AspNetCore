using System;
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

            services.AddHateoas((o, b) => o.UseAbsoluteUrlHref(b));

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

        public static IServiceCollection AddHateoas(this IServiceCollection services, Action<HypermediaServiceOptions, IUrlHelperBuilder> configureOptions)
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
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.TryAddTransient<IUrlHelperBuilder, UrlHelperBuilder>();
            services.TryAddScoped<IHypermediaService, HypermediaService>();
        }
    }
}
