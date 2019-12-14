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
                WHERE 
                    [wonga]=@whatever 
                    [[ AND [plingus]=@thangly ]]
                    [[ AND [thardimus]=@yarbinshaw ]]
                ORDER BY [crimson]";

            var tokens = RegexHelper.ParseOptionalTokens(sql);
            Assert.IsTrue(tokens.SelectMany(t => t.ParameterNames).SequenceEqual(new string[]
            {
                "@thangly", "@yarbinshaw"
            }));

            Assert.IsTrue(tokens.Select(t => t.Content).SequenceEqual(new string[]
            {
                "AND [plingus]=@thangly", "AND [thardimus]=@yarbinshaw"
            }));
        }        

        [TestMethod]
        public void ParseRequiredAndOptional()
        {
            string sql =
                @"SELECT [chintzy], [argenslard], [vuxitron]
                FROM [garbenslade]
                WHERE 
                    [helem]=@klaksod AND
                    [wilvip]=@horgunz
                    [[ AND [rembenslom]=@hoopsenfargle ]]
                    [[ AND ([enzelfrage]=@zahbenlious OR [yexelhor]=@craybentanz) ]]";

            var paramInfo = RegexHelper.ParseParameters(sql);

            Assert.IsTrue(paramInfo.Required.SequenceEqual(new string[] { "@klaksod", "@horgunz" }));
            Assert.IsTrue(paramInfo.Optional.Select(o => o.Content).SequenceEqual(new string[]
            {
                "AND [rembenslom]=@hoopsenfargle",
                "AND ([enzelfrage]=@zahbenlious OR [yexelhor]=@craybentanz)"
            }));
        }

        [TestMethod]
        public void ParseAllParamNames()
        {
            string sql =
                @"SELECT [chintzy], [argenslard], [vuxitron]
                FROM [garbenslade]
                WHERE 
                    [helem]=@klaksod AND
                    [wilvip]=@horgunz
                    {{ AND [rembenslom]=@hoopsenfargle }}
                    {{ AND ([enzelfrage]=@zahbenlious OR [yexelhor] IN (@craybentanz, @horgunz)) }}";

            var paramInfo = RegexHelper.ParseParameters(sql);
            Assert.IsTrue(paramInfo.AllParamNames().SequenceEqual(new string[]
            {
                "@klaksod", "@horgunz", "@hoopsenfargle", "@zahbenlious", "@craybentanz"
            }));
        }
    }
}
