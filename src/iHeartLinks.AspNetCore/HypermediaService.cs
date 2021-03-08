using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore
{
    public sealed class HypermediaService : IHypermediaService
    {
        private readonly Lazy<IUrlHelper> urlHelper;
        private readonly ILinkRequestProcessor linkRequestProcessor;
        private readonly IBaseUrlProvider baseUrlProvider;
        private readonly IUrlPathProvider urlPathProvider;
        private readonly IEnumerable<ILinkDataEnricher> linkDataEnrichers;
        private readonly ILinkFactory linkFactory;

        public HypermediaService(
            IUrlHelperBuilder urlHelperBuilder,
            ILinkRequestProcessor linkRequestProcessor,
            IBaseUrlProvider baseUrlProvider,
            IUrlPathProvider urlPathProvider,
            IEnumerable<ILinkDataEnricher> linkDataEnrichers,
            ILinkFactory linkFactory)
        {
            if (urlHelperBuilder == null)
            {
                throw new ArgumentNullException(nameof(urlHelperBuilder));
            }

            urlHelper = new Lazy<IUrlHelper>(() => urlHelperBuilder.Build());

            this.linkRequestProcessor = linkRequestProcessor ?? throw new ArgumentNullException(nameof(linkRequestProcessor));
            this.baseUrlProvider = baseUrlProvider ?? throw new ArgumentNullException(nameof(baseUrlProvider));
            this.urlPathProvider = urlPathProvider ?? throw new ArgumentNullException(nameof(urlPathProvider));
            this.linkDataEnrichers = linkDataEnrichers ?? throw new ArgumentNullException(nameof(linkDataEnrichers));
            this.linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        public Link GetLink()
        {
            var query = urlHelper.Value.ActionContext.HttpContext.Request.Query?.ToDictionary(x => x.Key, x=> x.Value.ToString());
            return GetLink(urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Name, query);
        }

        public Link GetLink(string request, object args)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must not be null or empty.");
            }

            var baseUrl = GetBaseUrlOrThrow();
            var linkRequest = linkRequestProcessor.Process(request);
            var urlPath = GetUrlPathOrThrow(linkRequest, args);
            var linkFactoryContext = new LinkFactoryContext()
                .SetBaseUrl(baseUrl)
                .SetUrlPath(urlPath);

            EnrichLinkFactoryContext(linkRequest, linkFactoryContext);

            return linkFactory.Create(linkFactoryContext);
        }

        private Uri GetBaseUrlOrThrow()
        {
            var baseUrl = baseUrlProvider.Provide();
            if (baseUrl == null)
            {
                throw new InvalidOperationException("The base URL provider must not return a null value.");
            }

            return baseUrl;
        }

        private Uri GetUrlPathOrThrow(LinkRequest linkRequest, object args)
        {
            var urlPath = urlPathProvider.Provide(new UrlPathProviderContext(linkRequest)
            {
                Args = args
            });

            if (urlPath == null)
            {
                throw new InvalidOperationException("The URL path provider must not return a null value.");
            }

            return urlPath;
        }

        private void EnrichLinkFactoryContext(LinkRequest linkRequest, LinkFactoryContext linkFactoryContext)
        {
            var linkDataWriter = new LinkDataWriter(linkFactoryContext);
            foreach (var enricher in linkDataEnrichers)
            {
                enricher.Enrich(linkRequest, linkDataWriter);
            }
        }
    }
}
