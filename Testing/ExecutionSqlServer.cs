using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class ExecutionSqlServer : ExecutionBase
    {
        [TestMethod]
        public void Execute()
        {
            //InitSampleDb();
        }

        protected override IDbConnection GetConnection()
        {
            //string connectionString = GetConnectionString("SqlServer")
            throw new NotImplementedException();
        }
    }
}
