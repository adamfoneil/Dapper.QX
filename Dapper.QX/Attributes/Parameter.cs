using System;

namespace Dapper.QX.Attributes
{
	/// <summary>
	/// Defines a query parameter without any related WHERE clause of its own,
	/// for example a parameter used inside an EXISTS subquery
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class ParameterAttribute : Attribute
	{
	}
}
