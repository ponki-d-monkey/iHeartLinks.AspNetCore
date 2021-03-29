using System;
using System.Collections.Generic;
using System.Linq;
using iHeartLinks.AspNetCore.BaseUrlProviders;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.AspNetCore.UrlPathProviders;
using iHeartLinks.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace iHeartLinks.AspNetCore
{
    public sealed class HypermediaService : IHypermediaService
    {
        private readonly Lazy<IUrlHelper> urlHelper;
        private readonly IBaseUrlProvider baseUrlProvider;
        private readonly IUrlPathProvider urlPathProvider;
        private readonly IEnumerable<ILinkDataEnricher> linkDataEnrichers;
        private readonly ILinkFactory linkFactory;

        public HypermediaService(
            IUrlHelperBuilder urlHelperBuilder,
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

            this.baseUrlProvider = baseUrlProvider ?? throw new ArgumentNullException(nameof(baseUrlProvider));
            this.urlPathProvider = urlPathProvider ?? throw new ArgumentNullException(nameof(urlPathProvider));
            this.linkDataEnrichers = linkDataEnrichers ?? throw new ArgumentNullException(nameof(linkDataEnrichers));
            this.linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        public Link GetLink()
        {
            return DoGetLink(LinkRequestBuilder
                .CreateWithRouteName(urlHelper.Value.ActionContext.ActionDescriptor.AttributeRouteInfo.Name)
                .SetRouteValuesIfNotNull(ConvertToRouteValues(urlHelper.Value.ActionContext.HttpContext.Request.Query)));
        }

        public Link GetLink(LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.ContainsKey(LinkRequestBuilder.RouteNameKey))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must contain a '{LinkRequestBuilder.RouteNameKey}' value.");
            }

            if (string.IsNullOrWhiteSpace(request.GetRouteName()))
            {
                throw new ArgumentException($"The '{LinkRequestBuilder.RouteNameKey}' item in the '{nameof(request)}' parameter must not be null or empty.");
            }

            return DoGetLink(request);
        }

        private RouteValueDictionary ConvertToRouteValues(IQueryCollection query)
        {
            if (query == null)
            {
                return null;
            }

            return RouteValueDictionary.FromArray(query.ToDictionary(x => x.Key, x => x.Value as object).ToArray());
        }

        private Link DoGetLink(LinkRequest request)
        {
            var baseUrl = GetBaseUrlOrThrow();
            var urlPath = GetUrlPathOrThrow(request);
            var linkFactoryContext = new LinkFactoryContext()
                .SetBaseUrl(baseUrl)
                .SetUrlPath(urlPath);

            EnrichLinkFactoryContext(request, linkFactoryContext);

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

        private Uri GetUrlPathOrThrow(LinkRequest request)
        {
            var urlPath = urlPathProvider.Provide(request);

            if (urlPath == null)
            {
                throw new InvalidOperationException("The URL path provider must not return a null value.");
            }

            return urlPath;
        }

        private void EnrichLinkFactoryContext(LinkRequest request, LinkFactoryContext linkFactoryContext)
        {
            var linkDataWriter = new LinkDataWriter(linkFactoryContext);
            foreach (var enricher in linkDataEnrichers)
            {
                enricher.Enrich(request, linkDataWriter);
            }
        }
    }
}
