using Dapper.QX.Attributes;
using Dapper.QX.Classes;
using Dapper.QX.Extensions;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Dapper.QX
{
	public static partial class QueryHelper
	{
		private const string WhereInner = "where";
		private const string AndWhereInner = "andWhere";

		public const string OrderByToken = "{orderBy}";
		public const string JoinToken = "{join}";
		public const string WhereToken = "{" + WhereInner + "}";
		public const string AndWhereToken = "{" + AndWhereInner + "}";
		public const string OffsetToken = "{offset}";
		public const string TopToken = "{top}";

		public const string GlobalScope = "global";

		public static bool GenerateDebugSql { get; set; } = true;

		public static string ResolveSql(string sql, object parameters, int newPageSize = 0, bool removeMacros = false)
		{
			return ResolveSql(sql, parameters, out _, newPageSize, removeMacros);
		}

		public static string ResolveSql(string sql, object parameters, out DynamicParameters dynamicParams, int newPageSize = 0, bool removeMacros = false)
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

			string result = (parameters is ISelfModifyingQuery qry) ? qry.BuildSql(sql) : sql;

			var properties = GetParamProperties(parameters, sql, out QueryParameters paramInfo);
			dynamicParams = GetDynamicParameters(properties, parameters);

			string queryTypeName = parameters.GetType().Name;

			result = ResolveInlineOptionalCriteria(result, parameters, paramInfo);
			result = ResolveOrderBy(result, parameters, queryTypeName);
			result = ResolveTopClause(result, parameters, dynamicParams);
			result = ResolveOptionalJoins(result, parameters, dynamicParams);
			result = ResolveInjectedCriteria(result, paramInfo, properties, parameters, dynamicParams);
			result = ResolveOffset(result, parameters, newPageSize);
			result = RegexHelper.RemovePlaceholders(result);

			// normally you would not remove macros because you had a reason for putting them there initially.
			// the only reason for this is to make tests work
			if (removeMacros)
			{
				result = RegexHelper.RemoveMacros(result);
			}

			return result.Trim();
		}

		private static string ResolveOffset(string sql, object parameters, int newPageSize = 0)
		{
			string result = sql;

			if (result.Contains(OffsetToken) && FindOffsetProperty(parameters, out PropertyInfo offsetProperty))
			{
				var offsetAttr = offsetProperty.GetCustomAttribute<OffsetAttribute>();
				int page = (int)offsetProperty.GetValue(parameters);
				result = result.Replace(OffsetToken, offsetAttr.GetOffsetFetchSyntax(page, newPageSize));
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

		private static string ResolveInjectedCriteria(string sql, QueryParameters paramInfo,
			IEnumerable<PropertyInfo> properties, object parameters, DynamicParameters queryParams)
		{
			var result = sql;
			var whereScopes = RegexHelper.GetWhereScopes(sql);

			// not supporting scoped phrase queries, so we have a marker that lets them be used once only
			bool usedPhraseAlready = false;

			var propertiesWithValues = properties
				.Where(pi => paramInfo.IsOptional(pi) && HasValue(pi, parameters, out _))
				.Select(pi => new
				{
					PropertyInfo = pi,
					Value = pi.GetValue(parameters)
				});

			foreach (var scope in whereScopes)
			{
				var terms = new List<string>();
				foreach (var p in propertiesWithValues)
				{
					if (GetCaseExpression(scope, p.PropertyInfo, p.Value, out string caseExpression))
					{
						terms.Add(caseExpression);
					}
					else if (GetWhereExpression(scope, p.PropertyInfo, out string whereExpression))
					{
						terms.Add(whereExpression);
					}
					else if (GetPhraseQuery(p.PropertyInfo, p.Value, out PhraseQuery phraseQuery) && !usedPhraseAlready)
					{
						queryParams.AddDynamicParams(phraseQuery.Parameters);
						terms.Add(phraseQuery.Expression);
						usedPhraseAlready = true;
					}
				}

				result = ResolveInjectedCriteria(result, scope, terms);
			}

			return result;
		}

		public static string ResolveInjectedCriteria(string sql, string scope, IEnumerable<string> terms)
		{
			var result = sql;

			// global scope has no prefix
			var insertScope = (scope.Equals(GlobalScope)) ? string.Empty : scope + ":";

			var tokens = new string[]
			{
				"{" + insertScope + WhereInner + "}",
				"{" + insertScope + AndWhereInner + "}"
			};

			var keywordOptions = new Dictionary<string, string>()
			{
				[tokens[0]] = "WHERE",
				[tokens[1]] = "AND"
			};

			while (result.ContainsAnyOf(tokens, out string token))
			{
				result = result.Replace(token, (terms?.Any() ?? false) ?
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

		private static bool GetWhereExpression(string scope, PropertyInfo pi, out string whereExpression)
		{
			WhereAttribute whereAttr = pi.GetCustomAttributes<WhereAttribute>().SingleOrDefault(attr => attr.Scope.Equals(scope));
			if (whereAttr != null)
			{
				whereExpression = whereAttr.Expression;
				return true;
			}

			whereExpression = null;
			return false;
		}

		private static bool GetCaseExpression(string scope, PropertyInfo pi, object value, out string caseExpression)
		{
			var cases = pi.GetCustomAttributes(typeof(CaseAttribute), false).OfType<CaseAttribute>().Where(attr => attr.Scope.Equals(scope));
			var selectedCase = cases?.FirstOrDefault(c => c.Value.Equals(value));
			if (selectedCase != null)
			{
				caseExpression = selectedCase.Expression;
				return true;
			}

			caseExpression = null;
			return false;
		}

		private static string ResolveOptionalJoins(string sql, object parameters, DynamicParameters dynamicParams)
		{
			var joinTerms = parameters.GetType().GetProperties()
			   .Where(pi => pi.HasAttribute<JoinAttribute>() && IncludeJoin(pi))
			   .Select(pi => pi.GetAttribute<JoinAttribute>().Sql)
			   .ToArray();
			
			var type = parameters.GetType();

			foreach (var term in joinTerms)
			{
				if (term.HasParameters(out var paramNames))
				{
					foreach (var name in paramNames)
					{
						var property = type.GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
						if (property != null)
						{
							var propertyValue = property.GetValue(parameters);
							if (propertyValue != null) dynamicParams.Add(name, propertyValue);
						}
					}
				}				
			}

			return sql.Replace(JoinToken, string.Join("\r\n", joinTerms));			

			bool IncludeJoin(PropertyInfo pi)
			{
				if (HasValue(pi, parameters, out var value))
				{
					if (pi.PropertyType.Equals(typeof(bool)))
					{
						return (bool)value;
					}

					return true;
				}

				return false;
			}
		}

		private static string ResolveTopClause(string sql, object parameters, DynamicParameters dynamicParams)
		{
			if (sql.Contains(TopToken))
			{
				var topProp = parameters.GetType().GetProperties()
				   .FirstOrDefault(pi => pi.HasAttribute<TopAttribute>() && HasValue(pi, parameters, out _));

				if (topProp is null) return sql;

				var value = topProp.GetValue(parameters);

				if (value != null)
				{
					var expression = $"TOP (@{topProp.Name})";

					sql = sql.Replace(TopToken, expression);
					dynamicParams.Add(topProp.Name, value);
				}
			}

			return sql;
		}

		private static string ResolveOrderBy(string sql, object parameters, string typeName)
		{
			if (sql.Contains(OrderByToken))
			{
				var orderByProp = parameters.GetType().GetProperties()
				   .FirstOrDefault(pi => pi.HasAttribute<OrderByAttribute>() && HasValue(pi, parameters, out _));

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

			return parameterNames.All(p => paramPropertyMap.ContainsKey(p.ToLower()) && HasValue(paramPropertyMap[p.ToLower()], parameters, out _));
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

		private static DynamicParameters GetDynamicParameters(IEnumerable<PropertyInfo> properties, object parameters)
		{
			var result = new DynamicParameters();

			foreach (var prop in properties)
			{
				if (HasValue(prop, parameters, out object value))
				{
					if (prop.PropertyType.Equals(typeof(DataTable)))
					{
						var typeName = GetTableTypeName(prop);
						result.Add(prop.Name, (value as DataTable).AsTableValuedParameter(typeName));
					}
					else if (!prop.HasAttribute<PhraseAttribute>())
					{
						result.Add(prop.Name, value);
					}
				}
			}

			return result;

			string GetTableTypeName(PropertyInfo prop)
			{
				var attr = prop.GetCustomAttribute<TableType>();
				return (attr != null) ? attr.TypeName : throw new Exception("DataTable properties require a [TableType] parameter.");
			}
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
				(pi.HasAttribute<JoinAttribute>() && pi.HasAttribute<TableType>()) ||
				allParams.Contains(pi.Name.ToLower()));

			return queryProps;
		}
	}
}
