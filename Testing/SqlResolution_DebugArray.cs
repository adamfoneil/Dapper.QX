using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Testing.Queries;

namespace Testing
{
    public partial class SqlResolution
    {
        [TestMethod]
        public void DebugSqlIntArray()
        {
            QueryHelper.GenerateDebugSql = true;

            try
            {
                var qry = new IntArrayParamQuery() { IdList = new int[] { 1, 2, 3 }, SomethingElseId = 23 };

                qry.ResolveSql();
                Assert.IsTrue(qry.DebugSql.ReplaceWhitespace().Equals(
                    @"DECLARE @SomethingElseId int;
                    SET @SomethingElseId = 23;
                    SELECT [Id] FROM [Whatever] WHERE [Id] IN (1, 2, 3) AND [SomethingElseId]=@somethingElseId ORDER BY [Yes]".ReplaceWhitespace()));
            }
            finally
            {
                QueryHelper.GenerateDebugSql = ExecutionSqlServer.ShouldDebugSql;
            }
        }
    }
}
