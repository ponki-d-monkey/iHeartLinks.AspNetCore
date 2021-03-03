using System;
using System.Collections.Generic;
using System.Linq;

namespace iHeartLinks.AspNetCore.LinkKeyProcessors
{
    public class PipeDelimitedLinkKeyProcessor : ILinkKeyProcessor
    {
        public LinkKey Process(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Parameter '{nameof(key)}' must not be null or empty.");
            }

            var equalDelimitedKeyValuePair = key.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var keyValuePairs = equalDelimitedKeyValuePair.Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries));
            var sanitizedKeyValuePairs = SanitizeKeyValuePairsOrThrowException(keyValuePairs);

            return new LinkKey(sanitizedKeyValuePairs.ToDictionary(x => x[0], x => x[1]));
        }

        private IEnumerable<string[]> SanitizeKeyValuePairsOrThrowException(IEnumerable<string[]> keyValuePairs)
        {
            var multipleIdsErrorMessage = $"Multiple values found for '{LinkKey.IdKey}'. A keyless value is treated as '{LinkKey.IdKey}'. If it is present, there is no need to supply a value with '{LinkKey.IdKey}' key explicitly.";
            var existingKeys = new HashSet<string>();
            foreach (var pair in keyValuePairs)
            {
                if (pair.Length > 2)
                {
                    throw new ArgumentException("A key-value pair delimited by an equal sign produced by splitting the key must contain 2 parts at the most.");
                }

                if (pair.Length > 1)
                {
                    var key = pair[0].Trim();
                    if (existingKeys.Contains(key))
                    {
                        var message = key == LinkKey.IdKey ?
                            multipleIdsErrorMessage :
                            "A key-value pair delimited by an equal sign produced by splitting the key must have a unique key.";

                        throw new ArgumentException(message);
                    }

                    existingKeys.Add(key);
                    yield return new[] { key, pair[1].Trim() };
                    continue;
                }

                if (existingKeys.Contains(LinkKey.IdKey))
                {
                    throw new ArgumentException(multipleIdsErrorMessage);
                }

                existingKeys.Add(LinkKey.IdKey);
                yield return new[] { LinkKey.IdKey, pair[0].Trim() };
            }
        }
    }
}
