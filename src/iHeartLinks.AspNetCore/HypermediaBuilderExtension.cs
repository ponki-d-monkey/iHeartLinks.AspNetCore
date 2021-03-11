using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore
{
    public static class HypermediaBuilderExtension
    {
        private const string SelfRel = "self";

        public static IHypermediaBuilder<TDocument> AddSelfRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string routeName)
            where TDocument : IHypermediaDocument
        {
            return AddRouteLink(builder, SelfRel, routeName);
        }

        public static IHypermediaBuilder<TDocument> AddSelfRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string routeName, object routeValues)
            where TDocument : IHypermediaDocument
        {
            return AddRouteLink(builder, SelfRel, routeName, routeValues);
        }

        public static IHypermediaBuilder<TDocument> AddSelfRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string routeName, Func<TDocument, bool> conditionHandler)
            where TDocument : IHypermediaDocument
        {
            return AddRouteLink(builder, SelfRel, routeName, conditionHandler);
        }

        public static IHypermediaBuilder<TDocument> AddSelfRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string routeName, object routeValues, Func<TDocument, bool> conditionHandler)
            where TDocument : IHypermediaDocument
        {
            return AddRouteLink(builder, SelfRel, routeName, routeValues, conditionHandler);
        }

        public static IHypermediaBuilder<TDocument> AddRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string routeName, Func<TDocument, bool> conditionHandler)
            where TDocument : IHypermediaDocument
        {
            ValidateCommonParameters(builder, rel, routeName);

            if (conditionHandler == null)
            {
                throw new ArgumentNullException(nameof(conditionHandler));
            }

            if (!conditionHandler.Invoke(builder.Document))
            {
                return builder;
            }

            return DoAddRouteLink(builder, rel, routeName);
        }

        public static IHypermediaBuilder<TDocument> AddRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string routeName)
            where TDocument : IHypermediaDocument
        {
            ValidateCommonParameters(builder, rel, routeName);

            return DoAddRouteLink(builder, rel, routeName);
        }

        public static IHypermediaBuilder<TDocument> AddRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string routeName, object routeValues, Func<TDocument, bool> conditionHandler)
            where TDocument : IHypermediaDocument
        {
            ValidateCommonParameters(builder, rel, routeName);

            if (conditionHandler == null)
            {
                throw new ArgumentNullException(nameof(conditionHandler));
            }

            if (routeValues == null)
            {
                throw new ArgumentNullException(nameof(routeValues));
            }

            if (!conditionHandler.Invoke(builder.Document))
            {
                return builder;
            }

            return DoAddRouteLink(builder, rel, routeName, routeValues);
        }

        public static IHypermediaBuilder<TDocument> AddRouteLink<TDocument>(this IHypermediaBuilder<TDocument> builder, string rel, string routeName, object routeValues)
            where TDocument : IHypermediaDocument
        {
            ValidateCommonParameters(builder, rel, routeName);

            if (routeValues == null)
            {
                throw new ArgumentNullException(nameof(routeValues));
            }

            return DoAddRouteLink(builder, rel, routeName, routeValues);
        }

        private static void ValidateCommonParameters<TDocument>(IHypermediaBuilder<TDocument> builder, string rel, string routeName) where TDocument : IHypermediaDocument
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
        }

        private static IHypermediaBuilder<TDocument> DoAddRouteLink<TDocument>(IHypermediaBuilder<TDocument> builder, string rel, string routeName)
            where TDocument : IHypermediaDocument
        {
            var link = builder.Service.GetLink(LinkRequestBuilder
                .CreateWithRouteName(routeName));

            builder.AddLink(rel, link);

            return builder;
        }

        private static IHypermediaBuilder<TDocument> DoAddRouteLink<TDocument>(IHypermediaBuilder<TDocument> builder, string rel, string routeName, object routeValues)
            where TDocument : IHypermediaDocument
        {
            var link = builder.Service.GetLink(LinkRequestBuilder
                .CreateWithRouteName(routeName)
                .SetRouteValuesIfNotNull(routeValues));

            builder.AddLink(rel, link);

            return builder;
        }
    }
}
