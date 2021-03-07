using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace iHeartLinks.AspNetCore.Extensions
{
    public static class HypermediaServiceBuilderExtension
    {
        public static HypermediaServiceBuilder UseExtendedLink(this HypermediaServiceBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
                .AddLinkDataEnricher<IsTemplatedEnricher>()
                .AddLinkDataEnricher<HttpMethodEnricher>()
                .UseUrlPathProvider<WithTemplatedUrlPathProvider>()
                .UseLinkFactory<HttpLinkFactory>()
                .Services.TryAddTransient<IQueryNameSelector, QueryNameSelector>();

            return builder;
        }
    }
}
