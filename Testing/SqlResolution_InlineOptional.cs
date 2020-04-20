using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Testing.Queries;

namespace Testing
{
    [TestClass]
    public partial class SqlResolution
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

        [TestMethod]
        public void QueryWithInlineOptionalRemoved()
        {
            string query = QueryHelper.ResolveSql(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    [yula]=@thurgood
                    [[ AND [monster]=@graviton ]]", new { quark = "yardis" });

            Assert.IsTrue(query.Equals(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    [yula]=@thurgood"));
        }

        [TestMethod]
        public void QueryWithInlineOptionalMultiParams()
        {
            string query = QueryHelper.ResolveSql(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    1 = 1
                    [[ AND [SomeDate] BETWEEN @start AND @end ]]", new { start = DateTime.Now, end = DateTime.Now.AddDays(30) });

            Assert.IsTrue(query.Equals(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    1 = 1
                    AND [SomeDate] BETWEEN @start AND @end"));
        }

        [TestMethod]
        public void QueryWithInlineOptionalMultiPartial()
        {
            string query = QueryHelper.ResolveSql(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    1 = 1
                    [[ AND [SomeDate] BETWEEN @start AND @end ]]", new { start = DateTime.Now });

            Assert.IsTrue(query.Equals(
                @"SELECT 
                    [this], [that], [other]
                FROM 
                    [whatever]
                WHERE
                    1 = 1")); // since end param is missing, the optional part is omitted
        }
    }
}
