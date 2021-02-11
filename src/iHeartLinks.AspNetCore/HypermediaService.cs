using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaService : IHypermediaService
    {
        public readonly IUrlHelper urlHelper;
        public readonly IActionDescriptorCollectionProvider provider;

        public HypermediaService(
            IUrlHelper urlHelper,
            IActionDescriptorCollectionProvider provider)
        {
            this.urlHelper = urlHelper ?? throw new ArgumentNullException(nameof(urlHelper));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public string GetCurrentMethod()
        {
            return urlHelper.ActionContext.HttpContext.Request.Method;
        }

        public string GetCurrentUrl()
        {
            return urlHelper.Link(urlHelper.ActionContext.ActionDescriptor.AttributeRouteInfo.Name, null);
        }

        public string GetCurrentUrlTemplate()
        {
            var baseUrl = GetCurrentRequestBaseUrl();

            return $"{baseUrl}{urlHelper.ActionContext.ActionDescriptor.AttributeRouteInfo.Template}";
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

            return urlHelper.Link(key, null);
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

            return urlHelper.Link(key, args);
        }

        public string GetUrlTemplate(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var actionDescriptor = TryGetActionDescriptor(key, $"The given key to retrieve the URL template does not exist. Value of '{nameof(key)}': {key}");
            var baseUrl = GetCurrentRequestBaseUrl();

            return $"{baseUrl}{actionDescriptor.AttributeRouteInfo.Template}";
        }

        private string GetCurrentRequestBaseUrl()
        {
            var request = urlHelper.ActionContext.HttpContext.Request;

            return $"{request.Scheme}://{request.Host.ToUriComponent()}/";
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
