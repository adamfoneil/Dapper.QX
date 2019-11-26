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
    }
}
