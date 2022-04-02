using System;

namespace Dapper.QX.Attributes
{
    /// <summary>
    /// Defines a WHERE clause expression that is appended to a query
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class WhereAttribute : Attribute
    {        
        public WhereAttribute(string scope, string expression)
        {
            Scope = scope;
            Expression = expression;
        }

        public WhereAttribute(string expression) : this(QueryHelper.GlobalScope, expression)
        {                        
        }

        public string Scope { get; }
        public string Expression { get; }

        /// <summary>
        /// Enables unit tests to test this parameter with a sample value to see if the underlying query is valid
        /// </summary>
        public object TestValue { get; set; }
    }
}
