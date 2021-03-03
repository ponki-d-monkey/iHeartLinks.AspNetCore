using System;
using System.Collections.Generic;
using System.Linq;

namespace iHeartLinks.AspNetCore.LinkRequestProcessors
{
    public class PipeDelimitedLinkRequestProcessor : ILinkRequestProcessor
    {
        public LinkRequest Process(string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException($"Parameter '{nameof(request)}' must not be null or empty.");
            }

            var equalDelimitedKeyValuePair = request.Split('|', StringSplitOptions.RemoveEmptyEntries);
            var keyValuePairs = equalDelimitedKeyValuePair.Select(x => x.Split('=', StringSplitOptions.RemoveEmptyEntries));
            var sanitizedKeyValuePairs = SanitizeKeyValuePairsOrThrowException(keyValuePairs);

            return new LinkRequest(sanitizedKeyValuePairs.ToDictionary(x => x[0], x => x[1]));
        }

        private IEnumerable<string[]> SanitizeKeyValuePairsOrThrowException(IEnumerable<string[]> keyValuePairs)
        {
            var multipleIdsErrorMessage = $"Multiple values found for '{LinkRequest.IdKey}'. A keyless value is treated as '{LinkRequest.IdKey}'. If it is present, there is no need to supply a value with '{LinkRequest.IdKey}' key explicitly.";
            var existingKeys = new HashSet<string>();
            foreach (var pair in keyValuePairs)
            {
                if (pair.Length > 2)
                {
                    throw new ArgumentException("A key-value pair delimited by an equal sign produced by splitting the request must contain 2 parts at the most.");
                }

                if (pair.Length > 1)
                {
                    var key = pair[0].Trim();
                    if (existingKeys.Contains(key))
                    {
                        var message = key == LinkRequest.IdKey ?
                            multipleIdsErrorMessage :
                            "A key-value pair delimited by an equal sign produced by splitting the request must have a unique key.";

                        throw new ArgumentException(message);
                    }

                    existingKeys.Add(key);
                    yield return new[] { key, pair[1].Trim() };
                    continue;
                }

                if (existingKeys.Contains(LinkRequest.IdKey))
                {
                    throw new ArgumentException(multipleIdsErrorMessage);
                }

                existingKeys.Add(LinkRequest.IdKey);
                yield return new[] { LinkRequest.IdKey, pair[0].Trim() };
            }
        }
    }
}
