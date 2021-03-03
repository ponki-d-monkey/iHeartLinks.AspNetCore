using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace iHeartLinks.AspNetCore.LinkKeyProcessors
{
    public class LinkKey
    {
        public const string IdKey = "id";

        public LinkKey(IDictionary<string, string> parts)
        {
            if (parts == null || !parts.Any())
            {
                throw new ArgumentException($"Parameter '{nameof(parts)}' must not be null or empty.");
            }

            Parts = new ReadOnlyDictionary<string, string>(parts);
        }

        public IReadOnlyDictionary<string, string> Parts { get; private set; }

        public string Id => Parts.GetValueOrDefault(IdKey);
    }
}
