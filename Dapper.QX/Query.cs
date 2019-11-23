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

    }
}
