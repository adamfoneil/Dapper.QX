[![Build status](https://ci.appveyor.com/api/projects/status/cyehxnqmbiwhwpqo?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-qx)
[![Nuget](https://img.shields.io/nuget/v/Dapper.QX)](https://www.nuget.org/packages/Dapper.QX/)

Nuget package **Dapper.QX** makes inline SQL more powerful and testable via the [Query\<TResult\>](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Query_base.cs) class. Get the convenience, safety and capability of [Dapper](https://github.com/StackExchange/Dapper) with dynamic criteria, tracing, and full text queries. From the Wiki: [Why Dapper.QX?](https://github.com/adamosoftware/Dapper.QX/wiki)

```csharp
public class MyQuery : Query<MyResultClass>
{
    public MyQuery() : base(
        @"SELECT * 
        FROM [whatever]
        {where}
        ORDER BY [something]")
    
    [Where("[SomeDate]>=@minDate")]
    public DateTime? MinDate { get; set; }
    
    [Where("[SomeDate]<=@maxDate")]
    public DateTime? MaxDate { get; set; }
    
    [Case("0", "[AssignedTo] IS NULL")]
    [Case("-1", "[AssignedTo] IS NOT NULL")]
    [Where("[AssignedTo]=@assignedTo")]
    public string AssignedTo { get; set; }
}
```
Run your query like this:
```csharp
using (var cn = GetConnection())
{
    var data = await new MyQuery() 
    {
        MinDate = DateTime.Now, 
        MaxDate = DateTime.Now.AddDays(30),
        AssignedTo = "somebody"
    }.ExecuteAsync(cn);
}
```
In the example above `GetConnection` is a fictional method -- you will need to provide your own method that returns an `IDbConnection` that works in your project. Read on below for an alternate syntax that lets you omit the `using` block.

Use **{where}** or **{andWhere}** tokens to indicate where dynamic criteria is inserted. Mix and match [Where](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Where.cs) and [Case](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Case.cs) attributes on query class properties to control what criteria is injected. [Learn about](https://github.com/adamosoftware/Dapper.QX/wiki/Reference) more attributes Dapper.QX offers.

Note that you can omit the `using` block if you use the `Execute*` [overloads](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Query_func.cs) that accept a `Func<IDbConnection>` instead of `IDbConnection`. This assumes you still have a method in your project that returns `IDbConnection`. Adapting the example above, this would look like this:

```csharp
var data = await new MyQuery() 
{
    MinDate = DateTime.Now, 
    MaxDate = DateTime.Now.AddDays(30),
    AssignedTo = "somebody"
}.ExecuteAsync(GetConnection);
```
This approach makes sense when you have just one query to run, and you don't need the database connection for anything else.

## Tooling
To help you build C# result classes for any SQL query, I offer a free tool [Postulate.Zinger](https://github.com/adamosoftware/Postulate.Zinger).

[![download](https://img.shields.io/badge/Download-Installer-blue.svg)](https://aosoftware.blob.core.windows.net/install/ZingerSetup.exe)

## Testing
Make query classes testable by basing them on [TestableQuery](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Abstract/TestableQuery.cs). This approach catches invalid SQL, but does not assert any particular query results. See the [wiki topic](https://github.com/adamfoneil/Dapper.QX/wiki/Testing-Tips) for more info.

Note that you can also use the interface [ITestableQuery](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Interfaces/ITestableQuery.cs) directly if you wish, but you must implement [TestExecute](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Interfaces/ITestableQuery.cs#L12) yourself. There's normally no reason to do this, since I use the same [implementation](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Abstract/TestableQuery.cs#L15) everywhere. Therefore, I recommend using the abstract class `TestableQuery` instead of `ITestableQuery`.

```csharp
public class MyQuery : TestableQuery<MyResultClass>
{
    // same code above omitted
  
    // implement GetTestCasesInner method to return every parameter combination you need to test
    protected IEnumerable<ITestableQuery> GetTestCasesInner()
    {
        yield return new MyQuery() { MinDate = DateTime.Now };
        yield return new MyQuery() { MaxDate = DateTime.Now };
        yield return new MyQuery() { AssignedTo = "0" };
        yield return new MyQuery() { AssignedTo = "-1" };
        yield return new MyQuery() { AssignedTo = "anyone" };
    }
}
```
Now, in your unit test project, use the [QueryHelper.Test](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/QueryHelper_ext.cs#L16) method for each of your queries. A good way to test queries on a SQL Server localdb instance is to use my [SqlServer.LocalDb.Testing](https://github.com/adamfoneil/SqlServer.LocalDb) package. You can see how it's used in Dapper.QX's own [tests](https://github.com/adamfoneil/Dapper.QX/blob/master/Testing/ExecutionSqlServer.cs#L93).
```csharp
[TestClass]
public class QueryTests
{
    private SqlConnection GetConnection()
    {
      // implement as needed 
    }

    [TestMethod]
    public void MyQuery() => QueryHelper.Test<MyQuery>(GetConnection);    
}
```
## Table-Valued Parameters
To pass a TVP, create public property on your query class of type `DataTable` and add the `[TableType]` attribute with the name of the UDT in SQL Server it maps to. In this example, `Source` is the TVP property:
```csharp
internal class SimpleTvpExample : Query<int>
{
    public SimpleTvpExample() : base(
        @"DECLARE @list [IdList];
        INSERT INTO @list ([Id]) SELECT [Id] FROM @source;
        SELECT [Id] FROM @list")
    {
    }

    [TableType("[dbo].[IdList]")]
    public DataTable Source { get; set; }
}
```
Set the `Source` property like any other parameter. You can create a `DataTable` dynamically with the [ToDataTable](https://github.com/adamfoneil/Dapper.QX/blob/master/Dapper.QX/Extensions/EnumerableExtensions.cs#L17) extension method, as in the [test](https://github.com/adamfoneil/Dapper.QX/blob/master/Testing/ExecutionSqlServer.cs#L238):

```csharp
var qry = new SimpleTvpExample() { Source = new int[] { 1, 2, 3 }.ToDataTable() };
```

## Multiple WHERE clauses
You can inject multiple WHERE clauses in a query by adding a prefix on the **{where}** and **{andWhere}** tokens, making them like **{_prefix_:where}** or **{_prefix_:andWhere}**. See [issue 24](https://github.com/adamfoneil/Dapper.QX/issues/24) for more info.

## Debugging
To help you debug resolved SQL, place a breakpoint on any of the `Execute*` calls, and step over that line. Look in the Debug Output window to see the resolved SQL along with any parameter declarations. You can paste this directly into SSMS and execute.

![img](https://adamosoftware.blob.core.windows.net/images/dapper-qx-debug.png)

Note the extra indent you're seeing in the SQL is because of whitespace in the sample query's [source file](https://github.com/adamosoftware/Ginseng8/blob/dapper-qx/Ginseng8.Mvc/Queries/OpenWorkItems.cs#L218) from where I took this screenshot. In the source file, the SQL is stored with a verbatim string, so the indent is preserved.

Note also at this time, TVPs are not included in the debug output.

