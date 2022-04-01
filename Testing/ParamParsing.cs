using Dapper.QX;
using Dapper.QX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Testing
{
    [TestClass]
    public class ParamParsing
    {
        [TestMethod]
        public void SimpleParamParse()
        {
            string sample = "THIS is a STATEMENT @yes that sort of doesn't REALLY resemble SQL @no indeed IS NOT";
            var paramNames = RegexHelper.ParseParameterNames(sample);
            Assert.IsTrue(paramNames.SequenceEqual(new string[] { "@yes", "@no" }));
        }

        [TestMethod]
        public void OptionalParams()
        {
            string sql =
                @"SELECT [this], [that], [other]
                FROM [whatever]
                WHERE 
                    [wonga]=@whatever 
                    [[ AND [plingus]=@thangly ]]
                    [[ AND [thardimus]=@yarbinshaw ]]
                ORDER BY [crimson]";

            var tokens = RegexHelper.ParseOptionalTokens(sql);
            Assert.IsTrue(tokens.SelectMany(t => t.ParameterNames).SequenceEqual(new string[]
            {
                "@thangly", "@yarbinshaw"
            }));

            Assert.IsTrue(tokens.Select(t => t.Content).SequenceEqual(new string[]
            {
                "AND [plingus]=@thangly", "AND [thardimus]=@yarbinshaw"
            }));
        }        

        [TestMethod]
        public void ParseRequiredAndOptional()
        {
            string sql =
                @"SELECT [chintzy], [argenslard], [vuxitron]
                FROM [garbenslade]
                WHERE 
                    [helem]=@klaksod AND
                    [wilvip]=@horgunz
                    [[ AND [rembenslom]=@hoopsenfargle ]]
                    [[ AND ([enzelfrage]=@zahbenlious OR [yexelhor]=@craybentanz) ]]";

            var paramInfo = RegexHelper.ParseParameters(sql);

            Assert.IsTrue(paramInfo.Required.SequenceEqual(new string[] { "@klaksod", "@horgunz" }));
            Assert.IsTrue(paramInfo.Optional.Select(o => o.Content).SequenceEqual(new string[]
            {
                "AND [rembenslom]=@hoopsenfargle",
                "AND ([enzelfrage]=@zahbenlious OR [yexelhor]=@craybentanz)"
            }));
        }

        [TestMethod]
        public void ParseAllParamNames()
        {
            string sql =
                @"SELECT [chintzy], [argenslard], [vuxitron]
                FROM [garbenslade]
                WHERE 
                    [helem]=@klaksod AND
                    [wilvip]=@horgunz
                    {{ AND [rembenslom]=@hoopsenfargle }}
                    {{ AND ([enzelfrage]=@zahbenlious OR [yexelhor] IN (@craybentanz, @horgunz)) }}";

            var paramInfo = RegexHelper.ParseParameters(sql);
            Assert.IsTrue(paramInfo.AllParamNames().SequenceEqual(new string[]
            {
                "@klaksod", "@horgunz", "@hoopsenfargle", "@zahbenlious", "@craybentanz"
            }));
        }

        [TestMethod]
        public void FindMacros()
        {
            var sql = $@"DECLARE @folders TABLE (                
                [Id] bigint NOT NULL,
                [FullPath] nvarchar(max) NOT NULL                
            );

            INSERT INTO @folders ([Id], [FullPath])
            SELECT [Id], [FullPath]
            FROM [dbo].[FnUserFolders](@customerId, @userId);

            WITH [folderTree] AS (
                SELECT [ParentId], [Id], [FullPath]
                FROM @folders
                WHERE [Id]=@rootFolderId
                UNION ALL
                SELECT [child].[ParentId], [child].[Id], [child].[FullPath]
                FROM @folders [child] INNER JOIN [folderTree] ON [child].[ParentId]=[folderTree].[Id]        
            ) SELECT
                [doc].*, [tree].[FullPath]
            FROM 
                [dbo].[Document] [doc] 
                INNER JOIN [dbo].[DocumentFolder] [df] ON [doc].[Id]=[df].[DocumentId]
                INNER JOIN [folderTree] ON COALESCE([df].[FolderId], @customerId * -1) = [folderTree].[Id]
            WHERE
                [doc].[CustomerId]=@customerId {{andWhere}} <<fields>>
            ORDER BY 
                {{orderBy}} {{offset}}";

            var macros = RegexHelper.ParseMacros(sql);
            Assert.IsTrue(macros.SequenceEqual(new string[] { "<<fields>>" }));
        }

        [TestMethod]
        public void GetWhereScopes1()
        {
            var sql = 
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
                    [ProjectID], [ProjectInfo], [Units]";

            var scopes = RegexHelper.GetWhereScopes(sql);
            Assert.IsTrue(scopes.SequenceEqual(new[] { "global", "prj" }));
        }

        [TestMethod]
        public void GetWhereScopes2()
        {
            var sql =
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
                    [ProjectID], [ProjectInfo], [Units]";

            var scopes = RegexHelper.GetWhereScopes(sql);
            Assert.IsTrue(scopes.SequenceEqual(new[] { "global", "prj" }));
        }
    }
}
