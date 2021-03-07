using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public static class LinkFactoryContextExtension
    {
        public static Uri GetBaseUrl(this LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Get(LinkFactoryContext.BaseUrlKey) as Uri;
        }

        public static Uri GetUrlPath(this LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return context.Get(LinkFactoryContext.UrlPathKey) as Uri;
        }

        public static Uri GetHref(this LinkFactoryContext context)
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

            if (baseUrl == null)
            {
                return urlPath;
            }

            if (urlPath == null)
            {
                return baseUrl;
            }

            Uri.TryCreate(baseUrl, urlPath, out Uri absoluteUrl);

            return absoluteUrl;
        }

        public static LinkFactoryContext SetBaseUrl(this LinkFactoryContext context, Uri value)
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

        public static LinkFactoryContext SetUrlPath(this LinkFactoryContext context, Uri value)
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

        public static LinkMapper<TLink> MapTo<TLink>(this LinkFactoryContext context, Func<Uri, TLink> createHandler)
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

        public static LinkMapper<TLink> MapTo<TLink>(this LinkFactoryContext context, Func<Uri, LinkFactoryContext, TLink> createHandler)
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
