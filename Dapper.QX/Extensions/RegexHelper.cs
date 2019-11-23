using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapper.QX.Extensions
{
    internal static class RegexHelper
    {
        /// <summary>
        /// Returns the defined parameter names within a SQL statement
        /// </summary>
        /// <param name="sql">SQL statement to analyze</param>
        /// <param name="cleaned">Set true to omit the leading @ sign</param>        
        internal static IEnumerable<string> GetParameterNames(this string sql, bool cleaned = false)
        {
            const string paramRegex = "@([a-zA-Z][a-zA-Z0-9_]*)";
            var matches = Regex.Matches(sql, paramRegex);
            return matches.OfType<Match>().Select(m => (cleaned) ? m.Value.Substring(1) : m.Value);
        }

        internal static IEnumerable<VariableToken> ParseOptionalTokens(string input)
        {
            // regex thanks to https://www.regextester.com/97707
            return Regex.Matches(input, @"\{([^}]+)\}").OfType<Match>().Select(m => new VariableToken()
            {
                Match = m,
                Token = m.Value,
                Content = m.Value.Substring(1, m.Value.Length - 2).Trim()
            });
        }

        public static string RemoveOptionalTokens(string input)
        {
            var optional = ParseOptionalTokens(input);
            string result = input;
            foreach (var opt in optional) result = result.Replace(opt.Token, string.Empty);
            return result;
        }
    }

    internal class VariableToken
    {
        public Match Match { get; set; }
        public string Token { get; set; }
        public string Content { get; set; }
    }
}
