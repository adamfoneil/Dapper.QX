using Dapper.QX.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Dapper.QX.Abstract
{
	public abstract class TestableQuery<TResult> : Query<TResult>, ITestableQuery
	{
		public TestableQuery(string sql) : base(sql)
		{
		}

		protected abstract IEnumerable<ITestableQuery> GetTestCasesInner();

		public IEnumerable<dynamic> TestExecute(IDbConnection connection) => TestExecuteHelper(connection);

		public IEnumerable<ITestableQuery> GetTestCases() => GetTestCasesInner();
	}
}
