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
            queryParams = null;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the properties of a query object based on parameters defined in a
        /// SQL statement as well as properties with Where and Case attributes
        /// </summary>
        private static IEnumerable<PropertyInfo> GetProperties(object query, string sql, out IEnumerable<string> builtInParams)
        {
            // this gets the param names within the query based on words with leading '@'
            builtInParams = RegexHelper.GetParameterNames(sql, true).Select(p => p.ToLower());
            var builtInParamsArray = builtInParams.ToArray();

            // these are the properties of the Query that are explicitly defined and may impact the WHERE clause
            var queryProps = query.GetType().GetProperties().Where(pi =>
                pi.HasAttribute<WhereAttribute>() ||
                pi.HasAttribute<CaseAttribute>() ||
                pi.HasAttribute<PhraseAttribute>() ||
                pi.HasAttribute<ParameterAttribute>() ||
                builtInParamsArray.Contains(pi.Name.ToLower()));

            return queryProps;
        }
    }
}
