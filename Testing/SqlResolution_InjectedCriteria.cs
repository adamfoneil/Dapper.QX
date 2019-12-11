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
            var query = new Yimba();
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Id] FROM [SampleTable]"));            
        }

        [TestMethod]
        public void QueryInjectCriteria()
        {
            var query = new Yimba() { FirstNameLike = "arxo" };
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Id] FROM [SampleTable] WHERE [FirstName] LIKE '%'+@firstNameLike+'%'"));
        }
    }
}
