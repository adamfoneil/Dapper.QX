using Dapper.QX.Classes;
using Dapper.QX.Exceptions;
using Dapper.QX.Extensions;
using Dapper.QX.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.QX
{
    public partial class Query<TResult>
    {
        public Query(string sql)
        {
            Sql = sql;
            DynamicParameters = new Dictionary<string, object>();
        }

        public string Sql { get; }
        public string ResolvedSql { get; private set; }
        public string DebugSql { get; private set; }
        public DynamicParameters Parameters { get; private set; }

        /// <summary>
        /// Passes parameters to the query that can't be modeled as properties
        /// </summary>
        public Dictionary<string, object> DynamicParameters { get; set; }

        public string ResolveSql()
        {
            return ResolveSql(out _);
        }

        public string ResolveSql(out DynamicParameters queryParams)
        {
            ResolvedSql = QueryHelper.ResolveSql(Sql, this, out queryParams);
            AddDynamicParams(queryParams);
            DebugSql = QueryHelper.ResolveParams(this, queryParams) + "\r\n\r\n" + DebugResolveArrays(ResolvedSql);
            Parameters = queryParams;
            return ResolvedSql;
        }

        private void AddDynamicParams(DynamicParameters queryParams)
        {
            if (DynamicParameters != null)
            {
                foreach (var kp in DynamicParameters)
                {
                    var pv = kp.Value as ParamValue;
                    if (pv != null)
                    {
                        queryParams.Add(kp.Key, pv.Value, pv.Type, pv.Direction, pv.Size, pv.Precision, pv.Scale);
                    }
                    else
                    {
                        queryParams.Add(kp.Key, kp.Value);
                    }
                }
            }
        }

        public async Task<IEnumerable<TResult>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Enumerable = await connection.QueryAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces);

            return result.Enumerable;
        }

        public async Task<TResult> ExecuteSingleAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces);

            return result.Single;
        }

        public async Task<TResult> ExecuteSingleOrDefaultAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null)
        {
            var result = await ExecuteInnerAsync(
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces);
            
            return result.Single;
        }

        private async Task<DapperResult<T>> ExecuteInnerAsync<T>(Func<string, object, Task<DapperResult<T>>> dapperMethod, List<QueryTrace> traces = null)
        {
            ResolveSql(out DynamicParameters queryParams);            

            try
            {
                Debug.Print(DebugSql);

                var stopwatch = Stopwatch.StartNew();
                var result = await dapperMethod.Invoke(ResolvedSql, queryParams);
                stopwatch.Stop();                

                var qt = new QueryTrace(GetType().Name, ResolvedSql, DebugSql, queryParams, stopwatch.Elapsed);
                OnQueryExecuted(qt);
                await OnQueryExecutedAsync(qt);
                traces?.Add(qt);

                return result;
            }
            catch (Exception exc)
            {                
                throw new QueryException(exc, ResolvedSql, DebugSql, queryParams);
            }            
        }

        private string DebugResolveArrays(string resolvedSql)
        {
            string result = resolvedSql;

            // todo: add string delimiter, ensure pi.GetValue works with string.Join

            try
            {
                var props = this.GetType().GetProperties().Where(pi => pi.PropertyType.IsArray).Select(pi =>
                {
                    var values = pi.GetValue(this) as int[];
                    return new
                    {
                        Token = " IN @" + pi.Name.ToLower(),
                        ValueList = " IN (" + string.Join(", ", values) + ")"
                    };
                });

                foreach (var p in props)
                {
                    result = Regex.Replace(result, p.Token, p.ValueList, RegexOptions.IgnoreCase);                        
                }

                return result;
            }
            catch 
            {
                // if any error, just give me what I started with
                return resolvedSql;
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
