using System.Collections.Generic;
using System.Linq;

namespace Dapper.QX.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool ContainsAny<T>(this IEnumerable<T> items, IEnumerable<T> searchFor)
        {
            return items.Any(item => searchFor.Contains(item));
        }
    }
}
