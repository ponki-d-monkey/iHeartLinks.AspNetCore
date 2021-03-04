using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public class HttpLink : Link
    {
        public HttpLink(string href, string method)
            : base(href)
        {
            Method = method;
        }

        public new string Href => base.Href;

        public string Method { get; }

        public bool? Templated { get; set; }
    }
}
