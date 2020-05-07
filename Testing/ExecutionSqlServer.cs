using AdamOneilSoftware;
using Dapper;
using Dapper.QX;
using Dapper.QX.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlIntegration.Library;
using SqlIntegration.Library.Classes;
using SqlIntegration.Library.Extensions;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Testing.Queries;

namespace Testing
{
    [TestClass]
    public class ExecutionSqlServer
    {
        private const string dbName = "DapperQX";

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            LocalDb.TryDropDatabase(dbName, out _);

            using (var cn = LocalDb.GetConnection(dbName, SampleObjects()))
            {                
                var tdg = new TestDataGenerator();
                tdg.Generate<TypicalQueryResult>(1000, (result) =>
                {
                    result.FirstName = tdg.Random(Source.FirstName);
                    result.SomeDate = tdg.RandomInRange(-1000, 1000, (i) => DateTime.Today.AddDays(i));
                    result.Weight = tdg.RandomInRange<decimal>(50, 150, (d) => d);
                    result.Notes = RandomPhrase(4, 8);
                }, (results) =>
                {
                    // AppVeyor seems to need this
                    if (cn.State == ConnectionState.Closed) cn.Open();

                    var dataTable = results.ToDataTable();
                    BulkInsert.ExecuteAsync(dataTable, cn as SqlConnection, DbObject.Parse("dbo.SampleTable"), 50, new BulkInsertOptions()
                    {
                        SkipIdentityColumn = "Id"
                    }).Wait();
                });
            }
        }

        private static string RandomPhrase(int minLength, int maxLength)
        {
            var allWords = new string[]
            {
                "whatever", "this", "that", "other", "clancy", "hemostat", "saving", "more",
                "lorem", "ipsum", "sum", "vortical", "snow", "indexes", "florian", "outreach",
                "erat", "aliquam", "elit", "amet", "nibh", "laoreet", "taxi", "evermore", "ensign",
                "easy", "fidget", "gargantuan", "larva", "mountain", "share", "grain", "nothing",
                "thorium", "uranium", "cobalt", "haul", "rollercoaster", "pronoun", "inject",
                "marrow", "ostium", "vice", "estuary", "pillbox", "derby", "shore"
            };

            var rnd = new Random();
            
            int length = rnd.Next(minLength, maxLength);
            List<string> words = new List<string>();
            for (int i = 0; i < length; i++) words.Add(allWords[rnd.Next(0, allWords.Length)]);

            return string.Join(" ", words);
        }

        private static IEnumerable<InitializeStatement> SampleObjects()
        {
            return new InitializeStatement[]
            {
                new InitializeStatement(
                    "dbo.SampleTable", "DROP TABLE %obj%", @"CREATE TABLE %obj% (
                        [FirstName] nvarchar(50) NOT NULL,
                        [Weight] decimal(5,2) NOT NULL,
                        [SomeDate] datetime NOT NULL,
                        [Notes] nvarchar(max) NULL,
                        [Id] int identity(1,1) PRIMARY KEY
                    )")
            };
        }

        [TestMethod]
        public void TypicalQuery()
        {
            QueryHelper.Test<TypicalQuery>(() => LocalDb.GetConnection(dbName));
        }       

        [TestMethod]
        public void QueryDynamicParams()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var qry = new DynamicQuery("[SomeDate]>@minDate") { FirstName = "peabody" };
                qry.DynamicParameters["minDate"] = new DateTime(1990, 1, 1);
                var results = qry.Execute(cn);
                Assert.IsTrue(qry.ResolvedSql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [SomeDate]>@minDate AND [FirstName] LIKE '%'+@firstName+'%' ORDER BY [FirstName]"));
                Assert.IsTrue(qry.DebugSql.Equals(
                    @"DECLARE @FirstName nvarchar(max), @minDate datetime;
SET @FirstName = 'peabody';
SET @minDate = '1/1/1990 12:00:00 AM';

SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [SomeDate]>@minDate AND [FirstName] LIKE '%'+@firstName+'%' ORDER BY [FirstName]"));
            }
        }
        
        [TestMethod]
        public void OffsetQueryAsync()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.ExecuteAsync(cn).Result;
                Assert.IsTrue(results.Count() == 20);
            }
        }

        [TestMethod]
        public void QueryWithTrace()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                List<QueryTrace> traces = new List<QueryTrace>();
                var results = new TypicalQuery().ExecuteAsync(cn, traces: traces).Result;
                Assert.IsTrue(traces.Count() == 1);
            }
        }

        [TestMethod]
        public void OffsetQuery()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.Execute(cn);
                Assert.IsTrue(results.Count() == 20);
            }
        }

        [TestMethod]
        public void PhraseQueryAsync()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var qry = new TypicalQuery() { NotesContain = "this whatever" };
                var results = qry.ExecuteAsync(cn).Result;
                Debug.Print(qry.ResolvedSql); // for my own curiosity when running locally
                Assert.IsTrue(results.Any());
            }
        }

        [TestMethod]
        public void PhraseQuery()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var qry = new TypicalQuery() { NotesContain = "this whatever" };
                var results = qry.Execute(cn);
                Debug.Print(qry.ResolvedSql); // for my own curiosity when running locally
                Assert.IsTrue(results.Any());
            }
        }

        [TestMethod]
        public void ParamDeclarations()
        {
            var qry = new TypicalQuery()
            {
                MinWeight = 12,
                MaxWeight = 36,
                FirstNameLike = "yohoo",
                MinDate = new DateTime(2020, 1, 15)
            };

            QueryHelper.ResolveSql(qry.Sql, qry, out DynamicParameters queryParms);
            var syntax = QueryHelper.ResolveParams(qry, queryParms);

            Assert.IsTrue(syntax.ReplaceWhitespace().Equals(
                @"DECLARE @MinWeight decimal, @MaxWeight decimal, @FirstNameLike nvarchar(max), @MinDate datetime;
                SET @MinWeight = 12;
                SET @MaxWeight = 36;
                SET @FirstNameLike = 'yohoo';
                SET @MinDate = '1/15/2020 12:00:00 AM';".ReplaceWhitespace()));
        }

        [TestMethod]
        public void DebugSql()
        {
            var qry = new TypicalQuery()
            {
                MinWeight = 9,
                MaxWeight = 56,
                FirstNameLike = "warbler",
                MinDate = new DateTime(2020, 1, 15)
            };            

            qry.ResolveSql();
            string debug = qry.DebugSql;
            Assert.IsTrue(debug.ReplaceWhitespace().Equals(
                @"DECLARE @MinWeight decimal, @MaxWeight decimal, @FirstNameLike nvarchar(max), @MinDate datetime;
                SET @MinWeight = 9;
                SET @MaxWeight = 56;
                SET @FirstNameLike = 'warbler';
                SET @MinDate = '1/15/2020 12:00:00 AM';

                SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [Weight]>=@minWeight AND [Weight]<=@maxWeight AND [FirstName] LIKE '%'+@firstNameLike+'%' AND [SomeDate]>=@minDate ORDER BY [FirstName]".ReplaceWhitespace()));      
        }
    }
}
