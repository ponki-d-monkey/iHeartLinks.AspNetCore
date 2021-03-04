using System;
using iHeartLinks.AspNetCore.LinkFactories;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public sealed class LinkDataWriter
    {
        private readonly LinkFactoryContext context;

        public LinkDataWriter(LinkFactoryContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public LinkDataWriter Write(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            context.Set(key, value);

            return this;
        }
    }
}
