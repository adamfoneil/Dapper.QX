﻿using Dapper.QX.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.QX
{
    public static partial class QueryHelper
    {
        public static string ResolveParams(object query, DynamicParameters queryParams)
        {            
            var sqlTypes = new Dictionary<Type, TypeSyntax>()
            {
                { typeof(string), new TypeSyntax("nvarchar(max)", true) },                
                { typeof(int), new TypeSyntax("int", false) },
                { typeof(int?), new TypeSyntax("int", false) },
                { typeof(long), new TypeSyntax("bigint", false) },
                { typeof(long?), new TypeSyntax("bigint", false) },
                { typeof(DateTime), new TypeSyntax("datetime", true) },
                { typeof(DateTime?), new TypeSyntax("datetime", true) },
                { typeof(bool), new TypeSyntax("bit", false) },
                { typeof(bool?), new TypeSyntax("bit", false) },
                { typeof(decimal), new TypeSyntax("decimal", false) },
                { typeof(decimal?), new TypeSyntax("decimal", false) }
            };            

            TypeSyntax getSqlType(Type type)
            {
                try
                {
                    return sqlTypes[type];
                }
                catch (Exception exc)
                {
                    throw new Exception($"Error getting SQL type for {type.Name}: {exc.Message}");
                }
            }

            Dictionary<string, TypeValue> paramInfo = GetParamInfo(queryParams, query);

            string declare = "DECLARE " + string.Join(", ", paramInfo.Select(kp => $"@{kp.Key} {getSqlType(kp.Value.Type).Type}")) + ";";
            string set = string.Join("\r\n", paramInfo.Select(kp => $"SET @{kp.Key} = {getSqlType(kp.Value.Type).FormatValue(kp.Value.ValueLiteral)};"));

            return declare + "\r\n" + set;
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