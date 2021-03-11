using System;
using iHeartLinks.AspNetCore.Enrichers;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public sealed class IsTemplatedEnricher : ILinkDataEnricher
    {
        public const string TemplatedKey = "templated";

        public void Enrich(LinkRequest request, LinkDataWriter writer)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (request.IsTemplated())
            {
                writer.Write(TemplatedKey, true);
            }
        }
    }
}
