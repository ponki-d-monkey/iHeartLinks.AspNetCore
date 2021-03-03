using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public sealed class HttpLinkFactory : LinkFactory
    {
        protected override Link DoCreate(LinkFactoryContext context)
        {
            return context
                .MapTo((h, c) => new HttpLink(h, c.Get("httpMethod").ToString()))
                .MapIfExisting<bool>("templated", (l, v) =>
                {
                    if (v)
                    {
                        l.Templated = v;
                    }
                })
                .Link;
        }
    }
}
