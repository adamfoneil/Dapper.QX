namespace Dapper.QX.Interfaces
{
    /// <summary>
    /// implement this on queries where you need something really custom that can't be modeled with typical properties
    /// </summary>
    public interface ISelfModifyingQuery
    {
        string BuildSql(string rawSql);
    }
}
