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

				var supportedTypes = sqlTypes.Select(kp => kp.Key);
				Dictionary<string, TypeValue> paramInfo = GetParamInfo(queryParams, query, supportedTypes);

				string declare = "DECLARE " + string.Join(", ", paramInfo.Select(kp => $"@{kp.Key} {getSqlType(kp.Value.Type).Type}")) + ";";
				string set = string.Join("\r\n", paramInfo.Select(kp => $"SET @{kp.Key} = {getSqlType(kp.Value.Type).FormatValue(kp.Value.ValueLiteral)};"));

				return declare + "\r\n" + set;

			}
			catch (Exception exc)
			{
				return $"Error generating debug SQL: {exc.Message}";
			}
		}

		private static Dictionary<string, TypeValue> GetParamInfo(DynamicParameters queryParams, object query, IEnumerable<Type> supportedTypes)
		{
			var typeMap = from prop in query.GetType().GetProperties()
						  join param in queryParams.ParameterNames on prop.Name equals param
						  select new
						  {
							  Name = param,
							  Type = prop.PropertyType,
							  Value = prop.GetValue(query)?.ToString()
						  };

			var result = typeMap
				.Where(tm => supportedTypes.Contains(tm.Type))
				.ToDictionary(item => item.Name, item => new TypeValue()
				{
					Type = item.Type,
					ValueLiteral = item.Value
				});

			foreach (var paramName in queryParams.ParameterNames.Except(result.Keys))
			{
				if (discoverDynamicParamLiteral(paramName, out TypeValue typeValue)) result.Add(paramName, typeValue);
			}

			return result;

			// there's no way to reflect types used within DynamicParameters, 
			// so I have just try them all until one works
			bool discoverDynamicParamLiteral(string paramName, out TypeValue typeValue)
			{
				Dictionary<Type, Func<string>> calls = new Dictionary<Type, Func<string>>()
				{
					{ typeof(string), () => queryParams.Get<string>(paramName) },
					{ typeof(int), () => queryParams.Get<int>(paramName).ToString() },
					{ typeof(int?), () => queryParams.Get<int?>(paramName).ToString() },
					{ typeof(long), () => queryParams.Get<long>(paramName).ToString() },
					{ typeof(long?), () => queryParams.Get<long?>(paramName).ToString() },
					{ typeof(DateTime), () => queryParams.Get<DateTime>(paramName).ToString() },
					{ typeof(DateTime?), () => queryParams.Get<DateTime?>(paramName).ToString() },
					{ typeof(bool), () => queryParams.Get<bool>(paramName).ToString() },
					{ typeof(bool?), () => queryParams.Get<bool?>(paramName).ToString() },
					{ typeof(decimal), () => queryParams.Get<decimal>(paramName).ToString() },
					{ typeof(decimal?), () => queryParams.Get<decimal?>(paramName).ToString() }
				};

				foreach (var kp in calls)
				{
					try
					{
						typeValue = new TypeValue()
						{
							Type = kp.Key,
							ValueLiteral = calls[kp.Key].Invoke(),
							IsDynamic = true
						};
						return true;
					}
					catch
					{
						// do nothing, just try the next type
					}
				}

				typeValue = null;
				return false;
			}
		}
	}
}
