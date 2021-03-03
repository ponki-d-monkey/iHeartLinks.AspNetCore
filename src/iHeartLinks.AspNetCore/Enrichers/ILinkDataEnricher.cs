using iHeartLinks.AspNetCore.LinkRequestProcessors;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public interface ILinkDataEnricher
    {
        void Enrich(LinkRequest linkRequest, LinkDataWriter writer);
    }
}
