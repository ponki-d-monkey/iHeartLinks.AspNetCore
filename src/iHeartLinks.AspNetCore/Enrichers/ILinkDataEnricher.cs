using iHeartLinks.AspNetCore.LinkKeyProcessors;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public interface ILinkDataEnricher
    {
        void Enrich(LinkKey linkKey, LinkDataWriter writer);
    }
}
