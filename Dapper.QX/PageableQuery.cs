using Dapper.QX.Attributes;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.QX
{
    /// <summary>
    /// use this when you need to get the total number of pages in a query that uses pagination
    /// </summary>    
    public class PageableQuery<TResult> : Query<TResult>
    {
        public PageableQuery(string sql) : base(sql)
        {
        }

        [Offset(DefaultPageSize)]
        public int? Page { get; set; }

        protected const int DefaultPageSize = 30;

        public async Task<int> GetPageCountAsync(IDbConnection connection, int newPageSize = 0)
        {
            int pageSize = (newPageSize > 0) ? newPageSize : DefaultPageSize;

            this.Page = null;
            var results = await ExecuteAsync(connection, newPageSize: newPageSize);
            int count = results.Count();
            return (count / pageSize) + (((count % pageSize) > 0) ? 1 : 0);
        }
    }
}
