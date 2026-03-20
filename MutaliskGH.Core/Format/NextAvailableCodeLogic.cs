using System;
using System.Collections.Generic;
using System.Globalization;

namespace MutaliskGH.Core.Format
{
    public static class NextAvailableCodeLogic
    {
        public static Result<string> FindNext(object target, IEnumerable<object> takenValues)
        {
            return FindNext(target, takenValues, "{000###}");
        }

        public static Result<string> FindNext(object target, IEnumerable<object> takenValues, string format)
        {
            Result<NextAvailableCodeFormat> formatResult = NextAvailableCodeFormat.Parse(format);
            if (formatResult.IsFailure)
            {
                return Result<string>.Failure(formatResult.ErrorMessage);
            }

            NextAvailableCodeFormat parsedFormat = formatResult.Value;
            Result<string> targetResult = parsedFormat.NormalizeDigits(target, true);
            if (targetResult.IsFailure)
            {
                return Result<string>.Failure(targetResult.ErrorMessage);
            }

            string normalizedTarget = targetResult.Value;
            int targetSuffix = int.Parse(
                parsedFormat.ExtractSearchableDigits(normalizedTarget),
                CultureInfo.InvariantCulture);

            HashSet<int> takenSuffixes = new HashSet<int>();
            if (takenValues != null)
            {
                foreach (object takenValue in takenValues)
                {
                    Result<string> takenResult = parsedFormat.NormalizeDigits(takenValue, false);
                    if (takenResult.IsFailure || takenResult.Value == null)
                    {
                        continue;
                    }

                    if (!parsedFormat.FixedSlotsMatch(normalizedTarget, takenResult.Value))
                    {
                        continue;
                    }

                    takenSuffixes.Add(
                        int.Parse(
                            parsedFormat.ExtractSearchableDigits(takenResult.Value),
                            CultureInfo.InvariantCulture));
                }
            }

            int maxSearchableValue = (int)Math.Pow(10, parsedFormat.SearchableLength) - 1;
            if (takenSuffixes.Count >= maxSearchableValue + 1)
            {
                return Result<string>.Success(null);
            }

            if (!takenSuffixes.Contains(targetSuffix))
            {
                return Result<string>.Success(parsedFormat.FormatCode(normalizedTarget, targetSuffix));
            }

            for (int distance = 1; distance <= maxSearchableValue; distance++)
            {
                int upward = targetSuffix + distance;
                if (upward <= maxSearchableValue && !takenSuffixes.Contains(upward))
                {
                    return Result<string>.Success(parsedFormat.FormatCode(normalizedTarget, upward));
                }

                int downward = targetSuffix - distance;
                if (downward >= 0 && !takenSuffixes.Contains(downward))
                {
                    return Result<string>.Success(parsedFormat.FormatCode(normalizedTarget, downward));
                }
            }

            return Result<string>.Success(null);
        }
    }
}
