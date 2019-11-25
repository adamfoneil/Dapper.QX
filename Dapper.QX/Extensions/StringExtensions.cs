using System.Collections.Generic;
using System.Linq;

namespace Dapper.QX.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsAnyOf(this string search, IEnumerable<string> items, out string result)
        {
            result = items.FirstOrDefault(text => search.Contains(text));
            return (result != null);
        }
    }
}
