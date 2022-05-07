using System;

namespace Dapper.QX.Attributes
{
    /// <summary>
    /// use the {top} token in your query to indicate where to insert the TOP clause
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TopAttribute : Attribute
    {
    }
}
