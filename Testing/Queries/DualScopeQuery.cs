using Dapper.QX;
using Dapper.QX.Attributes;
using System;

namespace Testing.Queries
{
    internal class DualScopeQuery : Query<string>        
    {
        public DualScopeQuery() : base(
            @"SELECT
                [ProjectID], [ProjectInfo], [Units], 
                SUM(BillableQty) AS [TotalBillableQty], SUM([Quantity]) AS [TotalQty], SUM([ExpenseDollars]) AS [TotalExpenseDollars], 
                SUM([BillableDollars]) AS [TotalBillableDollars], [prj-totals].[ProjectExpenseAmount] 
            FROM
                (SELECT [wr].* FROM [aw].[AllWorkRecords] [wr] {where}) AS [source]
                LEFT JOIN (
                    SELECT [ProjectID], SUM([Amount]) AS [ProjectExpenseAmount]
                    FROM [aw].[ProjectExpense]
                    WHERE 1=1 {prj:andWhere}
                    GROUP BY [ProjectID]
                ) [prj-totals] ON [source].[ProjectID]=[prj-totals].[ProjectID]
            GROUP BY
                [ProjectID], [ProjectInfo], [Units] 
            ORDER BY 
                [ProjectID], [ProjectInfo], [Units]")
        {
        }

        [Where("prj", "[ThisDate]>=@startDate")]
        [Where("[ThatDate]>=@startDate")]
        public DateTime? StartDate { get; set; }

        [Where("prj", "[ThisDate]<=@endDate")]
        [Where("[ThatDate]<=@endDate")]
        public DateTime? EndDate { get; set; }
    }

    internal class DualScopeQuery2 : Query<string>
    {
        public DualScopeQuery2() : base(
            @"SELECT
                [ProjectID], [ProjectInfo], [Units], 
                SUM(BillableQty) AS [TotalBillableQty], SUM([Quantity]) AS [TotalQty], SUM([ExpenseDollars]) AS [TotalExpenseDollars], 
                SUM([BillableDollars]) AS [TotalBillableDollars], [prj-totals].[ProjectExpenseAmount] 
            FROM
                (SELECT [wr].* FROM [aw].[AllWorkRecords] [wr] {where}) AS [source]
                LEFT JOIN (
                    SELECT [ProjectID], SUM([Amount]) AS [ProjectExpenseAmount]
                    FROM [aw].[ProjectExpense]
                    {prj:where}
                    GROUP BY [ProjectID]
                ) [prj-totals] ON [source].[ProjectID]=[prj-totals].[ProjectID]
            GROUP BY
                [ProjectID], [ProjectInfo], [Units] 
            ORDER BY 
                [ProjectID], [ProjectInfo], [Units]")
        {
        }

        [Where("prj", "[ThisDate]>=@startDate")]
        [Where("[ThatDate]>=@startDate")]
        public DateTime? StartDate { get; set; }

        [Where("prj", "[ThisDate]<=@endDate")]
        [Where("[ThatDate]<=@endDate")]
        public DateTime? EndDate { get; set; }
    }
}
