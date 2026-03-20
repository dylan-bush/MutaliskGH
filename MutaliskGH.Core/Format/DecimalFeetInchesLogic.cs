using System;

namespace MutaliskGH.Core.Format
{
    public static class DecimalFeetInchesLogic
    {
        public static Result<DecimalFeetInchesResult> Convert(
            double decimalInches,
            int denominator,
            int roundingMode,
            bool showZeroInches)
        {
            if (double.IsNaN(decimalInches) || double.IsInfinity(decimalInches))
            {
                return Result<DecimalFeetInchesResult>.Failure("Decimal inches must be a finite number.");
            }

            if (denominator <= 0)
            {
                return Result<DecimalFeetInchesResult>.Failure("Denominator must be a positive integer.");
            }

            if (roundingMode < 0 || roundingMode > 2)
            {
                return Result<DecimalFeetInchesResult>.Failure("Rounding mode must be 0 (nearest), 1 (up), or 2 (down).");
            }

            bool isNegative = decimalInches < 0.0;
            double absoluteInches = Math.Abs(decimalInches);
            double roundedAbsoluteInches = RoundToDenominator(absoluteInches, denominator, roundingMode);

            int totalNumerator = (int)Math.Round(
                roundedAbsoluteInches * denominator,
                MidpointRounding.AwayFromZero);

            int feet = totalNumerator / (12 * denominator);
            int inchesNumerator = totalNumerator - (feet * 12 * denominator);
            int wholeInches = inchesNumerator / denominator;
            int fractionNumerator = inchesNumerator % denominator;

            if (wholeInches == 12)
            {
                feet++;
                wholeInches = 0;
                fractionNumerator = 0;
            }

            if (fractionNumerator != 0)
            {
                int divisor = GreatestCommonDivisor(fractionNumerator, denominator);
                fractionNumerator /= divisor;
                denominator /= divisor;
            }

            string formatted = Format(feet, wholeInches, fractionNumerator, denominator, showZeroInches);
            if (isNegative && roundedAbsoluteInches > 0.0)
            {
                formatted = "-" + formatted;
            }

            double roundedSignedInches = isNegative ? -roundedAbsoluteInches : roundedAbsoluteInches;
            return Result<DecimalFeetInchesResult>.Success(
                new DecimalFeetInchesResult(formatted, roundedSignedInches));
        }

        private static double RoundToDenominator(double value, int denominator, int roundingMode)
        {
            double scaled = value * denominator;
            switch (roundingMode)
            {
                case 1:
                    return Math.Ceiling(scaled) / denominator;
                case 2:
                    return Math.Floor(scaled) / denominator;
                default:
                    return Math.Round(scaled, MidpointRounding.AwayFromZero) / denominator;
            }
        }

        private static string Format(
            int feet,
            int wholeInches,
            int fractionNumerator,
            int fractionDenominator,
            bool showZeroInches)
        {
            bool hasInches = wholeInches > 0 || fractionNumerator > 0;
            bool showFeet = feet > 0;

            if (!showFeet && !hasInches)
            {
                return "0\"";
            }

            string inchesText = BuildInchesText(wholeInches, fractionNumerator, fractionDenominator);

            if (showFeet)
            {
                if (hasInches)
                {
                    return feet + "'-" + inchesText + "\"";
                }

                if (showZeroInches)
                {
                    return feet + "'-0\"";
                }

                return feet + "'";
            }

            return inchesText + "\"";
        }

        private static string BuildInchesText(int wholeInches, int fractionNumerator, int fractionDenominator)
        {
            if (fractionNumerator == 0)
            {
                return wholeInches.ToString();
            }

            if (wholeInches == 0)
            {
                return fractionNumerator + "/" + fractionDenominator;
            }

            return wholeInches + " " + fractionNumerator + "/" + fractionDenominator;
        }

        private static int GreatestCommonDivisor(int left, int right)
        {
            left = Math.Abs(left);
            right = Math.Abs(right);

            while (right != 0)
            {
                int remainder = left % right;
                left = right;
                right = remainder;
            }

            return left == 0 ? 1 : left;
        }
    }
}
