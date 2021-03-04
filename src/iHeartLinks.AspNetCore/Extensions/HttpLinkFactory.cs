using iHeartLinks.AspNetCore.LinkFactories;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public sealed class HttpLinkFactory : LinkFactory
    {
        protected override Link DoCreate(LinkFactoryContext context)
        {
            return context
                .MapTo((h, c) => new HttpLink(h, c.Get(HttpMethodEnricher.HttpMethodKey).ToString()))
                .MapIfExisting<bool>(IsTemplatedEnricher.TemplatedKey, (l, v) =>
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
