using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlPathProviders;
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

        public Uri Provide(UrlPathProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var id = context.LinkRequest.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"Parameter '{nameof(context)}.{nameof(context.LinkRequest)}' must contain a value for '{LinkRequest.IdKey}'.");
            }

            if (RequiresTemplatedUrl(context.LinkRequest))
            {
                var template = GetTemplate(id);
                if (string.IsNullOrWhiteSpace(template))
                {
                    throw new InvalidOperationException($"The given '{LinkRequest.IdKey}' to retrieve the URL template returned a null or empty value. Value of '{LinkRequest.IdKey}': {id}");
                }

                return new Uri($"/{template}", UriKind.RelativeOrAbsolute);
            }

            return nonTemplatedUrlPathProvider.Provide(context);
        }

        protected virtual string GetTemplate(string id)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == id);
            if (actionDescriptor == null)
            {
                throw new KeyNotFoundException($"The given '{nameof(id)}' to retrieve the URL template does not exist. Value of '{nameof(id)}': {id}");
            }

            var template = actionDescriptor.AttributeRouteInfo.Template;
            var query = actionDescriptor.Parameters?.FirstOrDefault(x => x.BindingInfo.BindingSource.Id.Equals("query", StringComparison.CurrentCultureIgnoreCase));
            if (query != null)
            {
                var names = selector.Select(query.ParameterType.GetProperties());
                if (names.Any())
                {
                    template = $"{template}{{?{string.Join(',', names)}}}";
                }
            }

            return template;
        }

        private bool RequiresTemplatedUrl(LinkRequest linkRequest)
        {
            return linkRequest.Parts.ContainsKey("templated") && linkRequest.Parts["templated"].Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
