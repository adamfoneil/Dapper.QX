using System.Data;

namespace Dapper.QX.Models
{
    public class ParamValue
    {
        public ParamValue(object value, DbType type)
        {
            Value = value;
            Type = type;
        }

        public object Value { get; }
        public DbType Type { get; }

        public ParameterDirection? Direction { get; set; }
        public int? Size { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
    }
}
