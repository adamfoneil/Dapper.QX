using System;

namespace Dapper.QX.Exceptions
{
    public class QueryException : Exception
    {
        public QueryException(Exception source, string sql, DynamicParameters parameters) : base(source.Message, source)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public string Sql { get; }
        public DynamicParameters Parameters { get; }
    }
}
