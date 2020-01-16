using System;

namespace Dapper.QX.Models
{
    public class QueryTrace
    {
        public QueryTrace(string queryName, string sql, string debugSql, DynamicParameters parameters, TimeSpan duration)
        {
            QueryName = queryName;
            Sql = sql;
            DebugSql = debugSql;
            Parameters = parameters;
            Duration = duration;
        }

        public string QueryName { get; }
        public string Sql { get; }
        public string DebugSql { get; }
        public DynamicParameters Parameters { get; }
        public TimeSpan Duration { get; }
    }
}
