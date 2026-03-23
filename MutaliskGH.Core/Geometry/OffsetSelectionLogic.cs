using System;

namespace MutaliskGH.Core.Geometry
{
    public static class OffsetSelectionLogic
    {
        public static Result<OffsetSelectionResult> Select(int mode, OffsetCandidateValue positive, OffsetCandidateValue negative)
        {
            if (mode < 0 || mode > 2)
            {
                return Result<OffsetSelectionResult>.Failure("Offset mode must be 0 for smaller, 1 for larger, or 2 for both.");
            }

            bool hasPositive = positive != null && positive.Exists;
            bool hasNegative = negative != null && negative.Exists;

            if (!hasPositive && !hasNegative)
            {
                return Result<OffsetSelectionResult>.Failure("No offset result could be generated.");
            }

            if (mode == 2)
            {
                return Result<OffsetSelectionResult>.Success(
                    new OffsetSelectionResult(BuildBothOrder(positive, negative)));
            }

            if (hasPositive && !hasNegative)
            {
                return Result<OffsetSelectionResult>.Success(new OffsetSelectionResult(new[] { 1 }));
            }

            if (!hasPositive && hasNegative)
            {
                return Result<OffsetSelectionResult>.Success(new OffsetSelectionResult(new[] { -1 }));
            }

            int comparison = Compare(positive, negative);
            if (comparison == 0)
            {
                return Result<OffsetSelectionResult>.Success(new OffsetSelectionResult(new[] { 1 }));
            }

            if (mode == 1)
            {
                return Result<OffsetSelectionResult>.Success(
                    new OffsetSelectionResult(new[] { comparison > 0 ? 1 : -1 }));
            }

            return Result<OffsetSelectionResult>.Success(
                new OffsetSelectionResult(new[] { comparison > 0 ? -1 : 1 }));
        }

        private static int[] BuildBothOrder(OffsetCandidateValue positive, OffsetCandidateValue negative)
        {
            bool hasPositive = positive != null && positive.Exists;
            bool hasNegative = negative != null && negative.Exists;

            if (hasPositive && !hasNegative)
            {
                return new[] { 1 };
            }

            if (!hasPositive && hasNegative)
            {
                return new[] { -1 };
            }

            if (!hasPositive)
            {
                return Array.Empty<int>();
            }

            int comparison = Compare(positive, negative);
            if (comparison > 0)
            {
                return new[] { -1, 1 };
            }

            return new[] { 1, -1 };
        }

        private static int Compare(OffsetCandidateValue positive, OffsetCandidateValue negative)
        {
            const double epsilon = 1e-9;

            double metricDifference = positive.Metric - negative.Metric;
            if (Math.Abs(metricDifference) > epsilon)
            {
                return metricDifference > 0.0 ? 1 : -1;
            }

            double lengthDifference = positive.Length - negative.Length;
            if (Math.Abs(lengthDifference) > epsilon)
            {
                return lengthDifference > 0.0 ? 1 : -1;
            }

            return 0;
        }
    }
}
