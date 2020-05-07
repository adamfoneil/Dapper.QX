using System;

namespace Dapper.QX.Models
{
    internal class TypeValue
    {
        public Type Type { get; set; }
        public string ValueLiteral { get; set; }    
        public bool IsDynamic { get; set; }
    }
}
