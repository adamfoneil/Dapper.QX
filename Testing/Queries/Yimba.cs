using Dapper.QX;
using Dapper.QX.Attributes;
using System;

namespace Testing.Queries
{
    public class YimbaResult
    {
        public string FirstName { get; set; }
        public decimal Weight { get; set; }
        public DateTime Occident { get; set; }
    }

    public class Yimba : Query<YimbaResult>
    {
        public Yimba() : base("SELECT [FirstName], [Weight], [Occident] FROM [Yimba] {where}")
        {
        }

        [Where("[Weight]>=@minWeight")]
        public decimal? MinWeight { get; set; }

        [Where("[Weight]<=@maxWeight")]
        public decimal? MaxWeight { get; set; }

        [Where("[FirstName] LIKE '%'+@firstNameLike+'%'")]
        public string FirstNameLike { get; set; }
    }
}
