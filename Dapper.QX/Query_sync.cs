using Dapper.QX.Classes;
using Dapper.QX.Exceptions;
using Dapper.QX.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Dapper.QX
{
    public partial class Query<TResult>
    {
        public IEnumerable<TResult> Execute(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Enumerable = connection.Query<TResult>(sql, param, transaction, commandTimeout: commandTimeout, commandType: commandType)
                    };
                }, traces);

            return result.Enumerable;
        }

        public TResult ExecuteSingle(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = connection.QuerySingle<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces);

            return result.Single;
        }

        public TResult ExecuteSingleOrDefault(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = ExecuteInner(
                (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = connection.QuerySingleOrDefault<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces);

            return result.Single;
        }

        private DapperResult<T> ExecuteInner<T>(Func<string, object, DapperResult<T>> dapperMethod, List<QueryTrace> traces = null)
        {
            ResolveSql(out DynamicParameters queryParams);

            try
            {
                Debug.Print(DebugSql);

                var stopwatch = Stopwatch.StartNew();
                var result = dapperMethod.Invoke(ResolvedSql, queryParams);
                stopwatch.Stop();

                var qt = new QueryTrace(GetType().Name, ResolvedSql, DebugSql, queryParams, stopwatch.Elapsed);
                OnQueryExecuted(qt);
                traces?.Add(qt);

                return result;
            }
            catch (Exception exc)
            {
                throw new QueryException(exc, ResolvedSql, DebugSql, queryParams);
            }
        }
    }
}
