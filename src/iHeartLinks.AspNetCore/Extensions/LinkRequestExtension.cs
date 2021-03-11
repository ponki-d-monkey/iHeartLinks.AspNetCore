using System;
using iHeartLinks.Core;

namespace iHeartLinks.AspNetCore.Extensions
{
    public static class LinkRequestExtension
    {
        public static bool IsTemplated(this LinkRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.ContainsKey(IsTemplatedEnricher.TemplatedKey) &&
                request.GetValueOrDefault(IsTemplatedEnricher.TemplatedKey) is bool templated &&
                templated;
        }
    }
}
