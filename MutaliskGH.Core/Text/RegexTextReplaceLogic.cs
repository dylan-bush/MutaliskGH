using System;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    public static class RegexTextReplaceLogic
    {
        public static Result<RegexTextReplaceResult> Replace(string input, string pattern, string replacement)
        {
            if (input == null)
            {
                return Result<RegexTextReplaceResult>.Failure("Input text cannot be null.");
            }

            if (pattern == null)
            {
                return Result<RegexTextReplaceResult>.Failure("Regex pattern cannot be null.");
            }

            if (pattern.Length == 0)
            {
                return Result<RegexTextReplaceResult>.Failure("Regex pattern cannot be empty.");
            }

            string safeReplacement = replacement ?? string.Empty;

            try
            {
                string output = Regex.Replace(input, pattern, safeReplacement);
                return Result<RegexTextReplaceResult>.Success(
                    new RegexTextReplaceResult(
                        output,
                        true,
                        string.Empty,
                        !string.Equals(output, input, StringComparison.Ordinal)));
            }
            catch (ArgumentException exception)
            {
                return Result<RegexTextReplaceResult>.Failure("Invalid regex pattern: " + exception.Message);
            }
            catch (Exception exception)
            {
                return Result<RegexTextReplaceResult>.Failure("Failed to replace regex text: " + exception.Message);
            }
        }
    }
}
