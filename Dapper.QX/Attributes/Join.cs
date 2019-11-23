using System;

namespace Dapper.QX.Attributes
{
    /// <summary>
    /// Use this on <see cref="Query{TResult}"/> classes on bool properties to indicate dynamically inserted joins.	
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class JoinAttribute : Attribute
    {
        public JoinAttribute(string sql)
        {
            Sql = sql;
        }

        public string Sql { get; }
    }
}
