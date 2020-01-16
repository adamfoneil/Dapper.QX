using System;

namespace Dapper.QX.Exceptions
{
    public class QueryException : Exception
    {
        public QueryException(Exception source, string sql, string debugSql, DynamicParameters parameters) : base(source.Message, source)
        {
            Sql = sql;
            DebugSql = debugSql;
            Parameters = parameters;
        }

        public string Sql { get; }
        public string DebugSql { get; }
        public DynamicParameters Parameters { get; }
    }
}
