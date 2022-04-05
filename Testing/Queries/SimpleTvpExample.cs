using Dapper.QX;
using Dapper.QX.Attributes;
using System;
using System.Data;

namespace Testing.Queries
{
    internal class SimpleTvpExample : Query<int>
    {
        public SimpleTvpExample() : base(
            @"DECLARE @list [IdList];
            INSERT INTO @list ([Id]) SELECT [Id] FROM @source;
            SELECT [Id] FROM @list")
        {
        }

        [TableType("[dbo].[IdList]")]
        public DataTable Source { get; set; }
    }

    internal class PersonResult
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Id { get; set; }
    }

    internal class MoreComplexTvpExample : Query<PersonResult>
    {
        public MoreComplexTvpExample() : base(
            @"DECLARE @list [PersonInfo];
            INSERT INTO @list ([FirstName], [LastName], [DateOfBirth], [Id])
            SELECT [FirstName], [LastName], [DateOfBirth], [Id]
            FROM @source;
            SELECT * FROM @list;")
        {
        }

        [TableType("[dbo].[PersonInfo]")]
        public DataTable Source { get; set; }
    }
}
