using System;
using System.Collections.Generic;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.LinkRequestProcessors;
using iHeartLinks.AspNetCore.UrlProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore
{
    public sealed class HypermediaService : IHypermediaService
    {
        private readonly Lazy<IUrlHelper> urlHelper;
        private readonly ILinkRequestProcessor linkRequestProcessor;
        private readonly IBaseUrlProvider baseUrlProvider;
        private readonly IUrlProvider urlProvider;
        private readonly IEnumerable<ILinkDataEnricher> linkDataEnrichers;
        private readonly ILinkFactory linkFactory;

        public HypermediaService(
            IUrlHelperBuilder urlHelperBuilder,
            ILinkRequestProcessor linkRequestProcessor,
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

            this.linkRequestProcessor = linkRequestProcessor ?? throw new ArgumentNullException(nameof(linkRequestProcessor));
            this.baseUrlProvider = baseUrlProvider ?? throw new ArgumentNullException(nameof(baseUrlProvider));
            this.urlProvider = urlProvider ?? throw new ArgumentNullException(nameof(urlProvider));
            this.linkDataEnrichers = linkDataEnrichers ?? throw new ArgumentNullException(nameof(linkDataEnrichers));
            this.linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        public Link GetLink()
        {
            return GetLink(urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Name, default);
        }

        public Link GetLink(string request, object args)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must not be null or empty.");
            }

            var baseUrl = baseUrlProvider.Provide();
            if (baseUrl == null)
            {
                throw new InvalidOperationException("The base URL provider returned a null value. Base URL is required in order to proceed.");
            }

            var linkRequest = linkRequestProcessor.Process(request);
            var urlPath = urlProvider.Provide(new UrlProviderContext(linkRequest)
            {
                Args = args
            });

            if (urlPath == null)
            {
                throw new InvalidOperationException("The URL provider returned a null value. URL path is required in order to proceed.");
            }

            var linkFactoryContext = new LinkFactoryContext()
                .SetBaseUrl(baseUrl)
                .SetUrlPath(urlPath);

            var linkDataWriter = new LinkDataWriter(linkFactoryContext);
            foreach (var enricher in linkDataEnrichers)
            {
                enricher.Enrich(linkRequest, linkDataWriter);
            }

            return linkFactory.Create(linkFactoryContext);
        }
    }
}
