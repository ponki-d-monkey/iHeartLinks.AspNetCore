using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace iHeartLinks.AspNetCore.Extensions
{
    public class HttpMethodEnricher : ILinkDataEnricher
    {
        public const string HttpMethodKey = "httpMethod";

        private readonly IActionDescriptorCollectionProvider provider;

        public HttpMethodEnricher(IActionDescriptorCollectionProvider provider)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Enrich(LinkRequest request, LinkDataWriter writer)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var httpMethod = GetHttpMethod(request.GetRouteName());
            if (!string.IsNullOrWhiteSpace(httpMethod))
            {
                writer.Write(HttpMethodKey, httpMethod);
            }
        }

        protected virtual string GetHttpMethod(IReadOnlyList<string> httpMethods)
        {
            return httpMethods.First();
        }

        private string GetHttpMethod(string routeName)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == routeName);
            if (actionDescriptor == null)
            {
                return null;
            }

            var httpMethodMetadata = actionDescriptor.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata) as HttpMethodMetadata;
            if (httpMethodMetadata == null || httpMethodMetadata.HttpMethods == null || !httpMethodMetadata.HttpMethods.Any())
            {
                return null;
            }

            return GetHttpMethod(httpMethodMetadata.HttpMethods);
        }
    }
}
