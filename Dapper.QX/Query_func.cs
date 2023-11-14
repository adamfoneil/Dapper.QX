using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.QX
{
	public partial class Query<TResult>
	{
		public async Task<IEnumerable<TResult>> ExecuteAsync(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
		{
			using (var cn = getConnection.Invoke())
			{
				return await ExecuteAsync(cn, null, commandTimeout, commandType, logger, setParams, newPageSize);
			}
		}

		public async Task<TResult> ExecuteSingleAsync(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
		{
			using (var cn = getConnection.Invoke())
			{
				return await ExecuteSingleAsync(cn, null, commandTimeout, commandType, logger, setParams);
			}
		}

		public async Task<TResult> ExecuteSingleOrDefaultAsync(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
		{
			using (var cn = getConnection.Invoke())
			{
				return await ExecuteSingleOrDefaultAsync(cn, null, commandTimeout, commandType, logger, setParams);
			}
		}

		public IEnumerable<TResult> Execute(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
		{
			using (var cn = getConnection.Invoke())
			{
				return Execute(cn, null, commandTimeout, commandType, logger, setParams, newPageSize);
			}
		}

		public TResult ExecuteSingle(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
		{
			using (var cn = getConnection.Invoke())
			{
				return ExecuteSingle(cn, null, commandTimeout, commandType, logger, setParams);
			}
		}

		public TResult ExecuteSingleOrDefault(Func<IDbConnection> getConnection, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
		{
			using (var cn = getConnection.Invoke())
			{
				return ExecuteSingleOrDefault(cn, null, commandTimeout, commandType, logger, setParams);
			}
		}
	}
}
