using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapper.QX.Extensions
{
    public static class RegexHelper
    {
        /// <summary>
        /// Returns the defined parameter names within a SQL statement
        /// </summary>
        /// <param name="sql">SQL statement to analyze</param>
        /// <param name="cleaned">Set true to omit the leading @ sign</param>        
        public static IEnumerable<string> ParseParameterNames(this string sql, bool cleaned = false)
        {
            const string paramRegex = "@([a-zA-Z][a-zA-Z0-9_]*)";
            var matches = Regex.Matches(sql, paramRegex);
            return matches.OfType<Match>().Select(m => (cleaned) ? m.Value.Substring(1) : m.Value);
        }

        public static IEnumerable<OptionalToken> ParseOptionalTokens(string input)
        {
            // thanks to https://www.regextester.com/97707
            
            const string optionalRegex = @"\{{([^}}]+)\}}";
            const int markerLength = 2; // length of "{{" and "}}"

            return Regex.Matches(input, optionalRegex).OfType<Match>().Select(m => new OptionalToken()
            {
                Match = m,
                Token = m.Value,
                Content = m.Value.Substring(markerLength, m.Value.Length - (markerLength * 2)).Trim(),
                ParameterName = ParseParameterNames(m.Value).FirstOrDefault()
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

    public class OptionalToken
    {
        public Match Match { get; set; }
        public string Token { get; set; }
        public string Content { get; set; }
        public string ParameterName { get; set; }
    }
}
