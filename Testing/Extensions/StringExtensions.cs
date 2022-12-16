using System.Text.RegularExpressions;

namespace Testing
{
    public static class StringExtensions
    {
        public static string ReplaceWhitespace(this string input) => Regex.Replace(input, @"\s+", " ");
    }
}
