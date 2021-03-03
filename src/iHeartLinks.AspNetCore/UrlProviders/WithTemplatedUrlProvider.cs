using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace iHeartLinks.AspNetCore.UrlProviders
{
    public sealed class WithTemplatedUrlProvider : IUrlProvider
    {
        private readonly IActionDescriptorCollectionProvider provider;
        private readonly NonTemplatedUrlProvider nonTemplatedUrlProvider;

        public WithTemplatedUrlProvider(
            IActionDescriptorCollectionProvider provider,
            IUrlHelperBuilder urlHelperBuilder)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            nonTemplatedUrlProvider = new NonTemplatedUrlProvider(urlHelperBuilder);
        }

        public Uri Provide(UrlProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var id = context.LinkKey.Id;
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException($"Parameter '{nameof(context)}.{nameof(context.LinkKey)}' must contain a value for '{LinkKey.IdKey}'.");
            }

            if (RequiresTemplatedUrl(context.LinkKey))
            {
                var url = GetTemplate(id);
                if (string.IsNullOrWhiteSpace(url) ||
                    // A hack to make a templated url pass the Uri.IsWellFormedUriString() check. Will look for a more elegant solution later.
                    !Uri.IsWellFormedUriString(Regex.Replace(url, "[{}]", string.Empty), UriKind.RelativeOrAbsolute))
                {
                    throw new InvalidOperationException($"The given '{LinkKey.IdKey}' to retrieve the URL template did not provide a valid value. Value of '{LinkKey.IdKey}': {id}");
                }

                return new Uri(url, UriKind.RelativeOrAbsolute);
            }

            return nonTemplatedUrlProvider.Provide(context);
        }

        private bool RequiresTemplatedUrl(LinkKey linkKey)
        {
            return linkKey.Parts.ContainsKey("templated") && linkKey.Parts["templated"].Equals(bool.TrueString, StringComparison.CurrentCultureIgnoreCase);
        }

        private string GetTemplate(string id)
        {
            var actionDescriptor = provider.ActionDescriptors.Items.FirstOrDefault(x => x.AttributeRouteInfo.Name == id);
            if (actionDescriptor == null)
            {
                throw new KeyNotFoundException($"The given '{nameof(id)}' to retrieve the URL template does not exist. Value of '{nameof(id)}': {id}");
            }

            return actionDescriptor.AttributeRouteInfo.Template;
        }
    }
}
