using System;
using iHeartLinks.AspNetCore.LinkRequestProcessors;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public sealed class IsTemplatedEnricher : ILinkDataEnricher
    {
        private const string TemplatedKey = "templated";

        public void Enrich(LinkRequest linkRequest, LinkDataWriter writer)
        {
            if (linkRequest == null)
            {
                throw new ArgumentNullException(nameof(linkRequest));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!linkRequest.Parts.TryGetValue(TemplatedKey, out string templatedString))
            {
                return;
            }

            if (!bool.TryParse(templatedString, out bool templated))
            {
                return;
            }

            if (templated)
            {
                writer.Write(TemplatedKey, templated);
            }
        }
    }
}
