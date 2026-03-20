using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    public static class TextMatchMultipleLogic
    {
        public static Result<TextMatchMultipleResult> Evaluate(
            string text,
            IEnumerable<string> queries,
            RegexQueryMode mode)
        {
            if (text == null)
            {
                return Result<TextMatchMultipleResult>.Failure("Input text cannot be null.");
            }

            if (queries == null)
            {
                return Result<TextMatchMultipleResult>.Failure("Query list cannot be null.");
            }

            List<bool> matches = new List<bool>();
            List<int> matchingIndexes = new List<int>();
            List<int> nonMatchingIndexes = new List<int>();

            int index = 0;
            foreach (string query in queries)
            {
                Result<Regex> regexResult = RegexPatternMatcher.CreateRegex(query, mode, index);
                if (regexResult.IsFailure)
                {
                    return Result<TextMatchMultipleResult>.Failure(regexResult.ErrorMessage);
                }

                bool isMatch = regexResult.Value != null && regexResult.Value.IsMatch(text);
                matches.Add(isMatch);

                if (isMatch)
                {
                    matchingIndexes.Add(index);
                }
                else
                {
                    nonMatchingIndexes.Add(index);
                }

                index++;
            }

            return Result<TextMatchMultipleResult>.Success(
                new TextMatchMultipleResult(matches, matchingIndexes, nonMatchingIndexes));
        }
    }
}
