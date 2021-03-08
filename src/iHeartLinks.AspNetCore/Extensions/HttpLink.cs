using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public class HttpLink : Link
    {
        public HttpLink(string href, string method)
            : base(href)
        {
            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentException($"Parameter '{nameof(method)}' must not be null or empty.");
            }

            Method = method;
        }

        public new string Href => base.Href;

        public string Method { get; }

        public bool? Templated { get; set; }
    }
}
