using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testing.Queries;

namespace Testing
{
    public partial class SqlResolution
    {
        [TestMethod]
        public void QueryInjectCriteriaNone()
        {
            var query = new TypicalQuery();
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable]  ORDER BY [FirstName]"));
        }

        [TestMethod]
        public void QueryInjectCriteria()
        {
            var query = new TypicalQuery() { FirstNameLike = "arxo" };
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [FirstName] LIKE '%'+@firstNameLike+'%' ORDER BY [FirstName]"));
        }
    }
}
