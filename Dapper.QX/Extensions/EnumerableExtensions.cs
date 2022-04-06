using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.QX.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool ContainsAny<T>(this IEnumerable<T> items, IEnumerable<T> searchFor) => items.Any(item => searchFor.Contains(item));

        /// <summary>
        /// adapted from https://www.codeproject.com/Articles/835519/Passing-Table-Valued-Parameters-with-Dapper
        /// </summary>
        public static DataTable ToDataTable<T>(this IEnumerable<T> enumerable, bool simpleTypesOnly = true)
        {
            DataTable dataTable = new DataTable();

            if (typeof(T).IsValueType || typeof(T).FullName.Equals("System.String"))
            {
                dataTable.Columns.Add("ValueType", typeof(T));
                foreach (T obj in enumerable) dataTable.Rows.Add(obj);
            }
            else
            {
                Func<PropertyInfo, bool> filter = (pi) => true;
                if (simpleTypesOnly) filter = (pi) => pi.PropertyType.IsSimpleType();

                var properties = typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(pi => pi.CanRead && filter(pi))
                    .ToDictionary(item => item.Name);

                foreach (string name in properties.Keys)
                {
                    var propertyType = properties[name].PropertyType;
                    var columnType = (propertyType.IsNullableGeneric()) ? propertyType.GetGenericArguments()[0] : propertyType;
                    dataTable.Columns.Add(name, columnType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(properties.Select(kp => kp.Value.GetValue(obj)).ToArray());
                }
            }

            return dataTable;
        }
        
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable, string typeName)
        {
            var dataTable = enumerable.ToDataTable();

            return dataTable.AsTableValuedParameter(typeName);
        }

        internal static bool IsSimpleType(this Type type) => type.Equals(typeof(string)) || type.IsValueType;

        internal static bool IsNullableGeneric(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
