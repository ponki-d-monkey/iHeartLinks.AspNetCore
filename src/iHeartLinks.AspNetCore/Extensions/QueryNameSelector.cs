using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace iHeartLinks.AspNetCore.Extensions
{
    public class QueryNameSelector : IQueryNameSelector
    {
        public IEnumerable<string> Select(PropertyInfo[] parameterProperties)
        {
            if (parameterProperties == null)
            {
                throw new ArgumentNullException(nameof(parameterProperties));
            }

            return parameterProperties
                .Select(x =>
                {
                    var name = x.GetCustomAttributes(typeof(FromQueryAttribute), true)
                        .Where(x => x is FromQueryAttribute)
                        .OfType<FromQueryAttribute>()
                        .FirstOrDefault(x => x is FromQueryAttribute)?
                        .Name;

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        name = x.Name;
                    }

                    return name;
                });
        }
    }
}
