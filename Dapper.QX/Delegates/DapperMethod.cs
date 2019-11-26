using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Dapper.QX.Delegates
{
    internal delegate DapperResult<T> DapperMethod<T>(string sql, object param);
    
    internal class DapperResult<T>
    {
        public T Single { get; set; }
        public IEnumerable<T> Enumerable { get; set; }
    }

}
