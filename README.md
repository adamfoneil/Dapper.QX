**Dapper.QX** makes inline SQL more powerful and testable via the [Query](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Query.cs) class. Get the convenience, safety and capability of [Dapper](https://github.com/StackExchange/Dapper) with dynamic criteria, tracing, and full text queries.

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
Use **{where}** or **{andWhere}** tokens to indicate where dynamic criteria is inserted. Mix and match [Where](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Where.cs) and [Case](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Case.cs) attributes on query class properties to control what criteria is injected. Learn about more attributes Dapper.QX offers.

To help you build C# result classes for any SQL query, I offer a free tool [Postulate.Ziner](https://github.com/adamosoftware/Postulate.Zinger).

You can also use the [ResolveQueryAsync](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/QueryHelper_ext.cs#L9) method to execute inline SQL with dynamic optional parts marked by double square brackets:

```
using (var cn = GetConnection())
{
  var data = await cn.ResolveQueryAsync<SomeResultClass>(
     @"SELECT * 
      FROM [whatever]
      WHERE
        1 = 1
        [[ AND [SomeDate]>=@minDate ]]
        [[ AND [SomeDate]<=@maxDate ]]
      ORDER BY [something]", new { minDate = DateTime.Now });
}
```
Because **maxDate** is not specified, the corresponding SQL surrounding it is omitted from the query.

Make query classes testable with [ITestableQuery](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Interfaces/ITestableQuery.cs). This testing approach catches invalid SQL, but does not assert any particular query results.
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
  
  // implement TestExecute with the same implementation always
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
