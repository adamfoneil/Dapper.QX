using Dapper.QX.Attributes;
using System;
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

		public async Task<int> GetPageCountAsync(Func<IDbConnection> getConnection, int newPageSize = 0)
		{
			using (var cn = getConnection.Invoke())
			{
				return await GetPageCountAsync(cn, newPageSize);
			}
		}

		public async Task<int> GetPageCountAsync(IDbConnection connection, int newPageSize = 0)
		{
			var metrics = await GetMetricsAsync(connection, newPageSize);
			return metrics.Pages;
		}

		public async Task<ResultMetrics> GetMetricsAsync(Func<IDbConnection> getConnection, int newPageSize = 0)
		{
			using (var cn = getConnection.Invoke())
			{
				return await GetMetricsAsync(cn, newPageSize);
			}
		}

		public async Task<ResultMetrics> GetMetricsAsync(IDbConnection connection, int newPageSize = 0)
		{
			int pageSize = (newPageSize > 0) ? newPageSize : DefaultPageSize;

			var currentPage = this.Page;

			this.Page = null;
			var results = await ExecuteAsync(connection, newPageSize: newPageSize);
			int count = results.Count();

			this.Page = currentPage;

			return new ResultMetrics()
			{
				Rows = count,
				Pages = (count / pageSize) + (((count % pageSize) > 0) ? 1 : 0)
			};
		}

		public class ResultMetrics
		{
			public int Pages { get; set; }
			public int Rows { get; set; }
		}
	}
}
