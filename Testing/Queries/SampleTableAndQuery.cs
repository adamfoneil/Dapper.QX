using Dapper.QX;
using Dapper.QX.Attributes;
using Dapper.QX.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Testing.Queries
{
    public class SampleTableResult
    {
        public string FirstName { get; set; }
        public decimal Weight { get; set; }
        public DateTime SomeDate { get; set; }
        public int Id { get; set; }
    }

    public class Yimba : Query<SampleTableResult>, ITestableQuery
    {
        public Yimba() : base("SELECT [FirstName], [Weight], [SomeDate], [Id] FROM [SampleTable] {where}")
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

        public IEnumerable<ITestableQuery> GetTestCases()
        {
            yield return new Yimba() { MinWeight = 10 };
            yield return new Yimba() { MaxWeight = 100 };
            yield return new Yimba() { FirstNameLike = "jambo" };
            yield return new Yimba() { MinDate = new DateTime(2012, 1, 1) };
            yield return new Yimba() { MaxDate = new DateTime(2019, 1, 1) };
        }

        public IEnumerable<dynamic> TestExecute(IDbConnection connection)
        {
            return TestExecuteHelper(connection);
        }
    }
}
