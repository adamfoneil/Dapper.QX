using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.QX
{
    public class Query<TResult>
    {
        public Query(string sql)
        {
            Sql = sql;
        }

        public string Sql { get; }
        public string ResolvedSql { get; private set; }
        public DynamicParameters Parameters { get; private set; }

        /// <summary>
        /// Intended for implementing <see cref="Interfaces.ITestableQuery"/> for unit testing, not intended for use on its own
        /// </summary>
        public IEnumerable<dynamic> TestExecuteHelper(IDbConnection connection)
        {
            try
            {
                ResolvedSql = QueryUtil.ResolveSql(Sql, this, out DynamicParameters queryParams);
                return connection.Query(ResolvedSql, queryParams);
            }
            catch (Exception exc)
            {
                throw new Exception($"Query {GetType().Name} failed: {exc.Message}", exc);
            }
        }
    }
}
