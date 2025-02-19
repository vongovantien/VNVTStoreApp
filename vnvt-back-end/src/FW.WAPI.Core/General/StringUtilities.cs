using FW.WAPI.Core.DAL.Model.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FW.WAPI.Core.General
{
    public static class StringUtilities
    {
        /// <summary>
        /// Removes first occurrence of the given prefixes from beginning of the given string.
        /// Ordering is important. If one of the preFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="preFixes">one or more prefix.</param>
        /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (preFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Gets a substring of a string from end of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Right(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// Indicates whether this string is null or an System.String.Empty string.
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Removes first occurrence of the given postfixes from end of the given string.
        /// Ordering is important. If one of the postFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Left(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(0, len);
        }
    }

    // <summary>
    /// This class is used to extract dynamic values from a formatted string.
    /// It works as reverse of <see cref="string.Format(string,object)"/>
    /// </summary>
    /// <example>
    /// Say that str is "My name is Neo." and format is "My name is {name}.".
    /// Then Extract method gets "Neo" as "name".
    /// </example>
    public class FormattedStringValueExtracter
    {
        /// <summary>
        /// Extracts dynamic values from a formatted string.
        /// </summary>
        /// <param name="str">String including dynamic values</param>
        /// <param name="format">Format of the string</param>
        /// <param name="ignoreCase">True, to search case-insensitive.</param>
        /// <param name="splitformatCharacter">format is splitted using this character when provided.</param>
        public ExtractionResult Extract(string str, string format, bool ignoreCase = false, char? splitformatCharacter = null)
        {
            var stringComparison = ignoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (str == format)
            {
                return new ExtractionResult(true);
            }

            var formatTokens = TokenizeFormat(format, splitformatCharacter);

            if (formatTokens.IsNullOrEmpty())
            {
                return new ExtractionResult(str == "");
            }

            var result = new ExtractionResult(false);

            for (var i = 0; i < formatTokens.Count; i++)
            {
                var currentToken = formatTokens[i];
                var previousToken = i > 0 ? formatTokens[i - 1] : null;

                if (currentToken.Type == FormatStringTokenType.ConstantText)
                {
                    if (i == 0)
                    {
                        if (str.StartsWith(currentToken.Text, stringComparison))
                        {
                            str = str.Substring(currentToken.Text.Length);
                        }
                    }
                    else
                    {
                        var matchIndex = str.IndexOf(currentToken.Text, stringComparison);
                        if (matchIndex >= 0)
                        {
                            result.Matches.Add(new NameValue(previousToken.Text, str.Substring(0, matchIndex)));
                            result.IsMatch = true;
                            str = str.Substring(matchIndex + currentToken.Text.Length);
                        }
                    }
                }
            }

            var lastToken = formatTokens.Last();
            if (lastToken.Type == FormatStringTokenType.DynamicValue)
            {
                result.Matches.Add(new NameValue(lastToken.Text, str));
                result.IsMatch = true;
            }

            return result;
        }

        private List<FormatStringToken> TokenizeFormat(string originalFormat, char? splitformatCharacter = null)
        {
            if (splitformatCharacter == null)
            {
                return new FormatStringTokenizer().Tokenize(originalFormat);
            }

            var result = new List<FormatStringToken>();
            var formats = originalFormat.Split(splitformatCharacter.Value);

            foreach (var format in formats)
            {
                result.AddRange(new FormatStringTokenizer().Tokenize(format));
            }

            return result;
        }

        /// <summary>
        /// Checks if given <see cref="str"/> fits to given <see cref="format"/>.
        /// Also gets extracted values.
        /// </summary>
        /// <param name="str">String including dynamic values</param>
        /// <param name="format">Format of the string</param>
        /// <param name="values">Array of extracted values if matched</param>
        /// <param name="ignoreCase">True, to search case-insensitive</param>
        /// <returns>True, if matched.</returns>
        public static bool IsMatch(string str, string format, out string[] values, bool ignoreCase = false)
        {
            var result = new FormattedStringValueExtracter().Extract(str, format, ignoreCase);
            if (!result.IsMatch)
            {
                values = new string[0];
                return false;
            }

            values = result.Matches.Select(m => m.Value).ToArray();
            return true;
        }

        /// <summary>
        /// Used as return value of <see cref="Extract"/> method.
        /// </summary>
        public class ExtractionResult
        {
            /// <summary>
            /// Is fully matched.
            /// </summary>
            public bool IsMatch { get; set; }

            /// <summary>
            /// List of matched dynamic values.
            /// </summary>
            public List<NameValue> Matches { get; private set; }

            internal ExtractionResult(bool isMatch)
            {
                IsMatch = isMatch;
                Matches = new List<NameValue>();
            }
        }
    }

    internal class FormatStringTokenizer
    {
        public List<FormatStringToken> Tokenize(string format, bool includeBracketsForDynamicValues = false)
        {
            var tokens = new List<FormatStringToken>();

            var currentText = new StringBuilder();
            var inDynamicValue = false;

            for (var i = 0; i < format.Length; i++)
            {
                var c = format[i];
                switch (c)
                {
                    case '{':
                        if (inDynamicValue)
                        {
                            throw new FormatException("Incorrect syntax at char " + i + "! format string can not contain nested dynamic value expression!");
                        }

                        inDynamicValue = true;

                        if (currentText.Length > 0)
                        {
                            tokens.Add(new FormatStringToken(currentText.ToString(), FormatStringTokenType.ConstantText));
                            currentText.Clear();
                        }

                        break;

                    case '}':
                        if (!inDynamicValue)
                        {
                            throw new FormatException("Incorrect syntax at char " + i + "! These is no opening brackets for the closing bracket }.");
                        }

                        inDynamicValue = false;

                        if (currentText.Length <= 0)
                        {
                            throw new FormatException("Incorrect syntax at char " + i + "! Brackets does not containt any chars.");
                        }

                        var dynamicValue = currentText.ToString();
                        if (includeBracketsForDynamicValues)
                        {
                            dynamicValue = "{" + dynamicValue + "}";
                        }

                        tokens.Add(new FormatStringToken(dynamicValue, FormatStringTokenType.DynamicValue));
                        currentText.Clear();

                        break;

                    default:
                        currentText.Append(c);
                        break;
                }
            }

            if (inDynamicValue)
            {
                throw new FormatException("There is no closing } char for an opened { char.");
            }

            if (currentText.Length > 0)
            {
                tokens.Add(new FormatStringToken(currentText.ToString(), FormatStringTokenType.ConstantText));
            }

            return tokens;
        }
    }
}