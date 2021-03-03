using System;
using System.Collections;

namespace iHeartLinks.AspNetCore.LinkFactories
{
    public class LinkFactoryContext
    {
        public const string BaseUrlKey = "baseUrl";
        public const string UrlPathKey = "urlPath";

        private readonly Hashtable hashtable;

        public LinkFactoryContext()
        {
            hashtable = new Hashtable();
        }

        public object Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            return hashtable[key];
        }

        public void Set(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            hashtable[key] = value;
        }
    }
}
