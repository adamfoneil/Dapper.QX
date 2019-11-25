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
}

using (var cn = GetConnection())
{
  var data = await new MyQuery() 
  {
    MinDate = DateTime.Now, 
    MaxDate = DateTime.Now.AddDays(30) 
  }.ExecuteAsync(cn);
}
```
Use **{where}** or **{andWhere}** tokens to indicate where dynamic criteria is inserted. Mix and match [Where](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Where.cs) and [Case](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Attributes/Case.cs) attributes on query class properties to control what criteria is injected. Learn about more attributes Dapper.QX supports.

You can also use inline optional SQL like this:

```
using (var cn = GetConnection())
{
  var data = await cn.QueryDynamicAsync<SomeResultClass>(
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

Make query classes testable with [ITestableQuery](https://github.com/adamosoftware/Dapper.QX/blob/master/Dapper.QX/Interfaces/ITestableQuery.cs).
