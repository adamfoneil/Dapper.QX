using System;

namespace Dapper.QX.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class OrderByAttribute : Attribute
	{
		public OrderByAttribute(object value, string expression)
		{
			Value = value;
			Expression = expression;
		}

		public object Value { get; }

		public string Expression { get; }
	}
}
