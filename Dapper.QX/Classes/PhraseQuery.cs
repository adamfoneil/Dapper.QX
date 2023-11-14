using Dapper.QX.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dapper.QX.Classes
{
	internal class PhraseQuery
	{
		public IEnumerable<PhraseQueryToken> Tokens { get; }
		public DynamicParameters Parameters { get; }
		public string Expression { get; }

		public PhraseQuery(string propertyName, string input, PhraseAttribute phraseAttribute)
		{
			Tokens = Parse(input);
			Parameters = new DynamicParameters();
			var expressions = new List<string>();
			int index = 0;
			Tokens.ToList().ForEach((qt) =>
			{
				string paramName = $"{propertyName}{++index}";
				Parameters.Add(paramName, FormatValue(qt.Value, phraseAttribute.ConcatenateWildcards), DbType.String);
				expressions.Add(ParamExpression(qt.IsNegated, paramName, phraseAttribute.ConcatenateWildcards));
			});

			Expression = "(" + string.Join(" OR ", phraseAttribute.ColumnNames.Select(col => $"({string.Join(" AND ", expressions.Select(expr => $"{phraseAttribute.LeadingColumnDelimiter}{col}{phraseAttribute.EndingColumnDelimiter} {expr}"))})")) + ")";
		}

		/// <summary>
		/// This is for SqlCe compatibility, which requires wildcards in parameter values, not in executed SQL
		/// </summary>
		internal static string FormatValue(string value, bool wildcardsInResolvedSql)
		{
			return (wildcardsInResolvedSql) ? value : "%" + value + "%";
		}

		private static string ParamExpression(bool isNegated, string paramName, bool concatenateWildcards)
		{
			string expr = (concatenateWildcards) ? $"'%' + @{paramName} + '%'" : $"@{paramName}";
			return (!isNegated) ? $"LIKE {expr}" : $"NOT LIKE {expr}";
		}

		private static IEnumerable<PhraseQueryToken> Parse(string input)
		{
			var tokens = new List<PhraseQueryToken>();

			string AddTokens(string tempInput, IEnumerable<string> matches, Func<string, PhraseQueryToken> selector)
			{
				if (matches.Count() == 0) return tempInput;
				tokens.AddRange(matches.Select(s => selector.Invoke(s)));
				foreach (string m in matches) tempInput = tempInput.Replace(m, string.Empty);
				return tempInput;
			};

			string Unquote(string tempInput)
			{
				string result = tempInput;
				if (result.StartsWith("\"")) result = result.Substring(1);
				if (result.EndsWith("\"")) result = result.Substring(0, result.Length - 1);
				return result;
			};

			IEnumerable<string> GetMatches(string inputInner, string pattern)
			{
				return Regex.Matches(inputInner, pattern).OfType<Match>().Select(m => m.Value);
			}

			var quotedWords = GetMatches(input, "\"[^\"]*\"");
			string remainder = AddTokens(input, quotedWords, (s) => new PhraseQueryToken() { Value = Unquote(s) });

			var negatedQuoted = GetMatches(remainder, "-\"[^\"]*\"");
			remainder = AddTokens(remainder, negatedQuoted, (s) => new PhraseQueryToken() { Value = Unquote(s), IsNegated = true });

			var words = remainder.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());
			AddTokens(remainder, words, (m) => PhraseQueryToken.FromString(m));

			return tokens;
		}
	}

	internal class PhraseQueryToken
	{
		public string Value { get; set; }
		public bool IsNegated { get; set; }

		public static PhraseQueryToken FromString(string input)
		{
			return (input.StartsWith("-")) ?
				new PhraseQueryToken() { Value = input.Substring(1), IsNegated = true } :
				new PhraseQueryToken() { Value = input };
		}
	}
}
