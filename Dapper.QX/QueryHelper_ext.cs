using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dapper.QX
{
	public static partial class QueryHelper
	{
		public static async Task<IEnumerable<T>> ResolveQueryAsync<T>(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, int newPageSize = 0)
		{
			return await connection.QueryAsync<T>(ResolveSql(sql, param, newPageSize), param, transaction, commandTimeout, commandType);
		}

		public static void Test<TQuery>(Func<IDbConnection> getConnection) where TQuery : ITestableQuery, new()
		{
			var qry = new TQuery();
			Test(qry, getConnection);
		}

		public static void Test<TQuery>(TQuery query, Func<IDbConnection> getConnection) where TQuery : ITestableQuery
		{
			using (var cn = getConnection.Invoke())
			{
				foreach (var testCase in query.GetTestCases())
				{
					try
					{
						testCase.TestExecute(cn);
						Debug.Print(testCase.ResolvedSql);
					}
					catch
					{
						Debug.Print(testCase.ResolvedSql);
						throw;
					}
				}
			}
		}
	}
}
