using System.Collections.Generic;
using System.Reflection;

namespace iHeartLinks.AspNetCore.Extensions
{
    public interface IQueryNameSelector
    {
        IEnumerable<string> Select(PropertyInfo[] parameterProperties);
    }
}
