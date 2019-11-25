using Dapper.QX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    [TestClass]
    public class Extensions
    {
        [TestMethod]
        public void ContainsAnyShouldBeTrue()
        {
            string[] items = new string[] { "this", "that", "other", "hello" };
            bool result = items.ContainsAny(new string[] { "wonga", "other" });
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ContainsAnyShouldBeFalse()
        {
            string[] items = new string[] { "this", "that", "other", "hello" };
            bool result = items.ContainsAny(new string[] { "wonga", "witchita" });
            Assert.IsFalse(result);
        }
    }
}
