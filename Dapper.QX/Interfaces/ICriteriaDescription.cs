using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Dapper.QX.Interfaces
{
    /// <summary>
    /// implement this on queries when you want to generate a plain-English description 
    /// of the criteria in effect on a query
    /// </summary>
    public interface ICriteriaDescription
    {
        Task<List<string>> GetCriteriaTermsAsync(IDbConnection connection);
    }
}
