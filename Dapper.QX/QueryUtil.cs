using Dapper.QX.Attributes;
using Dapper.QX.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.QX
{
    public static class QueryUtil
    {
        public static string ResolveSql(string sql, object parameters, out DynamicParameters queryParams)
        {
            if (parameters == null)
            {
                queryParams = null;
                return RegexHelper.RemovePlaceholders(sql);
            }

            string result = sql;

            var properties = GetProperties(parameters, sql, out QueryParameters paramInfo);
            
            queryParams = GetDynamicParameters(parameters, properties);
            
            result = ResolveOptionalParams(result, properties, parameters, paramInfo);

            var placeholders = RegexHelper.ParsePlaceholders(result);
            

            result = ResolvePropertyParams(result, properties, parameters, queryParams);            
            
            return result;  
        }

        private static DynamicParameters GetDynamicParameters(object parameters, IEnumerable<PropertyInfo> properties)
        {
            var result = new DynamicParameters();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameters);
                if (value != null && !prop.HasAttribute<PhraseAttribute>()) result.Add(prop.Name, value);
            }
            return result;
        }

        /// <summary>
        /// Returns the properties of a query object based on parameters defined in a
        /// SQL statement as well as properties with Where and Case attributes
        /// </summary>
        private static IEnumerable<PropertyInfo> GetProperties(object parameters, string sql, out QueryParameters paramInfo)
        {
            // this gets the param names within the query based on words with leading '@'
            paramInfo = RegexHelper.ParseParameters(sql, cleaned: true);
            
            var allParams = paramInfo.AllParamNames().Select(p => p.ToLower()).ToArray();            

            // these are the properties of the Query that are explicitly defined and may impact the WHERE clause
            var queryProps = parameters.GetType().GetProperties().Where(pi =>
                pi.HasAttribute<WhereAttribute>() ||
                pi.HasAttribute<CaseAttribute>() ||
                pi.HasAttribute<PhraseAttribute>() ||
                pi.HasAttribute<ParameterAttribute>() ||
                allParams.Contains(pi.Name.ToLower()));

            return queryProps;
        }

        private static string RemovePlaceholders(string sql)
        {
            return RegexHelper.RemovePlaceholders(sql);

            string result = sql;

            var tokens = GetWhereTokens();
            foreach (var kp in tokens) result = result.Replace(kp.Key, string.Empty);

            return result;
        }
    }
}
