using Dapper.QX.Abstract;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;
using SqlIntegration.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Data;

namespace Testing.Queries
{
    public class TestableQuerySample : TestableQuery<int>
    {
        public TestableQuerySample() : base("SELECT {top} [FirstName], [Weight], [SomeDate], [Notes], [SampleTable].[Id] FROM [SampleTable] {join} {where} ORDER BY [FirstName] {offset}")
        {
        }

        [Top]
        public int? LimitRows { get; set; }

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

        [TableType("dbo.IdList")]
        [Join("INNER JOIN @joinIds AS [j] ON [SampleTable].[Id] = [j].[Id]")]
		public DataTable JoinIds { get; set; }
        
        /*
         * used only to see if new Debug output appeared
        [Where("[MissingColumn]=@brokenParam")]
        public string BrokenParam { get; set; }
        */

        [Offset(20)]
        public int? PageNumber { get; set; }

        protected override IEnumerable<ITestableQuery> GetTestCasesInner()
        {
            yield return new TestableQuerySample() { MinWeight = 10 };
            yield return new TestableQuerySample() { MaxWeight = 100 };
            yield return new TestableQuerySample() { FirstNameLike = "jambo" };
            yield return new TestableQuerySample() { MinDate = new DateTime(2012, 1, 1) };
            yield return new TestableQuerySample() { MaxDate = new DateTime(2019, 1, 1) };
            yield return new TestableQuerySample() { NotesContain = "this that -whatever" };
            yield return new TestableQuerySample() { PageNumber = 3 };
			yield return new TestableQuerySample() { JoinIds = new int[] { 1, 2, 3 }.ToDataTable() };
			//yield return new TestableQuerySample() { BrokenParam = "whatever" };
		}
    }
}
