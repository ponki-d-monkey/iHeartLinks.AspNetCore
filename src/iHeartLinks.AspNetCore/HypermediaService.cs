using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaService : IHypermediaService
    {
        private readonly string baseUrl;
        private readonly IUrlHelper urlHelper;
        private readonly IActionDescriptorCollectionProvider provider;

        public HypermediaService(
            IBaseUrlProvider baseUrlProvider,
            IUrlHelper urlHelper,
            IActionDescriptorCollectionProvider provider)
        {
            if (baseUrlProvider == null)
            {
                throw new ArgumentNullException(nameof(baseUrlProvider));
            }

            baseUrl = baseUrlProvider.GetBaseUrl();

            this.urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string GetCurrentMethod()
        {
            return urlHelper.ActionContext.HttpContext.Request.Method;
        }

        public string GetCurrentUrl()
        {
            return GetUrl(urlHelper.ActionContext.ActionDescriptor.AttributeRouteInfo.Name);
        }

        public string GetCurrentUrlTemplate()
        {
            return $"{baseUrl}/{urlHelper.ActionContext.ActionDescriptor.AttributeRouteInfo.Template}";
        }

        public string GetMethod(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var actionDescriptor = TryGetActionDescriptor(key, $"The given key to retrieve the HTTP method does not exist. Value of '{nameof(key)}': {key}");
            var httpMethodMetadata = actionDescriptor.EndpointMetadata.FirstOrDefault(x => x is HttpMethodMetadata) as HttpMethodMetadata;
            if (httpMethodMetadata == null || httpMethodMetadata.HttpMethods == null || !httpMethodMetadata.HttpMethods.Any())
            {
                return null;
            }

            return httpMethodMetadata.HttpMethods.First();
        }

        public string GetUrl(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var routeUrl = urlHelper.RouteUrl(key);

            return $"{baseUrl}{routeUrl}";
        }

        public string GetUrl(string key, object args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var routeUrl = urlHelper.RouteUrl(key, args);

            return $"{baseUrl}{routeUrl}";
        }

        public string GetUrlTemplate(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var actionDescriptor = TryGetActionDescriptor(key, $"The given key to retrieve the URL template does not exist. Value of '{nameof(key)}': {key}");

            return $"{baseUrl}/{actionDescriptor.AttributeRouteInfo.Template}";
        }

        private ActionDescriptor TryGetActionDescriptor(string key, string exceptionMessage)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == key);
            if (actionDescriptor == null)
            {
                throw new KeyNotFoundException(exceptionMessage);
            }

            return actionDescriptor;
        }
    }
}
