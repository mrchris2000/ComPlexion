using System;
using System.Text.RegularExpressions;

namespace Complexion.Portable.Connection
{
	internal static class StringExtensions
	{
	    /// <summary>
		/// Check that a string is not null or empty
		/// </summary>
		/// <param name="input">String to check</param>
		/// <returns>bool</returns>
        internal static bool HasValue(this string input)
		{
			return !string.IsNullOrEmpty(input);
		}

		/// <summary>
		/// Remove underscores from a string
		/// </summary>
		/// <param name="input">String to process</param>
		/// <returns>string</returns>
        internal static string RemoveUnderscoresAndDashes(this string input)
		{
			return input.Replace("_", "").Replace("-", ""); // avoiding regex
		}

	    /// <summary>
	    /// Converts a string to pascal case with the option to remove underscores
	    /// </summary>
	    /// <param name="text">String to convert</param>
	    /// <param name="removeUnderscores">Option to remove underscores</param>
	    /// <returns></returns>
	    private static string ToPascalCase(this string text, bool removeUnderscores = true)
		{
			if (String.IsNullOrEmpty(text))
				return text;

			text = text.Replace("_", " ");
			var joinString = removeUnderscores ? String.Empty : "_";
			var words = text.Split(' ');
		    if (words.Length <= 1 && !words[0].IsUpperCase()) 
                return String.Concat(words[0].Substring(0, 1).ToUpper(), words[0].Substring(1));
		    for (var i = 0; i < words.Length; i++)
		    {
		        if (words[i].Length > 0)
		        {
		            var word = words[i];
		            var restOfWord = word.Substring(1);

		            if (restOfWord.IsUpperCase())
		                restOfWord = restOfWord.ToLower();

		            var firstChar = char.ToUpper(word[0]);
		            words[i] = String.Concat(firstChar, restOfWord);
		        }
		    }
		    return String.Join(joinString, words);
		}

		/// <summary>
		/// Converts a string to camel case
		/// </summary>
		/// <param name="lowercaseAndUnderscoredWord">String to convert</param>
		/// <returns>String</returns>
        internal static string ToCamelCase(this string lowercaseAndUnderscoredWord)
		{
			return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
		}

		/// <summary>
		/// Convert the first letter of a string to lower case
		/// </summary>
		/// <param name="word">String to convert</param>
		/// <returns>string</returns>
		private static string MakeInitialLowerCase(this string word)
		{
			return String.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
		}

		/// <summary>
		/// Checks to see if a string is all uppper case
		/// </summary>
		/// <param name="inputString">String to check</param>
		/// <returns>bool</returns>
		private static bool IsUpperCase(this string inputString)
		{
			return Regex.IsMatch(inputString, @"^[A-Z]+$");
		}
	}
}
