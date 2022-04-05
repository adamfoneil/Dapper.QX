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
        
        /// <summary>
        /// thanks to https://www.codeproject.com/Articles/835519/Passing-Table-Valued-Parameters-with-Dapper
        /// </summary>
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable, string typeName, params string[] columnNames)
        {
            var dataTable = new DataTable();

            if (typeof(T).IsValueType || typeof(T).FullName.Equals("System.String"))
            {
                dataTable.Columns.Add(columnNames == null ? "NONAME" : columnNames.First(), typeof(T));
                foreach (T obj in enumerable) dataTable.Rows.Add(obj);
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();

                if (readableProperties.Length > 1 && columnNames == null)
                {
                    throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
                }

                var createColumns = (columnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach (string name in createColumns)
                {
                    dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(createColumns.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj)).ToArray());
                }
            }

            return dataTable.AsTableValuedParameter(typeName);
        }

        internal static bool IsSimpleType(this Type type) => type.Equals(typeof(string)) || type.IsValueType;

        internal static bool IsNullableGeneric(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
