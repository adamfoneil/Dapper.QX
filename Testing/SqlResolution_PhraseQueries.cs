using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Testing.Queries;

namespace Testing
{
    public partial class SqlResolution
    {
        [TestMethod]
        public void PhraseQuerySqlWordsOnly()
        {
            var qry = new PhraseQueryTest() { Search = "this that other" };
            qry.ResolveSql();
            string sql = qry.ResolvedSql;
            Assert.IsTrue(sql.Equals(@"SELECT * FROM [Employee] WHERE (([FirstName] LIKE '%' + @Search1 + '%' AND [FirstName] LIKE '%' + @Search2 + '%' AND [FirstName] LIKE '%' + @Search3 + '%') OR ([LastName] LIKE '%' + @Search1 + '%' AND [LastName] LIKE '%' + @Search2 + '%' AND [LastName] LIKE '%' + @Search3 + '%') OR ([Email] LIKE '%' + @Search1 + '%' AND [Email] LIKE '%' + @Search2 + '%' AND [Email] LIKE '%' + @Search3 + '%') OR ([Notes] LIKE '%' + @Search1 + '%' AND [Notes] LIKE '%' + @Search2 + '%' AND [Notes] LIKE '%' + @Search3 + '%'))"));
            Assert.IsTrue(qry.Parameters.ParameterNames.SequenceEqual(new string[] { "Search1", "Search2", "Search3" }));
            Assert.IsTrue(qry.Parameters.Get<string>("Search1").Equals("this"));
            Assert.IsTrue(qry.Parameters.Get<string>("Search2").Equals("that"));
            Assert.IsTrue(qry.Parameters.Get<string>("Search3").Equals("other"));
        }

        [TestMethod]
        public void PhraseQueryQuoted()
        {
            var qry = new PhraseQueryTest() { Search = "\"hello kitty\" yes" };
            qry.ResolveSql();
            string sql = qry.ResolvedSql;
            Assert.IsTrue(sql.Equals(@"SELECT * FROM [Employee] WHERE (([FirstName] LIKE '%' + @Search1 + '%' AND [FirstName] LIKE '%' + @Search2 + '%') OR ([LastName] LIKE '%' + @Search1 + '%' AND [LastName] LIKE '%' + @Search2 + '%') OR ([Email] LIKE '%' + @Search1 + '%' AND [Email] LIKE '%' + @Search2 + '%') OR ([Notes] LIKE '%' + @Search1 + '%' AND [Notes] LIKE '%' + @Search2 + '%'))"));
            Assert.IsTrue(qry.Parameters.ParameterNames.SequenceEqual(new string[] { "Search1", "Search2" }));
            Assert.IsTrue(qry.Parameters.Get<string>("Search1").Equals("hello kitty"));
            Assert.IsTrue(qry.Parameters.Get<string>("Search2").Equals("yes"));
        }

        [TestMethod]
        public void PhraseQueryNegated()
        {
            var qry = new PhraseQueryTest() { Search = "\"hello kitty\" -yes" };
            qry.ResolveSql();
            string sql = qry.ResolvedSql;
            Assert.IsTrue(sql.Equals(@"SELECT * FROM [Employee] WHERE (([FirstName] LIKE '%' + @Search1 + '%' AND [FirstName] NOT LIKE '%' + @Search2 + '%') OR ([LastName] LIKE '%' + @Search1 + '%' AND [LastName] NOT LIKE '%' + @Search2 + '%') OR ([Email] LIKE '%' + @Search1 + '%' AND [Email] NOT LIKE '%' + @Search2 + '%') OR ([Notes] LIKE '%' + @Search1 + '%' AND [Notes] NOT LIKE '%' + @Search2 + '%'))"));
            Assert.IsTrue(qry.Parameters.ParameterNames.SequenceEqual(new string[] { "Search1", "Search2" }));
            Assert.IsTrue(qry.Parameters.Get<string>("Search1").Equals("hello kitty"));
            Assert.IsTrue(qry.Parameters.Get<string>("Search2").Equals("yes"));
        }

        [TestMethod]
        public void PhraseQuerySqlInjectedWildcards()
        {
            // for SqlCe, but without an actual CE connection

            var qry = new PhraseQueryTestCe() { Search = "\"hello kitty\" -yes" };
            qry.ResolveSql();
            string sql = qry.ResolvedSql;
            Assert.IsTrue(sql.Equals(@"SELECT * FROM [Employee] WHERE (([FirstName] LIKE @Search1 AND [FirstName] NOT LIKE @Search2) OR ([LastName] LIKE @Search1 AND [LastName] NOT LIKE @Search2) OR ([Email] LIKE @Search1 AND [Email] NOT LIKE @Search2) OR ([Notes] LIKE @Search1 AND [Notes] NOT LIKE @Search2))"));
            Assert.IsTrue(qry.Parameters.ParameterNames.SequenceEqual(new string[] { "Search1", "Search2" }));
            Assert.IsTrue(qry.Parameters.Get<string>("Search1").Equals("%hello kitty%"));
            Assert.IsTrue(qry.Parameters.Get<string>("Search2").Equals("%yes%"));
        }

    }
}
