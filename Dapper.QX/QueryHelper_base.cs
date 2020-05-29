using Dapper.QX.Attributes;
using Dapper.QX.Classes;
using Dapper.QX.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapper.QX
{
    public static partial class QueryHelper
    {
        public const string OrderByToken = "{orderBy}";
        public const string JoinToken = "{join}";
        public const string WhereToken = "{where}";
        public const string AndWhereToken = "{andWhere}";
        public const string OffsetToken = "{offset}";

        public static string ResolveSql(string sql, object parameters)
        {
            return ResolveSql(sql, parameters, out _);
        }

        public static string ResolveSql(string sql, object parameters, out DynamicParameters dynamicParams)
        {
            if (sql is null)
            {
                throw new ArgumentNullException(nameof(sql));
            }

            if (parameters == null)
            {
                dynamicParams = null;
                return RegexHelper.RemovePlaceholders(sql);
            }

            string result = sql;

            var properties = GetParamProperties(parameters, sql, out QueryParameters paramInfo);
            dynamicParams = GetDynamicParameters(properties, parameters);

            string queryTypeName = parameters.GetType().Name;

            result = ResolveInlineOptionalCriteria(result, parameters, paramInfo);
            result = ResolveOrderBy(result, parameters, queryTypeName);
            result = ResolveOptionalJoins(result, parameters);
            result = ResolveInjectedCriteria(result, paramInfo, properties, parameters, dynamicParams);
            result = ResolveOffset(result, parameters);
            result = RegexHelper.RemovePlaceholders(result);

            return result.Trim();
        }

        private static string ResolveOffset(string sql, object parameters)
        {
            string result = sql;

            if (result.Contains(OffsetToken) && FindOffsetProperty(parameters, out PropertyInfo offsetProperty))
            {
                var offsetAttr = offsetProperty.GetCustomAttribute<OffsetAttribute>();
                int page = (int)offsetProperty.GetValue(parameters);
                result = result.Replace(OffsetToken, offsetAttr.GetOffsetFetchSyntax(page));
            }

            return result;
        }

        private static bool FindOffsetProperty(object queryObject, out PropertyInfo offsetProperty)
        {
            offsetProperty = queryObject.GetType().GetProperties().FirstOrDefault(pi => pi.HasAttribute<OffsetAttribute>() && pi.PropertyType.Equals(typeof(int?)));
            if (offsetProperty != null)
            {
                object value = offsetProperty.GetValue(queryObject);
                return (value != null);
            }

            return false;
        }

        private static string ResolveInjectedCriteria(string sql, QueryParameters paramInfo, IEnumerable<PropertyInfo> properties, object parameters, DynamicParameters queryParams)
        {
            string result = sql;

            if (result.ContainsAnyOf(new string[] { WhereToken, AndWhereToken }, out string token))
            {
                List<string> terms = new List<string>();

                foreach (var pi in properties)
                {
                    if (HasValue(pi, parameters, out object value) && paramInfo.IsOptional(pi))
                    {
                        if (GetCaseExpression(pi, value, out string caseExpression))
                        {
                            terms.Add(caseExpression);
                        }
                        else if (GetWhereExpression(pi, out string whereExpression))
                        {
                            terms.Add(whereExpression);
                        }
                        else if (GetPhraseQuery(pi, value, out PhraseQuery phraseQuery))
                        {
                            queryParams.AddDynamicParams(phraseQuery.Parameters);
                            terms.Add(phraseQuery.Expression);
                        }
                    }
                }

                Dictionary<string, string> keywordOptions = new Dictionary<string, string>()
                {
                    { WhereToken, "WHERE" },
                    { AndWhereToken, "AND" }
                };

                result = result.Replace(token, (terms.Any()) ?
                    $"{keywordOptions[token]} {string.Join(" AND ", terms)}" :
                    string.Empty);
            }

            return result;
        }

        private static bool GetPhraseQuery(PropertyInfo pi, object value, out PhraseQuery phraseQuery)
        {
            PhraseAttribute phraseAttr = pi.GetAttribute<PhraseAttribute>();
            if (phraseAttr != null)
            {
                phraseQuery = new PhraseQuery(pi.Name, value.ToString(), phraseAttr);
                return true;
            }

            phraseQuery = null;
            return false;
        }

        private static bool GetWhereExpression(PropertyInfo pi, out string whereExpression)
        {
            WhereAttribute whereAttr = pi.GetAttribute<WhereAttribute>();
            if (whereAttr != null)
            {
                whereExpression = whereAttr.Expression;
                return true;
            }

            whereExpression = null;
            return false;
        }

        private static bool GetCaseExpression(PropertyInfo pi, object value, out string caseExpression)
        {
            var cases = pi.GetCustomAttributes(typeof(CaseAttribute), false).OfType<CaseAttribute>();
            var selectedCase = cases?.FirstOrDefault(c => c.Value.Equals(value));
            if (selectedCase != null)
            {
                caseExpression = selectedCase.Expression;
                return true;
            }

            caseExpression = null;
            return false;
        }

        private static string ResolveOptionalJoins(string sql, object parameters)
        {
            var joinTerms = parameters.GetType().GetProperties()
               .Where(pi => pi.HasAttribute<JoinAttribute>() && pi.GetValue(parameters).Equals(true))
               .Select(pi => pi.GetAttribute<JoinAttribute>().Sql);

            return sql.Replace(JoinToken, string.Join("\r\n", joinTerms));
        }

        private static string ResolveOrderBy(string sql, object parameters, string typeName)
        {
            if (sql.Contains(OrderByToken))
            {
                var orderByProp = parameters.GetType().GetProperties()
                   .FirstOrDefault(pi => pi.HasAttribute<OrderByAttribute>() && HasValue(pi, parameters));

                if (orderByProp == null)
                {
                    throw new Exception($"Query {typeName} has an {{orderBy}} token, but no corresponding property with the [OrderBy] attribute.");
                }

                var value = orderByProp.GetValue(parameters);
                var sortOptions = orderByProp.GetCustomAttributes<OrderByAttribute>();
                var selectedSort = sortOptions.FirstOrDefault(a => a.Value.Equals(value));

                if (selectedSort == null) throw new Exception($"Query order by property {orderByProp.Name} had no matching case for value {value}");

                return sql.Replace(OrderByToken, selectedSort.Expression);
            }

            return sql;
        }

        private static string ResolveInlineOptionalCriteria(string input, object parameters, QueryParameters paramInfo)
        {
            string result = input;
            foreach (var optional in paramInfo.Optional)
            {
                if (AllParametersSet(parameters, optional.ParameterNames))
                {
                    result = result.Replace(optional.Token, optional.Content);
                }
                else
                {
                    // if the param object does not specify a property for this token, then remove the token from the SQL
                    result = result.Replace(optional.Token, string.Empty);
                }
            }
            return result;
        }

        private static bool AllParametersSet(object parameters, string[] parameterNames)
        {
            var properties = parameters.GetType().GetProperties();

            var paramPropertyMap =
                (from name in parameterNames
                 join pi in properties on name.ToLower() equals pi.Name.ToLower()
                 select new
                 {
                     Name = name.ToLower(),
                     PropertyInfo = pi
                 }).ToDictionary(row => row.Name, row => row.PropertyInfo);

            return parameterNames.All(p => paramPropertyMap.ContainsKey(p.ToLower()) && HasValue(paramPropertyMap[p.ToLower()], parameters));
        }

        private static bool HasValue(PropertyInfo propertyInfo, object @object, out object value)
        {
            value = propertyInfo.GetValue(@object);

            if (value != null)
            {
                if (propertyInfo.HasAttribute(out NullWhenAttribute attr))
                {
                    if (attr.NullValues?.Contains(value) ?? false) return false;
                }

                if (value.Equals(string.Empty)) return false;
                return true;
            }

            return false;
        }

        private static bool HasValue(PropertyInfo propertyInfo, object @object)
        {
            return HasValue(propertyInfo, @object, out object value);
        }

        private static DynamicParameters GetDynamicParameters(IEnumerable<PropertyInfo> properties, object parameters)
        {
            var result = new DynamicParameters();
            foreach (var prop in properties)
            {
                if (HasValue(prop, parameters, out object value) && !prop.HasAttribute<PhraseAttribute>()) result.Add(prop.Name, value);
            }
            return result;
        }

        /// <summary>
        /// Returns the properties of a query object based on parameters defined in a
        /// SQL statement as well as properties with Where and Case attributes
        /// </summary>
        private static IEnumerable<PropertyInfo> GetParamProperties(object parameters, string sql, out QueryParameters paramInfo)
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
    }
}
