using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaService : IHypermediaService
    {
        private readonly string baseUrl;
        private readonly Lazy<IUrlHelper> urlHelper;
        private readonly IActionDescriptorCollectionProvider provider;

        public HypermediaService(
            IOptions<HypermediaServiceOptions> options,
            IUrlHelperBuilder urlHelperBuilder,
            IActionDescriptorCollectionProvider provider)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            baseUrl = options.Value.BaseUrlProvider.GetBaseUrl();

            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = new Lazy<IUrlHelper>(() => urlHelperBuilder.Build());

            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string GetCurrentMethod()
        {
            return urlHelper.Value.ActionContext.HttpContext.Request.Method;
        }

        public string GetCurrentUrl()
        {
            return GetUrl(urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Name);
        }

        public string GetCurrentUrlTemplate()
        {
            return $"{baseUrl}/{urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Template}";
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

            var routeUrl = urlHelper.Value.RouteUrl(key);

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

            var routeUrl = urlHelper.Value.RouteUrl(key, args);

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
