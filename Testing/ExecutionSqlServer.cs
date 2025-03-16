using AdamOneilSoftware;
using Dapper;
using Dapper.QX;
using Dapper.QX.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServer.LocalDb;
using SqlServer.LocalDb.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Testing.Queries;

namespace Testing
{
    [TestClass]
    public class ExecutionSqlServer
    {
        private const string dbName = "DapperQX";

        public const bool ShouldDebugSql = false;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {            
            QueryHelper.GenerateDebugSql = ShouldDebugSql;

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

                    using var bulkInsert = new SqlBulkCopy(cn);
                    bulkInsert.DestinationTableName = "dbo.SampleTable";
                    bulkInsert.WriteToServer(dataTable);
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

        private static IEnumerable<InitializeStatement> SampleObjects() => new InitializeStatement[]
			{
				new(
					"dbo.SampleTable", "DROP TABLE %obj%", @"CREATE TABLE %obj% (
                        [FirstName] nvarchar(50) NOT NULL,
                        [Weight] decimal(5,2) NOT NULL,
                        [SomeDate] datetime NOT NULL,
                        [Notes] nvarchar(max) NULL,
                        [Id] int identity(1,1) PRIMARY KEY
                    )"),
				new("dbo.IdList", "DROP TYPE %obj%", @"CREATE TYPE %obj% AS TABLE ([Id] int NOT NULL PRIMARY KEY)"),
			};


		[TestMethod]
        public void TypicalQuery()
        {
            QueryHelper.Test<TypicalQuery>(() => LocalDb.GetConnection(dbName));
        }        

        [TestMethod]
        public void TypicalTestableQuery()
        {
            QueryHelper.Test<TestableQuerySample>(() => LocalDb.GetConnection(dbName));
        }

        [TestMethod]
        public void QueryDynamicParams()
        {
            QueryHelper.GenerateDebugSql = true;

            try
            {
                using (var cn = LocalDb.GetConnection(dbName))
                {
                    var qry = new DynamicQuery("[SomeDate]>@minDate") { FirstName = "peabody" };
                    var results = qry.Execute(cn, setParams: (queryParams) =>
                    {
                        queryParams.Add("minDate", new DateTime(1990, 1, 1));
                    });
                    Assert.IsTrue(qry.ResolvedSql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [SomeDate]>@minDate AND [FirstName] LIKE '%'+@firstName+'%' ORDER BY [FirstName]"));
                    Assert.IsTrue(qry.DebugSql.ReplaceWhitespace().Equals(
                        @"DECLARE @FirstName nvarchar(max), @minDate datetime;
                    SET @FirstName = 'peabody';
                    SET @minDate = '1/1/1990 12:00:00 AM';

                    SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [SomeDate]>@minDate AND [FirstName] LIKE '%'+@firstName+'%' ORDER BY [FirstName]".ReplaceWhitespace()));
                }
            }
            finally
            {
                QueryHelper.GenerateDebugSql = ShouldDebugSql;
            }
        }
        
        [TestMethod]
        public void OffsetQueryAsync()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.ExecuteAsync(cn).Result;
                Debug.Print($"result count = {results.Count()}");
                Assert.IsTrue(results.Count() == 20);
            }
        }

        [TestMethod]
        public void OffsetQueryNewPageSize()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.ExecuteAsync(cn, newPageSize: 10).Result;
                Debug.Print($"result count = {results.Count()}");
                Assert.IsTrue(results.Count() == 10);
            }
        }

        [TestMethod]
        public void OffsetQuery()
        {
            using (var cn = LocalDb.GetConnection(dbName))
            {
                var results = new TypicalQuery() { PageNumber = 4 }.Execute(cn);
                Debug.Print($"result count = {results.Count()}");
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
            QueryHelper.GenerateDebugSql = true;

            try
            {
                var qry = new TypicalQuery()
                {
                    MinWeight = 9,
                    MaxWeight = 56,
                    FirstNameLike = "warbler",
                    MinDate = new DateTime(2020, 1, 15)
                };

                qry.ResolveSql(removeMacros: true);
                string debug = qry.DebugSql;
                Assert.IsTrue(debug.ReplaceWhitespace().Equals(
                    @"DECLARE @MinWeight decimal, @MaxWeight decimal, @FirstNameLike nvarchar(max), @MinDate datetime;
                    SET @MinWeight = 9;
                    SET @MaxWeight = 56;
                    SET @FirstNameLike = 'warbler';
                    SET @MinDate = '1/15/2020 12:00:00 AM';

                    SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [Weight]>=@minWeight AND [Weight]<=@maxWeight AND [FirstName] LIKE '%'+@firstNameLike+'%' AND [SomeDate]>=@minDate  ORDER BY [FirstName]".ReplaceWhitespace()));
            }
            finally
            {
                QueryHelper.GenerateDebugSql = ShouldDebugSql;
            }            
        }

        [TestMethod]
        public void SimpleTvp()
        {
            var qry = new SimpleTvpExample() { Source = new int[] { 1, 2, 3 }.ToDataTable() };

			using var cn = LocalDb.GetConnection(dbName);

			try
			{
				cn.Execute("CREATE TYPE [IdList] AS TABLE ([Id] int NOT NULL PRIMARY KEY)");
			}
			catch
			{
				// do nothing                    
			}

			var results = qry.Execute(cn);
			Assert.IsTrue(results.SequenceEqual(new int[] { 1, 2, 3 }));
		}

        [TestMethod]
        public void MoreComplexTvp()
        {
            var data = new[]
            {
                new PersonResult() { FirstName = "Waldo", LastName = "Where Is", DateOfBirth = new DateTime(1990, 1, 1), Id = 432 },
                new PersonResult() { FirstName = "Jenny", LastName = "Anybody", DateOfBirth = new DateTime(1980, 1, 1), Id = 184 },
            };

            var qry = new MoreComplexTvpExample()
            {
                Source = data.ToDataTable()
            };

            using (var cn = LocalDb.GetConnection(dbName))
            {
                try
                {
                    cn.Execute(
                        @"CREATE TYPE [dbo].[PersonInfo] AS TABLE (
                            [FirstName] nvarchar(50) NOT NULL, 
                            [LastName] nvarchar(50) NOT NULL,
                            [DateOfBirth] datetime NULL,
                            [Id] int NOT NULL
                        )");
                }
                catch 
                {
                    // do nothing
                }

                var results = qry.Execute(cn);
                Assert.IsTrue(results.Select(row => row.Id).SequenceEqual(data.Select(row => row.Id)));
            }
        }

        [TestMethod]
        public async Task DateParamError()
        {
            using var cn = LocalDb.GetConnection(dbName);
            var qry = new TypicalQuery() { MinDate = new DateTime(1, 1, 1) };

            try
            {
                var results = await qry.ExecuteAsync(cn);
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.Message.Contains("in TypicalQuery"));
            }            
        }
    }
}
