using System;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlProviders;
using Microsoft.Extensions.DependencyInjection;
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
                .AddLinkEnricher<IsTemplatedEnricher>()
                .AddLinkEnricher<HttpMethodEnricher>()
                .Services
                    .Replace(ServiceDescriptor.Transient<IUrlProvider, WithTemplatedUrlProvider>())
                    .Replace(ServiceDescriptor.Transient<ILinkFactory, HttpLinkFactory>());

            return builder;
        }
    }
}
