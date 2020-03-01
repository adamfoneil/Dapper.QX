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

    public class StringArrayParamQuery : Query<string>
    {
        public StringArrayParamQuery() : base("SELECT [Yes] FROM [Whatever] {where} ORDER BY [Yes]")
        {
        }

        [Where("[Name] IN @nameList")]
        public string[] NameList { get; set; }
    }
}
