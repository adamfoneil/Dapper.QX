using AdamOneilSoftware;
using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlIntegration.Library;
using SqlIntegration.Library.Classes;
using SqlIntegration.Library.Extensions;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Models;
using System;
using System.Collections.Generic;
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
                }, async (results) =>
                {
                    var dataTable = results.ToDataTable();
                    await BulkInsert.ExecuteAsync(dataTable, cn as SqlConnection, DbObject.Parse("dbo.SampleTable"), 50, new BulkInsertOptions()
                    {
                        SkipIdentityColumn = "Id"
                    });
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
        public void OffsetQuery()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.ExecuteAsync(cn).Result;
                Assert.IsTrue(results.Count() == 20);
            }
        }

        [TestMethod]
        public void PhraseQuery()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var qry = new TypicalQuery() { NotesContain = "this whatever" };
                var results = qry.ExecuteAsync(cn).Result;
                Debug.Print(qry.ResolvedSql); // for my own curiosity when running locally
                Assert.IsTrue(results.Any());
            }
        }
    }
}
