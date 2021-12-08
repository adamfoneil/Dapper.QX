using Dapper.QX;
using Dapper.QX.Attributes;

namespace Testing.Queries
{
    public class UnionQuery : Query<string>
    {
        public UnionQuery() : base(
            @"SELECT [This] FROM [That] {where} ORDER BY [Whatever]
            UNION
            SELECT [That] FROM [This] [Whatever]='bingo' {andWhere} ORDER BY [Another]")
        {
        }

        [Where("[SomethingId]=@somethingId")]
        public int? SomethingId { get; set; }
    }
}
