using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    public static class MultipleRegexIndexLogic
    {
        public static Result<MultipleRegexIndexResult> FindMatches(
            string value,
            IEnumerable<string> patterns)
        {
            if (value == null)
            {
                return Result<MultipleRegexIndexResult>.Failure("Input value cannot be null.");
            }

            if (patterns == null)
            {
                return Result<MultipleRegexIndexResult>.Failure("Pattern list cannot be null.");
            }

            int? firstMatchIndex = null;
            List<int> allMatchIndexes = new List<int>();

            int index = 0;
            foreach (string pattern in patterns)
            {
                Result<Regex> regexResult = RegexPatternMatcher.CreateRegex(pattern, RegexQueryMode.Raw, index);
                if (regexResult.IsFailure)
                {
                    return Result<MultipleRegexIndexResult>.Failure(regexResult.ErrorMessage);
                }

                if (regexResult.Value != null && regexResult.Value.IsMatch(value))
                {
                    if (!firstMatchIndex.HasValue)
                    {
                        firstMatchIndex = index;
                    }

                    allMatchIndexes.Add(index);
                }

                index++;
            }

            return Result<MultipleRegexIndexResult>.Success(
                new MultipleRegexIndexResult(firstMatchIndex, allMatchIndexes));
        }
    }
}
