using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dapper.QX.Extensions
{
    public static class RegexHelper
    {
        public static QueryParameters ParseParameters(string sql, bool cleaned = false)
        {
            var result = new QueryParameters();
            result.Optional = ParseOptionalTokens(sql, cleaned).ToArray();

            string baseSql = RemoveOptionalTokens(sql);
            result.Required = ParseParameterNames(baseSql, cleaned).ToArray();

            return result;
        }

        public static string RemovePlaceholders(string input)
        {
            string result = input;

            var optional = ParseOptionalTokens(result);
            foreach (var o in optional) result = result.Replace(o.Token, string.Empty);

            var remaining = ParsePlaceholders(result);
            foreach (var ph in remaining) result = result.Replace(ph, string.Empty);

            return result;
        }

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

        public static IEnumerable<string> ParsePlaceholders(string input)
        {            
            var regexes = new string[] { @"\{([^}]+)\}", @"\%([^%]+)\%" };
            foreach (var pattern in regexes)
            {
                var matches = Regex.Matches(input, pattern);
                foreach (Match m in matches) yield return m.Value;
            }
        }

        public static IEnumerable<OptionalToken> ParseOptionalTokens(string input, bool cleaned = false)
        {
            // thanks to https://www.regextester.com/97707
            
            const string optionalRegex = @"\[{2}(.*)\]{2}";
            const int markerLength = 2; // length of "{{" and "}}"

            return Regex.Matches(input, optionalRegex).OfType<Match>().Select(m => new OptionalToken()
            {
                Match = m,
                Token = m.Value,
                Content = m.Value.Substring(markerLength, m.Value.Length - (markerLength * 2)).Trim(),
                ParameterNames = ParseParameterNames(m.Value, cleaned).ToArray()                
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

    public class QueryParameters
    {
        public string[] Required { get; set; }
        public OptionalToken[] Optional { get; set; }

        public bool IsOptional(PropertyInfo pi)
        {
            return !IsRequired(pi);
        }

        public bool IsRequired(PropertyInfo pi)
        {
            return Required.Select(p => p.ToLower()).Contains(pi.Name.ToLower());
        }

        public IEnumerable<string> AllParamNames()
        {
            return Required.Concat(Optional.SelectMany(o => o.ParameterNames)).Distinct();
        }        
    }

    public class OptionalToken
    {
        public Match Match { get; set; }
        public string Token { get; set; }
        public string Content { get; set; }
        public string[] ParameterNames { get; set; }
        public string[] CleanedParameterNames { get; set; }

        /// <summary>
        /// These parameters were
        /// </summary>
        public string[] Required { get; set; }
    }
}
