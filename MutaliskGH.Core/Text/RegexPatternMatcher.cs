using System;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    internal static class RegexPatternMatcher
    {
        public static Result<Regex> CreateRegex(string pattern, RegexQueryMode mode, int patternIndex)
        {
            if (pattern == null)
            {
                return Result<Regex>.Success(null);
            }

            if (!Enum.IsDefined(typeof(RegexQueryMode), mode))
            {
                return Result<Regex>.Failure("Unsupported regex query mode: " + (int)mode);
            }

            string effectivePattern = mode == RegexQueryMode.Exact
                ? "^" + Regex.Escape(pattern) + "$"
                : pattern;

            try
            {
                return Result<Regex>.Success(new Regex(effectivePattern, RegexOptions.CultureInvariant));
            }
            catch (ArgumentException exception)
            {
                return Result<Regex>.Failure(
                    "Invalid regex pattern at index " + patternIndex + ": '" + pattern + "' - " + exception.Message);
            }
        }
    }
}
