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

        [TestMethod]
        public void DataTableWithEnum()
        {
            var data = new[]
            {
                new MessageInfo() { AppointmentId = 1, MessageId = "whatever", MessageType = MessageType.Email, Recipient = "adamo@nowhere.org" },
                new MessageInfo() { AppointmentId = 2, MessageId = "another", MessageType = MessageType.Text, Recipient = "223-3439" }
            }.ToDataTable();

            Assert.IsTrue(data.Columns["MessageType"].DataType.Equals(typeof(int)));
        }

        private enum MessageType
        {
            Email = 1,
            Text = 2
        }

        private class MessageInfo
        {
            public int AppointmentId { get; set; }
            public MessageType MessageType { get; set; }
            public string MessageId { get; set; }
            public string Recipient { get; set; }
        }
    }
}
