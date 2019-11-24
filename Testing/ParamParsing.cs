using Dapper.QX;
using Dapper.QX.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Testing
{
    [TestClass]
    public class ParamParsing
    {
        [TestMethod]
        public void SimpleParamParse()
        {
            string sample = "THIS is a STATEMENT @yes that sort of doesn't REALLY resemble SQL @no indeed IS NOT";
            var paramNames = RegexHelper.ParseParameterNames(sample);
            Assert.IsTrue(paramNames.SequenceEqual(new string[] { "@yes", "@no" }));
        }

        [TestMethod]
        public void OptionalParams()
        {
            string sql =
                @"SELECT [this], [that], [other]
                FROM [whatever]
                WHERE [wonga]=@whatever 
                    {{ AND [plingus]=@thangly }}
                    {{ AND [thardimus]=@yarbinshaw }}
                ORDER BY [crimson]";

            var tokens = RegexHelper.ParseOptionalTokens(sql);
            Assert.IsTrue(tokens.Select(t => t.ParameterName).SequenceEqual(new string[]
            {
                "@thangly", "@yarbinshaw"
            }));

            Assert.IsTrue(tokens.Select(t => t.Content).SequenceEqual(new string[]
            {
                "AND [plingus]=@thangly", "AND [thardimus]=@yarbinshaw"
            }));
        }        
    }
}
