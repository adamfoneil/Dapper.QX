using Dapper.QX.Classes;
using Dapper.QX.Exceptions;
using Dapper.QX.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Dapper.QX
{
    public partial class Query<TResult>
    {
        public IEnumerable<TResult> Execute(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Enumerable = connection.Query<TResult>(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType)
                    };
                }, logger, setParams, newPageSize);

            return result.Enumerable;
        }

        public TResult ExecuteSingle(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = connection.QuerySingle<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, logger, setParams);

            return result.Single;
        }

        public TResult ExecuteSingleOrDefault(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, ILogger logger = null, Action<DynamicParameters> setParams = null)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = connection.QuerySingleOrDefault<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, logger, setParams);

            return result.Single;
        }

        private DapperResult<T> ExecuteInner<T>(Func<string, object, DapperResult<T>> dapperMethod, ILogger logger = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
        {
            ResolveSql(out DynamicParameters queryParams, setParams, newPageSize, removeMacros: true);

            try
            {
                Debug.Print(DebugSql);
                logger?.LogDebug(DebugSql);

                var stopwatch = Stopwatch.StartNew();
                var result = dapperMethod.Invoke(ResolvedSql, queryParams);
                stopwatch.Stop();                

                return result;
            }
            catch (Exception exc)
            {
                var qryExc = new QueryException(exc, ResolvedSql, DebugSql, queryParams);
                logger?.LogError(qryExc, exc.Message);
                throw qryExc;
            }
        }
    }
}
