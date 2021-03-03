using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public static class LinkFactoryContextExtension
    {
        public static string GetBaseUrl(this LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Get(LinkFactoryContext.BaseUrlKey)?.ToString();
        }

        public static string GetUrlPath(this LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Get(LinkFactoryContext.UrlPathKey)?.ToString();
        }

        public static string GetHref(this LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var baseUrl = context.GetBaseUrl();
            var urlPath = context.GetUrlPath();

            if (baseUrl == null && urlPath == null)
            {
                return null;
            }

            return $"{baseUrl}{urlPath}";
        }

        public static LinkFactoryContext SetBaseUrl(this LinkFactoryContext context, object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            context.Set(LinkFactoryContext.BaseUrlKey, value);

            return context;
        }

        public static LinkFactoryContext SetUrlPath(this LinkFactoryContext context, object value)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            context.Set(LinkFactoryContext.UrlPathKey, value);

            return context;
        }

        public static LinkMapper<TLink> MapTo<TLink>(this LinkFactoryContext context, Func<string, TLink> createHandler)
            where TLink : Link
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (createHandler == null)
            {
                throw new ArgumentNullException(nameof(createHandler));
            }

            var link = createHandler.Invoke(context.GetHref());

            return new LinkMapper<TLink>(context, link);
        }

        public static LinkMapper<TLink> MapTo<TLink>(this LinkFactoryContext context, Func<string, LinkFactoryContext, TLink> createHandler)
            where TLink : Link
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (createHandler == null)
            {
                throw new ArgumentNullException(nameof(createHandler));
            }

            var link = createHandler.Invoke(context.GetHref(), context);

            return new LinkMapper<TLink>(context, link);
        }
    }
}
