using System;

namespace iHeartLinks.AspNetCore.Extensions
{
    public static class LinkRequestBuilderExtension
    {
        public static LinkRequestBuilder SetIsTemplated(this LinkRequestBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Set(IsTemplatedEnricher.TemplatedKey, true);
        }
    }
}
