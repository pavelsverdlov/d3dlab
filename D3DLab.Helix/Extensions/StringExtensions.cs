// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensions.cs" company="Helix 3D Toolkit">
//   http://helixtoolkit.codeplex.com, license: MIT
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace HelixToolkit.Wpf.SharpDX
{
    using System.Collections;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// A regular expression containing "a one or more whitespaces" pattern.
        /// </summary>
        private static Regex oneOrMoreWhitespaces = new Regex(@"\s+", RegexOptions.Compiled);

        private static char[] whiteChars = new[] { ' ', '\t' };

        /// <summary>
        /// Splits the string on whitespace.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>Array of strings.</returns>
        public static string[] SplitOnWhitespace(this string input)
        {
            //return oneOrMoreWhitespaces.Split(input.Trim());
            if (string.IsNullOrWhiteSpace(input))
                return new string[0];
            return input.Split(whiteChars, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public static bool Contains(this string str, char c)
        {
            return str.IndexOf(c) >= 0;
        }

        /// <summary>
        /// Creates a string from the items in an enumerable.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>
        /// A string.
        /// </returns>
        public static string EnumerateToString(this IEnumerable items, string prefix = null, string separator = " ")
        {
            var builder = new StringBuilder();
            foreach (var item in items)
            {
                if (builder.Length > 0)
                {
                    builder.Append(separator);
                }

                if (prefix != null)
                {
                    builder.Append(prefix);
                }

                builder.Append(item);
            }

            return builder.ToString();
        }
    }
}