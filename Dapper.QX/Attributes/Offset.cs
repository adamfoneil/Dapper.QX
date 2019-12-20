using System;

namespace Dapper.QX.Attributes
{
    /// <summary>
    /// Describes how to inject SQL Server OFFSET/FETCH syntax into a query.
    /// Query SQL requires an {offset} token to indicate where the OFFSET syntax is inserted.
    /// Assumes 0-based paging (i.e. first page = 0).
    /// Your query Paging property should be nullable int, and your must have ORDER BY in your query
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OffsetAttribute : Attribute
    {
        public OffsetAttribute(int pageSize)
        {
            PageSize = pageSize;
        }

        public int PageSize { get; }

        public int GetStartRow(int page)
        {
            return page * PageSize;
        }

        public string GetOffsetFetchSyntax(int page)
        {
            return $"OFFSET {GetStartRow(page)} ROWS FETCH NEXT {PageSize} ROWS ONLY";
        }
    }
}
