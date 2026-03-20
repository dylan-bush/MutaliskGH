using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    public static class RegexCullLogic
    {
        public static Result<RegexCullResult<T>> Filter<T>(
            bool returnMatches,
            IEnumerable<string> testValues,
            IEnumerable<string> patterns,
            IEnumerable<T> items)
        {
            if (testValues == null)
            {
                return Result<RegexCullResult<T>>.Failure("Test value list cannot be null.");
            }

            if (patterns == null)
            {
                return Result<RegexCullResult<T>>.Failure("Pattern list cannot be null.");
            }

            if (items == null)
            {
                return Result<RegexCullResult<T>>.Failure("Data list cannot be null.");
            }

            List<string> testValueList = new List<string>(testValues);
            List<string> patternList = new List<string>(patterns);
            List<T> itemList = new List<T>(items);

            if (testValueList.Count != itemList.Count)
            {
                return Result<RegexCullResult<T>>.Failure(
                    "Test value list and data list must contain the same number of items.");
            }

            List<Regex> regexes = new List<Regex>();
            for (int patternIndex = 0; patternIndex < patternList.Count; patternIndex++)
            {
                Result<Regex> regexResult = RegexPatternMatcher.CreateRegex(
                    patternList[patternIndex],
                    RegexQueryMode.Raw,
                    patternIndex);

                if (regexResult.IsFailure)
                {
                    return Result<RegexCullResult<T>>.Failure(regexResult.ErrorMessage);
                }

                if (regexResult.Value != null)
                {
                    regexes.Add(regexResult.Value);
                }
            }

            List<T> filteredItems = new List<T>();
            List<T> culledItems = new List<T>();
            List<bool> matchFlags = new List<bool>();

            for (int index = 0; index < testValueList.Count; index++)
            {
                string value = testValueList[index] ?? string.Empty;
                bool isMatch = false;
                for (int regexIndex = 0; regexIndex < regexes.Count; regexIndex++)
                {
                    if (regexes[regexIndex].IsMatch(value))
                    {
                        isMatch = true;
                        break;
                    }
                }

                matchFlags.Add(isMatch);

                bool keepItem = returnMatches ? isMatch : !isMatch;
                if (keepItem)
                {
                    filteredItems.Add(itemList[index]);
                }
                else
                {
                    culledItems.Add(itemList[index]);
                }
            }

            return Result<RegexCullResult<T>>.Success(
                new RegexCullResult<T>(
                    filteredItems,
                    matchFlags,
                    BuildPatternSummary(patternList),
                    culledItems));
        }

        private static string BuildPatternSummary(IReadOnlyList<string> patternList)
        {
            if (patternList.Count == 0)
            {
                return string.Empty;
            }

            string summary = string.Empty;
            for (int index = 0; index < patternList.Count; index++)
            {
                if (patternList[index] == null)
                {
                    continue;
                }

                if (summary.Length > 0)
                {
                    summary += " OR ";
                }

                summary += patternList[index];
            }

            return summary;
        }
    }
}
