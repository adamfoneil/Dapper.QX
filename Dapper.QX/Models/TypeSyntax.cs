namespace Dapper.QX.Models
{
    internal class TypeSyntax
    {
        public TypeSyntax(string type, bool isString)
        {
            Type = type;
            IsString = isString;
        }

        public string Type { get; set; }
        public bool IsString { get; set; }

        public string FormatValue(string value)
        {
            return (IsString) ?
                "'" + value?.Replace("'", "''") + "'" :
                value;
        }
    }
}
