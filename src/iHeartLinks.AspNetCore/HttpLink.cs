using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore
{
    public class HttpLink : Link
    {
        public HttpLink(string href, string method)
            : base(href)
        {
            Method = method;
        }

        public string Method { get; }

        public bool? Templated { get; set; }
    }
}
