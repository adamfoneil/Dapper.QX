using Dapper.QX;
using Dapper.QX.Attributes;

namespace Testing.Queries
{
    public class DynamicQuery : Query<TypicalQueryResult>
    {
        public DynamicQuery(string whereClause) : base($"SELECT [FirstName], [Weight], [SomeDate], [Notes], [Id] FROM [SampleTable] WHERE {whereClause} {{andWhere}} ORDER BY [FirstName]")
        {
        }

        [Where("[FirstName] LIKE '%'+@firstName+'%'")]
        public string FirstName { get; set; }
    }
}
