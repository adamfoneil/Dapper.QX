using Dapper.QX.Classes;
using Dapper.QX.Exceptions;
using Dapper.QX.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dapper.QX
{
    public partial class Query<TResult>
    {
        public Query(string sql)
        {
            Sql = sql;
        }

        public string Sql { get; }
        public string ResolvedSql { get; private set; }
        public DynamicParameters Parameters { get; private set; }

        public string ResolveSql()
        {
            ResolvedSql = QueryHelper.ResolveSql(Sql, this, out DynamicParameters queryParams);
            Parameters = queryParams;
            return ResolvedSql;
        }

        public string ResolveSql(out DynamicParameters queryParams)
        {
            ResolvedSql = QueryHelper.ResolveSql(Sql, this, out queryParams);
            Parameters = queryParams;
            return ResolvedSql;
        }

        public async Task<IEnumerable<TResult>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Enumerable = await connection.QueryAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                });

            return result.Enumerable;
        }

        public async Task<TResult> ExecuteSingleAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                });

            return result.Single;
        }

        public async Task<TResult> ExecuteSingleOrDefaultAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                });

            return result.Single;

        }

        private async Task<DapperResult<T>> ExecuteInnerAsync<T>(Func<string, object, Task<DapperResult<T>>> dapperMethod)
        {
            ResolvedSql = QueryHelper.ResolveSql(Sql, this, out DynamicParameters queryParams);
            Parameters = queryParams;

            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await dapperMethod.Invoke(ResolvedSql, queryParams);
            }
            catch (Exception exc)
            {
                throw new QueryException(exc, ResolvedSql, queryParams);
            }
            finally
            {
                stopwatch.Stop();
                var qt = new QueryTrace(GetType().Name, ResolvedSql, queryParams, stopwatch.Elapsed);
                OnQueryExecuted(qt);
                await OnQueryExecutedAsync(qt);
            }
        }

        /// <summary>
        /// Override this to capture information about a query execution in your application
        /// </summary>		
        protected virtual void OnQueryExecuted(QueryTrace queryTrace)
        {
        }

        /// <summary>
        /// Intended for implementing <see cref="Interfaces.ITestableQuery"/> for unit testing, not intended for use on its own
        /// </summary>
        public IEnumerable<dynamic> TestExecuteHelper(IDbConnection connection)
        {
            try
            {
                ResolvedSql = QueryHelper.ResolveSql(Sql, this, out DynamicParameters queryParams);
                return connection.Query(ResolvedSql, queryParams);
            }
            catch (Exception exc)
            {
                throw new Exception($"Query {GetType().Name} failed: {exc.Message}", exc);
            }
        }
    }
}
