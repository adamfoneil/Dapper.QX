using System;

namespace Dapper.QX.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NullWhenAttribute : Attribute
    {
        public NullWhenAttribute(params object[] values)
        {
            NullValues = values;
        }

        public object[] NullValues { get; }
    }
}
