using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Enrichers
{
    public interface ILinkDataEnricher
    {
        void Enrich(LinkRequest request, LinkDataWriter writer);
    }
}
