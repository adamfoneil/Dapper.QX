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
        }

        public string Sql { get; }
        public string ResolvedSql { get; private set; }
        public string DebugSql { get; private set; }
        public DynamicParameters Parameters { get; private set; }

        public string ResolveSql(int newPageSize = 0, bool removeMacros = false)
        {
            return ResolveSql(out _, newPageSize: newPageSize, removeMacros: removeMacros);
        }

        public string ResolveSql(out DynamicParameters queryParams, Action<DynamicParameters> setParams = null, int newPageSize = 0, bool removeMacros = false)
        {
            ResolvedSql = QueryHelper.ResolveSql(Sql, this, out queryParams, newPageSize, removeMacros);
            setParams?.Invoke(queryParams);            
            DebugSql = QueryHelper.ResolveParams(this, queryParams) + "\r\n\r\n" + DebugResolveArrays(ResolvedSql);
            Parameters = queryParams;
            return ResolvedSql;
        }

        public async Task<IEnumerable<TResult>> ExecuteAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
        {
            var result = await ExecuteInnerAsync(connection,
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Enumerable = await connection.QueryAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces, setParams, newPageSize);

            return result.Enumerable;
        }

        public async Task<TResult> ExecuteSingleAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null, Action<DynamicParameters> setParams = null)
        {
            var result = await ExecuteInnerAsync(connection,
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces, setParams);

            return result.Single;
        }

        public async Task<TResult> ExecuteSingleOrDefaultAsync(IDbConnection connection, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null, List<QueryTrace> traces = null, Action<DynamicParameters> setParams = null)
        {
            var result = await ExecuteInnerAsync(connection,
                async (string sql, object param) =>
                {
                    return new DapperResult<TResult>()
                    {
                        Single = await connection.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType)
                    };
                }, traces, setParams);
            
            return result.Single;
        }

        private async Task<DapperResult<T>> ExecuteInnerAsync<T>(IDbConnection connection, Func<string, object, Task<DapperResult<T>>> dapperMethod, List<QueryTrace> traces = null, Action<DynamicParameters> setParams = null, int newPageSize = 0)
        {
            ResolveSql(out DynamicParameters queryParams, setParams, newPageSize);

            var macros = RegexHelper.ParseMacros(ResolvedSql);

#if NETSTANDARD2_0
            var macroInserts = await ResolveMacrosAsync(connection, macros);

            if (macroInserts.inserts.Any())
            {
                queryParams.AddDynamicParams(macroInserts.parameters);
                foreach (var macro in macroInserts.inserts)
                {
                    ResolvedSql = ResolvedSql.Replace(macro.Key, macro.Value);
                    DebugSql = DebugSql.Replace(macro.Key, macro.Value);
                }
            }
#endif            

            try
            {                
                Debug.Print(DebugSql);

                var stopwatch = Stopwatch.StartNew();

#if NETSTANDARD2_0
                await OnExecutingAsync(connection, queryParams);
#endif
                var result = await dapperMethod.Invoke(ResolvedSql, queryParams);
                stopwatch.Stop();                

                var qt = new QueryTrace(GetType().Name, ResolvedSql, DebugSql, queryParams, stopwatch.Elapsed);
                OnQueryExecuted(qt);

#if NETSTANDARD2_0
                await OnQueryExecutedAsync(connection, qt);
#endif
                traces?.Add(qt);

                return result;
            }
            catch (Exception exc)
            {                
                throw new QueryException(exc, ResolvedSql, DebugSql, queryParams);
            }            
        }

#if NETSTANDARD2_0
        protected virtual async Task<(Dictionary<string, string> inserts, DynamicParameters parameters)> ResolveMacrosAsync(IDbConnection connection, IEnumerable<string> macros) => await Task.FromResult((macros.ToDictionary(m => m, m => string.Empty), new DynamicParameters()));

        protected virtual async Task OnExecutingAsync(IDbConnection connection, DynamicParameters parameters) => await Task.CompletedTask;
#endif

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
                ResolvedSql = QueryHelper.ResolveSql(Sql, this, out DynamicParameters queryParams, removeMacros: true);
                return connection.Query(ResolvedSql, queryParams);
            }
            catch (Exception exc)
            {
                throw new Exception($"Query {GetType().Name} failed: {exc.Message}", exc);
            }
        }
    }
}
