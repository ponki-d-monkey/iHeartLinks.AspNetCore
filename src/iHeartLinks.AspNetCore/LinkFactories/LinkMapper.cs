using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public sealed class LinkMapper<TLink>
        where TLink : Link
    {
        public LinkMapper(LinkFactoryContext context, TLink link)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Link = link ?? throw new ArgumentNullException(nameof(link));
        }

        public LinkFactoryContext Context { get; }

        public TLink Link { get; }

        public LinkMapper<TLink> MapIfExisting<TValue>(string key, Action<TLink, TValue> mapper)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            var value = Context.Get(key);
            if (value != null)
            {
                mapper.Invoke(Link, (TValue)value);
            }

            return this;
        }
    }
}
