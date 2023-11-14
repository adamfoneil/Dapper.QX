using System;

namespace Dapper.QX.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class PhraseAttribute : Attribute
	{
		public PhraseAttribute(params string[] columnNames)
		{
			ColumnNames = columnNames;
		}

		public string[] ColumnNames { get; }

		public char LeadingColumnDelimiter { get; set; } = '[';
		public char EndingColumnDelimiter { get; set; } = ']';

		/// <summary>
		/// Indicates whether wildcard concatenation is added around parameter names in the resolved SQL.
		/// True for SQL Server, False for SQL Compact Edition.        
		/// </summary>
		public bool ConcatenateWildcards { get; set; } = true;
	}
}
