using System;

namespace Dapper.QX.Attributes
{
    /// <summary>
    /// Defines a WHERE clause expression that is appended to a query
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WhereAttribute : Attribute
    {
        public WhereAttribute(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; }

        /// <summary>
        /// Enables unit tests to test this parameter with a sample value to see if the underlying query is valid
        /// </summary>
        public object TestValue { get; set; }
    }
}
