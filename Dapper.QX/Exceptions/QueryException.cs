using System;

namespace Dapper.QX.Exceptions
{
    public class QueryException : Exception
    {
        public QueryException(Exception source, string sql, string debugSql, DynamicParameters parameters, Type sourceType) : base($"{source.Message} in {sourceType.Name}", source)
        {
            Sql = sql;
            DebugSql = debugSql;
            Parameters = parameters;
            QueryType = sourceType;
        }

        public Type QueryType { get; }
        public string Sql { get; }
        public string DebugSql { get; }
        public DynamicParameters Parameters { get; }
    }
}
