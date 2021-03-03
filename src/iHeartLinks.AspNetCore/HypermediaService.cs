using System;
using System.Collections.Generic;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkKeyProcessors;
using iHeartLinks.AspNetCore.UrlProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore
{
    public class HypermediaService : IHypermediaService
    {
        private readonly Lazy<IUrlHelper> urlHelper;
        private readonly ILinkKeyProcessor linkKeyProcessor;
        private readonly IBaseUrlProvider baseUrlProvider;
        private readonly IUrlProvider urlProvider;
        private readonly IEnumerable<ILinkDataEnricher> linkDataEnrichers;
        private readonly ILinkFactory linkFactory;

        public HypermediaService(
            IUrlHelperBuilder urlHelperBuilder,
            ILinkKeyProcessor linkKeyProcessor,
            IBaseUrlProvider baseUrlProvider,
            IUrlProvider urlProvider,
            IEnumerable<ILinkDataEnricher> linkDataEnrichers,
            ILinkFactory linkFactory)
        {
            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = new Lazy<IUrlHelper>(() => urlHelperBuilder.Build());

            this.linkKeyProcessor = linkKeyProcessor ?? throw new ArgumentNullException(nameof(linkKeyProcessor));
            this.baseUrlProvider = baseUrlProvider ?? throw new ArgumentNullException(nameof(baseUrlProvider));
            this.urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
            this.linkDataEnrichers = linkDataEnrichers ?? throw new ArgumentNullException(nameof(linkDataEnrichers));
            this.linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        public Link GetLink()
        {
            return GetLink(urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Name, default);
        }

        public Link GetLink(string key, object args)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var linkKey = linkKeyProcessor.Process(key);
            var baseUrl = baseUrlProvider.Provide();
            var urlPath = urlProvider.Provide(new UrlProviderContext(linkKey)
            {
                Args = args
            });

            var linkFactoryContext = new LinkFactoryContext()
                .SetBaseUrl(baseUrl)
                .SetUrlPath(urlPath);

            var linkDataWriter = new LinkDataWriter(linkFactoryContext);
            foreach (var enricher in linkDataEnrichers)
            {
                enricher.Enrich(linkKey, linkDataWriter);
            }

            return linkFactory.Create(linkFactoryContext);
        }
    }
}
