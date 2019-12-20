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

    public class TypicalQuery : Query<TypicalQueryResult>, ITestableQuery
    {
        public TypicalQuery() : base("SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] {where} ORDER BY [FirstName] {offset}")
        {
        }

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

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new TypicalQuery() { MinWeight = 10 };
            yield return new TypicalQuery() { MaxWeight = 100 };
            yield return new TypicalQuery() { FirstNameLike = "jambo" };
            yield return new TypicalQuery() { MinDate = new DateTime(2012, 1, 1) };
            yield return new TypicalQuery() { MaxDate = new DateTime(2019, 1, 1) };
            yield return new TypicalQuery() { NotesContain = "this that -whatever" };
            yield return new TypicalQuery() { PageNumber = 3 };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
