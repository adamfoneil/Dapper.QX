using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.QX
{
    public static partial class QueryHelper
    {
        public static async Task<IEnumerable<T>> ResolveQueryAsync<T>(this IDbConnection connection, string sql, object param, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return await connection.QueryAsync<T>(ResolveSql(sql, param), param, transaction, commandTimeout, commandType);
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
                    testCase.TestExecute(cn);
                }
            }
        }
    }
}
