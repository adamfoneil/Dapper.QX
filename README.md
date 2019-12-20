[![Build status](https://ci.appveyor.com/api/projects/status/cyehxnqmbiwhwpqo?svg=true)](https://ci.appveyor.com/project/adamosoftware/dapper-qx)

Nuget package **Dapper.QX** makes inline SQL more powerful and testable via the [Query\<TResult\>](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Query_base.cs) class. Get the convenience, safety and capability of [Dapper](https://github.com/StackExchange/Dapper) with dynamic criteria, tracing, and full text queries.

```
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
```
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
Use **{where}** or **{andWhere}** tokens to indicate where dynamic criteria is inserted. Mix and match [Where](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Where.cs) and [Case](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Case.cs) attributes on query class properties to control what criteria is injected. [Learn about](https://github.com/adamosoftware/Dapper.QX/wiki/Reference) more attributes Dapper.QX offers.

To help you build C# result classes for any SQL query, I offer a free tool [Postulate.Zinger](https://github.com/adamosoftware/Postulate.Zinger).

## Testing
Make query classes testable with the [ITestableQuery](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Interfaces/ITestableQuery.cs) interface. This approach catches invalid SQL, but does not assert any particular query results.
```
public class MyQuery : Query<MyResultClass>, ITestableQuery
{
  // same code above omitted
  
  // implement GetTestCases method to return every parameter combination you need to test
  public IEnumerable<ITestableQuery> GetTestCases()
  {
    yield return new MyQuery() { MinDate = DateTime.Now };
    yield return new MyQuery() { MaxDate = DateTime.Now };
    yield return new MyQuery() { AssignedTo = "0" };
    yield return new MyQuery() { AssignedTo = "-1" };
    yield return new MyQuery() { AssignedTo = "anyone" };
  }
  
  // implement TestExecute the same way always
  public IEnumerable<dynamic> TestExecute(IDbConnection connection)
  {
    return TestExecuteHelper(connection);
  }
}
```
Now, in your unit test project, use the [QueryHelper.Test](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/QueryHelper_ext.cs#L16) method for each of your queries.
```
[TestClass]
public class QueryTests
{
  private SqlConnection GetConnection()
  {
    // implement as needed 
  }

  [TestMethod]
  public void MyQuery()
  {
    QueryHelper.Test<MyQuery>(GetConnection);
  }
}
```
