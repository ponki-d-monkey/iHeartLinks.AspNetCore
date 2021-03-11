using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace iHeartLinks.AspNetCore.Extensions
{
    public class WithTemplatedUrlPathProvider : IUrlPathProvider
    {
        private readonly IQueryNameSelector selector;
        private readonly IActionDescriptorCollectionProvider provider;
        private readonly NonTemplatedUrlPathProvider nonTemplatedUrlPathProvider;

        public WithTemplatedUrlPathProvider(
            IQueryNameSelector selector,
            IActionDescriptorCollectionProvider provider,
            IUrlHelperBuilder urlHelperBuilder)
        {
            this.selector = selector ?? throw new ArgumentNullException(nameof(selector));
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            nonTemplatedUrlPathProvider = new NonTemplatedUrlPathProvider(urlHelperBuilder);
        }

        public Uri Provide(LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var routeName = request.GetRouteName();
            if (string.IsNullOrWhiteSpace(routeName))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must contain a '{LinkRequestBuilder.RouteNameKey}' value.");
            }

            if (request.IsTemplated())
            {
                var template = GetTemplate(routeName);
                if (string.IsNullOrWhiteSpace(template))
                {
                    throw new InvalidOperationException($"The given '{LinkRequestBuilder.RouteNameKey}' to retrieve the URL template returned a null or empty value. Value of '{LinkRequestBuilder.RouteNameKey}': {routeName}");
                }

                return new Uri($"/{template}", UriKind.Relative);
            }

            return nonTemplatedUrlPathProvider.Provide(request);
        }

        protected virtual string FormatTemplate(string template, string queryTemplate)
        {
            return $"{template}{queryTemplate}";
        }

        protected virtual string FormatQueryTemplate(IEnumerable<string> queryNames)
        {
            return $"{{?{string.Join(',', queryNames)}}}";
        }

        private string GetTemplate(string routeName)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == routeName);
            if (actionDescriptor == null)
            {
                throw new KeyNotFoundException($"The given '{LinkRequestBuilder.RouteNameKey}' to retrieve the URL template does not exist. Value of '{LinkRequestBuilder.RouteNameKey}': {routeName}");
            }

            var template = actionDescriptor.AttributeRouteInfo.Template;
            var queryTemplate = GetQueryTemplate(actionDescriptor);
            if (!string.IsNullOrWhiteSpace(queryTemplate))
            {
                template = FormatTemplate(template, queryTemplate);
            }

            return template;
        }

        private string GetQueryTemplate(ActionDescriptor actionDescriptor)
        {
            var query = actionDescriptor.Parameters.FirstOrDefault(x => x.BindingInfo.BindingSource.Id.Equals("query", StringComparison.CurrentCultureIgnoreCase));
            if (query == null)
            {
                return null;
            }

            var queryNames = selector.Select(query.ParameterType.GetProperties()) ?? Enumerable.Empty<string>();
            if (!queryNames.Any())
            {
                return null;
            }

            return FormatQueryTemplate(queryNames);
        }
    }
}
