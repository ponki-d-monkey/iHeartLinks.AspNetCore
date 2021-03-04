using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public class LinkFactory : ILinkFactory
    {
        public Link Create(LinkFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return DoCreate(context);
        }

        protected virtual Link DoCreate(LinkFactoryContext context)
        {
            return context.MapTo(h => new Link(h)).Link;
        }
    }
}
