using System;
using System.Linq;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public sealed class HttpMethodEnricher : ILinkDataEnricher
    {
        private const string HttpMethodKey = "httpMethod";

        private readonly IActionDescriptorCollectionProvider provider;

        public HttpMethodEnricher(IActionDescriptorCollectionProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Enrich(LinkKey linkKey, LinkDataWriter writer)
        {
            if (linkKey == null)
            {
                throw new ArgumentNullException(nameof(linkKey));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var httpMethod = GetHttpMethod(linkKey.Id);
            if (!string.IsNullOrWhiteSpace(httpMethod))
            {
                writer.Write(HttpMethodKey, httpMethod);
            }
        }

        private string GetHttpMethod(string id)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == id);
            if (actionDescriptor == null)
            {
                return null;
            }

            var httpMethodMetadata = actionDescriptor.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata) as HttpMethodMetadata;
            if (httpMethodMetadata == null || httpMethodMetadata.HttpMethods == null || !httpMethodMetadata.HttpMethods.Any())
            {
                return null;
            }

            return httpMethodMetadata.HttpMethods.First();
        }
    }
}
