using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Testing.Queries
{
    public class TypicalQueryResult
    {
        public string FirstName { get; set; }
        public decimal Weight { get; set; }
        public DateTime SomeDate { get; set; }
        public string Notes { get; set; }
        public int Id { get; set; }
    }

    public class TypicalQuery : Query<TypicalQueryResult>, ITestableQuery, ISelfModifyingQuery
    {
        public TypicalQuery() : base("SELECT {top} [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] {join} {where} **removethis** <<macro>> ORDER BY [FirstName] {offset}")
        {
        }

        [Top]
        public int? Top { get; set; }

        [Where("[Weight]>=@minWeight")]
        public decimal? MinWeight { get; set; }

        [Where("[Weight]<=@maxWeight")]
        public decimal? MaxWeight { get; set; }

        [Where("[FirstName] LIKE '%'+@firstNameLike+'%'")]
        public string FirstNameLike { get; set; }

        [Where("[SomeDate]>=@minDate")]
        public DateTime? MinDate { get; set; }

        [Where("[SomeDate]<=@maxDate")]
        public DateTime? MaxDate { get; set; }

        [Phrase(new string[] { nameof(TypicalQueryResult.Notes) })]
        public string NotesContain { get; set; }

        [Offset(20)]
        public int? PageNumber { get; set; }

        [Join("INNER JOIN [SampleTable2] ON [SampleTable].[Id] = [SampleTable2].[SampleId]")]
		public bool WithTable2Join { get; set; }

        [NullWhen(0)]
        [Parameter]
        [Join("INNER JOIN [SampleTable3] ON [SampleTable].[Id] = [SampleTable3].[SampleId] AND [SampleTable3]=@joinWithParam")] 
		public int JoinWithParam { get; set; }

		/// <summary>
		/// not a practical example, but demonstrates how query can be "self-modified" when run
		/// </summary>
		public string BuildSql(string rawSql) => rawSql.Replace("**removethis** ", string.Empty);
        
        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new TypicalQuery() { MinWeight = 10 };
            yield return new TypicalQuery() { MaxWeight = 100 };
            yield return new TypicalQuery() { FirstNameLike = "jambo" };
            yield return new TypicalQuery() { MinDate = new DateTime(2012, 1, 1) };
            yield return new TypicalQuery() { MaxDate = new DateTime(2019, 1, 1) };
            yield return new TypicalQuery() { NotesContain = "this that -whatever" };
            yield return new TypicalQuery() { PageNumber = 3 };
            yield return new TypicalQuery() { Top = 10 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
