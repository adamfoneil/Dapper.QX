using System.Collections.Generic;

namespace Dapper.QX.Classes
{
	/// <summary>
	/// This enables us to use a single method to execute Dapper queries, 
	/// which implements the tracing and exception handling available across this library.
	/// See <see cref="Query{TResult}.ExecuteInnerAsync{T}(System.Func{string, object, System.Threading.Tasks.Task{DapperResult{T}}})"/>
	/// </summary>    
	internal class DapperResult<T>
	{
		public T Single { get; set; }
		public IEnumerable<T> Enumerable { get; set; }
	}

}