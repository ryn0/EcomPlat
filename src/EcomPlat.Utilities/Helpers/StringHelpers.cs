using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EcomPlat.Utilities.Helpers
{
    public class StringHelpers
    {
        public static string UrlKey(string p)
        {
            if (string.IsNullOrWhiteSpace(p))
            {
                return string.Empty;
            }

            // Step 1: Normalize the string to decompose accented characters.
            string normalized = p.Normalize(NormalizationForm.FormD);

            // Step 2: Remove non-ASCII characters (like accents).
            var stringBuilder = new StringBuilder();
            foreach (char c in normalized)
            {
                // Keep only base characters (e.g., 'e' instead of 'é').
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Step 3: Convert the cleaned string back to normal form.
            string cleaned = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Step 4: Replace special characters with meaningful equivalents.
            cleaned = cleaned.Replace("&", "and");

            // Step 5: Use regex to replace non-alphanumeric characters with a single space.
            var replaceRegex = Regex.Replace(cleaned, @"[^a-zA-Z0-9\s-]+", " ");

            // Step 6: Collapse multiple spaces or dashes into a single dash.
            var urlSafe = Regex.Replace(replaceRegex, @"[\s-]+", "-").Trim('-');

            // Step 7: Convert to lowercase.
            return urlSafe.ToLowerInvariant();
        }

        /// <summary>
        /// Truncates the given text to a specified maximum length without cutting words in half.
        /// If the text is truncated, an ellipsis ("...") is appended.
        /// </summary>
        /// <param name="input">The string to truncate.</param>
        /// <param name="maxLength">The maximum allowed length (default is 60).</param>
        /// <returns>A truncated string that does not exceed maxLength.</returns>
        public static string TruncateToNearestWord(string input, int maxLength = 60)
        {
            // If input is null/empty or already within the limit, return it as-is.
            if (string.IsNullOrWhiteSpace(input) || input.Length <= maxLength)
            {
                return input;
            }

            // Split into words (removing extra spaces).
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var sb = new StringBuilder();
            foreach (var word in words)
            {
                // +1 for the space we'll add before the word (except the first).
                // +3 for the ellipsis length ("...").
                int neededLength = sb.Length == 0 ? word.Length : sb.Length + 1 + word.Length;

                // Check if adding this word (plus the ellipsis) would exceed the limit.
                if (neededLength + 3 > maxLength)
                {
                    break;
                }

                // If it's not the first word, add a space before it.
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }

                sb.Append(word);
            }

            // Append the ellipsis if we actually truncated anything.
            if (sb.Length < input.Length)
            {
                sb.Append("...");
            }

            return sb.ToString();
        }
    }
}
