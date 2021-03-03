using System;
using iHeartLinks.AspNetCore.LinkKeyProcessors;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public sealed class IsTemplatedEnricher : ILinkDataEnricher
    {
        private const string TemplatedKey = "templated";

        public void Enrich(LinkKey linkKey, LinkDataWriter writer)
        {
            if (linkKey == null)
            {
                throw new ArgumentNullException(nameof(linkKey));
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (!linkKey.Parts.TryGetValue(TemplatedKey, out string templatedString))
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
