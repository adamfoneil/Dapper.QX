using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlIntegration.Library.Extensions;
using Testing.Queries;

namespace Testing
{    
	public partial class SqlResolution
	{
		[TestMethod]
		public void OptionalJoin()
		{
			var query = new TypicalQuery() { WithTable2Join = true };
			var sql = QueryHelper.ResolveSql(query.Sql, query, removeMacros: true);
			Assert.AreEqual(@"SELECT  [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] INNER JOIN [SampleTable2] ON [SampleTable].[Id] = [SampleTable2].[SampleId]   ORDER BY [FirstName]", sql);
		}

		[TestMethod]
		public void OptionalJoinWithParam()
		{
			var query = new TypicalQuery() { JoinWithParam = 1 };
			var sql = QueryHelper.ResolveSql(query.Sql, query, removeMacros: true);
			Assert.AreEqual(@"SELECT  [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] INNER JOIN [SampleTable3] ON [SampleTable].[Id] = [SampleTable3].[SampleId] AND [SampleTable3]=@joinWithParam   ORDER BY [FirstName]", sql);
		}

		[TestMethod]
		public void TvpJoinProperty()
		{
			var qry = new TestableQuerySample() { JoinIds = new int[] { 1, 2, 3 }.ToDataTable() };
			var sql = QueryHelper.ResolveSql(qry.Sql, qry, removeMacros: true);
			Assert.AreEqual(@"SELECT  [FirstName], [Weight], [SomeDate], [Notes], [SampleTable].[Id] FROM [SampleTable] INNER JOIN @joinIds AS [j] ON [SampleTable].[Id] = [j].[Id]  ORDER BY [FirstName]", sql);
		}
	}
}
