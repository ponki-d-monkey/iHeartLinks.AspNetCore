using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public interface ILinkFactory
    {
        Link Create(LinkFactoryContext context);
    }
}
