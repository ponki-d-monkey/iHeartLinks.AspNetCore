using System;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped(x =>
            {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();

                return factory.GetUrlHelper(actionContext);
            });

            services.AddScoped<IHypermediaService, HypermediaService>();

            return services;
        }
    }
}
