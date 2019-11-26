using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;

namespace Testing
{
    public abstract class ExecutionBase
    {
        protected abstract IDbConnection GetConnection();        

        protected string GetConnectionString(string name)
        {
            string json = File.ReadAllText("..\\..\\settings.json");
            var obj = JToken.Parse(json);
            return obj.SelectToken($"ConnectionStrings")[name].Value<string>();
        }
    }
}
