using Dapper.QX;
using Dapper.QX.Attributes;

namespace Testing.Queries
{
    public class IntArrayParamQuery : Query<int>
    {
        public IntArrayParamQuery() : base("SELECT [Id] FROM [Whatever] WHERE [Id] IN @idList AND [SomethingElseId]=@somethingElseId ORDER BY [Yes]")
        {
        }

        public int SomethingElseId { get; set; }
        public int[] IdList { get; set; }
    }

    /// <summary>
    /// not implemented
    /// </summary>
    public class StringArrayParamQuery : Query<string>
    {
        public StringArrayParamQuery() : base("SELECT [Yes] FROM [Whatever] {where} ORDER BY [Yes]")
        {
        }

        [Where("[Name] IN @nameList")]
        public string[] NameList { get; set; }
    }

    public class NullWhenAndWhereQuery : Query<int>
    {
        public NullWhenAndWhereQuery() : base("SELECT [Id] FROM [Whatever] WHERE [TenantId]=@tenantId {andWhere} ORDER BY [BlahBlahBlah]")
        {
        }

        public int TenantId { get; set; }

        [NullWhen(-1)]
        [Where("[OptionalId] = @value")]
        public int? OptionalId { get; set; }
    }

    public class NullWhenQuery : Query<int>
    {
        public NullWhenQuery() : base("SELECT [Id] FROM [Whatever] {where} ORDER BY [BlahBlahBlah]")
        {
        }

        [NullWhen(-1)]
        [Where("[OptionalId] = @value")]
        public int? OptionalId { get; set; }
    }

}
