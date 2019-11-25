using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class SqlResolve
    {
        [TestMethod]
        public void QueryWithInlineOptionalCriteria()
        {
            string query = QueryHelper.ResolveSql(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    [yula]=@thurgood
                    [[ AND [monster]=@graviton ]]", new { graviton = "yardis" });

            Assert.IsTrue(query.Equals(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    [yula]=@thurgood
                    AND [monster]=@graviton"));
        }
    }
}
