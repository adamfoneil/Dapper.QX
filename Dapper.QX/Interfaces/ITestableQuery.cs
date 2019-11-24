using System.Collections.Generic;
using System.Data;

namespace Dapper.QX.Interfaces
{
    /// <summary>
    /// Implement this on your Query types to make it easy to unit test your queries.
    /// Use <see cref="Query{TResult}.TestExecuteHelper(IDbConnection)"/> in your interface implementation
    /// </summary>
    public interface ITestableQuery
    {
        IEnumerable<dynamic> TestExecute(IDbConnection connection);
        IEnumerable<ITestableQuery> GetTestCases();
    }
}
