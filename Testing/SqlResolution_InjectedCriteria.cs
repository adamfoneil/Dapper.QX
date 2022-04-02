using Dapper.QX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable]  <<macro>> ORDER BY [FirstName]"));
        }

        [TestMethod]
        public void QueryInjectCriteria()
        {
            var query = new TypicalQuery() { FirstNameLike = "arxo" };
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE [FirstName] LIKE '%'+@firstNameLike+'%' <<macro>> ORDER BY [FirstName]"));
        }

        [TestMethod]
        public void UnionQuery()
        {
            var query = new UnionQuery() { SomethingId = 12 };
            var sql = QueryHelper.ResolveSql(query.Sql, query);
            Assert.IsTrue(sql.Equals(
            @"SELECT [This] FROM [That] WHERE [SomethingId]=@somethingId ORDER BY [Whatever]
            UNION
            SELECT [That] FROM [This] [Whatever]='bingo' AND [SomethingId]=@somethingId ORDER BY [Another]"));
        }

        [TestMethod]
        public void QueryNullWhenSame()
        {
            var qry1 = new NullWhenQuery();

            // using -1 for this property should be the same as null because [NullWhen(-1)]
            qry1.OptionalId = -1;

            // don't set the OptionalId property
            var qry2 = new NullWhenQuery();

            // two queries should be the same
            Assert.IsTrue(qry1.ResolveSql().Equals(qry2.ResolveSql()));
        }

        [TestMethod]
        public void QueryNullWhenDifferent()
        {
            var qry1 = new NullWhenQuery();
            qry1.OptionalId = 332; // not [NullWhen(-1)], so it should be included

            var sql = qry1.ResolveSql();
            Assert.IsTrue(sql.Equals("SELECT [Id] FROM [Whatever] WHERE [OptionalId] = @value ORDER BY [BlahBlahBlah]"));
        }

        [TestMethod]
        public void QueryNullWhenAndWhere()
        {
            var qry1 = new NullWhenAndWhereQuery();

            // using -1 for this property should be the same as null because [NullWhen(-1)]
            qry1.OptionalId = -1;

            // don't set the OptionalId property
            var qry2 = new NullWhenAndWhereQuery();

            // two queries should be the same
            Assert.IsTrue(qry1.ResolveSql().Equals(qry2.ResolveSql()));
        }

        [TestMethod]
        public void QueryNewPageSize()
        {
            var qry = new TypicalQuery();
            qry.PageNumber = 10;
            var sql = qry.ResolveSql(newPageSize: 5);
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable]  <<macro>> ORDER BY [FirstName] OFFSET 50 ROWS FETCH NEXT 5 ROWS ONLY"));
        }

        [TestMethod]
        public void QueryDefaultPageSize()
        {
            var qry = new TypicalQuery();
            qry.PageNumber = 10;
            var sql = qry.ResolveSql();
            Assert.IsTrue(sql.Equals("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable]  <<macro>> ORDER BY [FirstName] OFFSET 200 ROWS FETCH NEXT 20 ROWS ONLY"));
        }

        [TestMethod]
        public void DualScopeQuery1()
        {
            var qry = new DualScopeQuery();
            qry.StartDate = new DateTime(2022, 3, 31);

            var sql = qry.ResolveSql();
            Assert.IsTrue(sql.Equals(@"SELECT
                [ProjectID], [ProjectInfo], [Units], 
                SUM(BillableQty) AS [TotalBillableQty], SUM([Quantity]) AS [TotalQty], SUM([ExpenseDollars]) AS [TotalExpenseDollars], 
                SUM([BillableDollars]) AS [TotalBillableDollars], [prj-totals].[ProjectExpenseAmount] 
            FROM
                (SELECT [wr].* FROM [aw].[AllWorkRecords] [wr] WHERE [ThatDate]>=@startDate) AS [source]
                LEFT JOIN (
                    SELECT [ProjectID], SUM([Amount]) AS [ProjectExpenseAmount]
                    FROM [aw].[ProjectExpense]
                    WHERE 1=1 AND [ThisDate]>=@startDate
                    GROUP BY [ProjectID]
                ) [prj-totals] ON [source].[ProjectID]=[prj-totals].[ProjectID]
            GROUP BY
                [ProjectID], [ProjectInfo], [Units] 
            ORDER BY 
                [ProjectID], [ProjectInfo], [Units]"));
        }

        [TestMethod]
        public void DualScopeQuery2()
        {
            var qry = new DualScopeQuery();
            qry.StartDate = new DateTime(2022, 3, 31);
            qry.EndDate = new DateTime(2022, 4, 2);

            var sql = qry.ResolveSql();
            Assert.IsTrue(sql.Equals(@"SELECT
                [ProjectID], [ProjectInfo], [Units], 
                SUM(BillableQty) AS [TotalBillableQty], SUM([Quantity]) AS [TotalQty], SUM([ExpenseDollars]) AS [TotalExpenseDollars], 
                SUM([BillableDollars]) AS [TotalBillableDollars], [prj-totals].[ProjectExpenseAmount] 
            FROM
                (SELECT [wr].* FROM [aw].[AllWorkRecords] [wr] WHERE [ThatDate]>=@startDate AND [ThatDate]<=@endDate) AS [source]
                LEFT JOIN (
                    SELECT [ProjectID], SUM([Amount]) AS [ProjectExpenseAmount]
                    FROM [aw].[ProjectExpense]
                    WHERE 1=1 AND [ThisDate]>=@startDate AND [ThisDate]<=@endDate
                    GROUP BY [ProjectID]
                ) [prj-totals] ON [source].[ProjectID]=[prj-totals].[ProjectID]
            GROUP BY
                [ProjectID], [ProjectInfo], [Units] 
            ORDER BY 
                [ProjectID], [ProjectInfo], [Units]"));
        }
    }
}
