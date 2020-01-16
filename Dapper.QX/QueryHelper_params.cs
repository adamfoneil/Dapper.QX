using Dapper.QX.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.QX
{
    public static partial class QueryHelper
    {
        public static string ResolveParams(object query, DynamicParameters queryParams)
        {
            try
            {
                if (!queryParams?.ParameterNames?.Any() ?? true)
                {
                    return "/* no params defined */";
                }

                var sqlTypes = new Dictionary<Type, TypeSyntax>()
                {
                    { typeof(string), new TypeSyntax("nvarchar(max)", true) },
                    { typeof(int), new TypeSyntax("int", false) },
                    { typeof(int?), new TypeSyntax("int", false) },
                    { typeof(long), new TypeSyntax("bigint", false) },
                    { typeof(long?), new TypeSyntax("bigint", false) },
                    { typeof(DateTime), new TypeSyntax("datetime", true) },
                    { typeof(DateTime?), new TypeSyntax("datetime", true) },
                    { typeof(bool), new TypeSyntax("bit", false) { Transform = (value) => (value.Equals("True")) ? "1" : "0" } },
                    { typeof(bool?), new TypeSyntax("bit", false) { Transform = (value) => (value.Equals("True")) ? "1" : "0" } },
                    { typeof(decimal), new TypeSyntax("decimal", false) },
                    { typeof(decimal?), new TypeSyntax("decimal", false) }
                };

                TypeSyntax getSqlType(Type type)
                {
                    try
                    {
                        return sqlTypes[type];
                    }
                    catch
                    {
                        return new TypeSyntax($"/* {type.Name} not supported */", false);
                    }
                }

                Dictionary<string, TypeValue> paramInfo = GetParamInfo(queryParams, query);

                string declare = "DECLARE " + string.Join(", ", paramInfo.Select(kp => $"@{kp.Key} {getSqlType(kp.Value.Type).Type}")) + ";";
                string set = string.Join("\r\n", paramInfo.Select(kp => $"SET @{kp.Key} = {getSqlType(kp.Value.Type).FormatValue(kp.Value.ValueLiteral)};"));

                return declare + "\r\n" + set;

            }
            catch (Exception exc)
            {
                return $"Error generating debug SQL: {exc.Message}";
            }
        }

        private static Dictionary<string, TypeValue> GetParamInfo(DynamicParameters queryParams, object query)
        {
            var typeMap = from prop in query.GetType().GetProperties()
                          join param in queryParams.ParameterNames on prop.Name equals param
                          select new
                          {
                              Name = param,
                              Type = prop.PropertyType,
                              Value = prop.GetValue(query)?.ToString()
                          };

            return typeMap.ToDictionary(item => item.Name, item => new TypeValue() { Type = item.Type, ValueLiteral = item.Value });
        }
    }
}
