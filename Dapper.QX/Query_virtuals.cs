using Dapper.QX.Models;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.QX
{
    public partial class Query<TResult>
    {
#if NETSTANDARD2_0
        /// <summary>
        /// Override this to capture information about a query execution in your application
        /// </summary>
        protected virtual async Task OnQueryExecutedAsync(IDbConnection connection, QueryTrace queryTrace)
        {
            await Task.CompletedTask;
        }
#endif
    }
}
