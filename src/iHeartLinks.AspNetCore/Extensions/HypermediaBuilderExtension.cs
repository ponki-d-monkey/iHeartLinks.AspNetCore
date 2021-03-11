using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public static class HypermediaBuilderExtension
    {
        public static IHypermediaBuilder<TDocument> AddLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string href, string method, Func<TDocument, bool> conditionHandler)
            where TDocument : IHypermediaDocument
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(rel))
            {
                throw new ArgumentException($"Parameter '{nameof(rel)}' must not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(href))
            {
                throw new ArgumentException($"Parameter '{nameof(href)}' must not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentException($"Parameter '{nameof(method)}' must not be null or empty.");
            }

            return builder.AddLinksPerCondition(conditionHandler, b => DoAddLink(b, rel, href, method));
        }

        public static IHypermediaBuilder<TDocument> AddLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string href, string method)
            where TDocument : IHypermediaDocument
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(rel))
            {
                throw new ArgumentException($"Parameter '{nameof(rel)}' must not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(href))
            {
                throw new ArgumentException($"Parameter '{nameof(href)}' must not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentException($"Parameter '{nameof(method)}' must not be null or empty.");
            }

            return DoAddLink(builder, rel, href, method);
        }

        public static IHypermediaBuilder<TDocument> AddRouteTemplate<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string routeName)
            where TDocument : IHypermediaDocument
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrWhiteSpace(rel))
            {
                throw new ArgumentException($"Parameter '{nameof(rel)}' must not be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(routeName))
            {
                throw new ArgumentException($"Parameter '{nameof(routeName)}' must not be null or empty.");
            }

            var link = builder.Service.GetLink(LinkRequestBuilder
                .CreateWithRouteName(routeName)
                .SetIsTemplated());

            builder.AddLink(rel, link);

            return builder;
        }

        private static IHypermediaBuilder<TDocument> DoAddLink<TDocument>(IHypermediaBuilder<TDocument> builder, string rel, string href, string method) where TDocument : IHypermediaDocument
        {
            var link = new HttpLink(href, method);

            builder.AddLink(rel, link);

            return builder;
        }
    }
}
