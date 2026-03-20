using System;
using System.Globalization;

namespace MutaliskGH.Core.Format
{
    public sealed class NextAvailableCodeFormat
    {
        private NextAvailableCodeFormat(
            string prefix,
            string pattern,
            string suffix,
            int searchableStart,
            int searchableLength,
            bool hasExplicitSearchableSlots)
        {
            Prefix = prefix;
            Pattern = pattern;
            Suffix = suffix;
            SearchableStart = searchableStart;
            SearchableLength = searchableLength;
            HasExplicitSearchableSlots = hasExplicitSearchableSlots;
        }

        public string Prefix { get; }

        public string Pattern { get; }

        public string Suffix { get; }

        public int TotalDigits
        {
            get { return Pattern.Length; }
        }

        public int SearchableStart { get; }

        public int SearchableLength { get; }

        public bool HasExplicitSearchableSlots { get; }

        public static Result<NextAvailableCodeFormat> Parse(string format)
        {
            string normalized = string.IsNullOrWhiteSpace(format) ? "{000###}" : format.Trim();

            int openingBrace = normalized.IndexOf('{');
            int closingBrace = normalized.IndexOf('}');

            if (openingBrace < 0 || closingBrace < 0 || closingBrace <= openingBrace)
            {
                return Result<NextAvailableCodeFormat>.Failure(
                    "Format must contain one placeholder group, such as {000###}.");
            }

            if (normalized.IndexOf('{', openingBrace + 1) >= 0 || normalized.IndexOf('}', closingBrace + 1) >= 0)
            {
                return Result<NextAvailableCodeFormat>.Failure(
                    "Format must contain exactly one placeholder group.");
            }

            string prefix = normalized.Substring(0, openingBrace);
            string pattern = normalized.Substring(openingBrace + 1, closingBrace - openingBrace - 1);
            string suffix = normalized.Substring(closingBrace + 1);

            if (pattern.Length == 0)
            {
                return Result<NextAvailableCodeFormat>.Failure(
                    "Format placeholder cannot be empty.");
            }

            int firstSearchable = -1;
            int lastSearchable = -1;

            for (int index = 0; index < pattern.Length; index++)
            {
                char character = pattern[index];
                if (character != '0' && character != '#')
                {
                    return Result<NextAvailableCodeFormat>.Failure(
                        "Format placeholder may only contain 0 and # characters.");
                }

                if (character == '#')
                {
                    if (firstSearchable < 0)
                    {
                        firstSearchable = index;
                    }

                    lastSearchable = index;
                }
            }

            if (firstSearchable >= 0)
            {
                for (int index = firstSearchable; index <= lastSearchable; index++)
                {
                    if (pattern[index] != '#')
                    {
                        return Result<NextAvailableCodeFormat>.Failure(
                            "Searchable # slots must form one contiguous span.");
                    }
                }

                return Result<NextAvailableCodeFormat>.Success(
                    new NextAvailableCodeFormat(
                        prefix,
                        pattern,
                        suffix,
                        firstSearchable,
                        lastSearchable - firstSearchable + 1,
                        true));
            }

            return Result<NextAvailableCodeFormat>.Success(
                new NextAvailableCodeFormat(
                    prefix,
                    pattern,
                    suffix,
                    0,
                    pattern.Length,
                    false));
        }

        public Result<string> NormalizeDigits(object value, bool failOnNull)
        {
            if (value == null)
            {
                return failOnNull
                    ? Result<string>.Failure("A target code is required.")
                    : Result<string>.Success(null);
            }

            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (string.IsNullOrWhiteSpace(text))
            {
                return failOnNull
                    ? Result<string>.Failure("A target code is required.")
                    : Result<string>.Success(null);
            }

            text = text.Trim();

            string formattedDigits;
            if (TryExtractFormattedDigits(text, out formattedDigits))
            {
                return Result<string>.Success(formattedDigits);
            }

            if (!LooksLikeRawNumericInput(text))
            {
                return failOnNull
                    ? Result<string>.Failure("Target code does not match the requested format.")
                    : Result<string>.Success(null);
            }

            return NormalizeRawDigits(text, failOnNull);
        }

        public string FormatCode(string normalizedTargetDigits, int searchableValue)
        {
            char[] digits = normalizedTargetDigits.ToCharArray();
            string searchableDigits = searchableValue.ToString(
                new string('0', SearchableLength),
                CultureInfo.InvariantCulture);

            for (int index = 0; index < SearchableLength; index++)
            {
                digits[SearchableStart + index] = searchableDigits[index];
            }

            return Prefix + new string(digits) + Suffix;
        }

        public string ExtractSearchableDigits(string normalizedDigits)
        {
            return normalizedDigits.Substring(SearchableStart, SearchableLength);
        }

        public bool FixedSlotsMatch(string targetDigits, string candidateDigits)
        {
            for (int index = 0; index < Pattern.Length; index++)
            {
                bool isSearchable = index >= SearchableStart && index < SearchableStart + SearchableLength;
                if (isSearchable)
                {
                    continue;
                }

                if (targetDigits[index] != candidateDigits[index])
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryExtractFormattedDigits(string text, out string digits)
        {
            digits = null;

            if (!text.StartsWith(Prefix, StringComparison.Ordinal) ||
                !text.EndsWith(Suffix, StringComparison.Ordinal))
            {
                return false;
            }

            int digitsLength = text.Length - Prefix.Length - Suffix.Length;
            if (digitsLength != TotalDigits)
            {
                return false;
            }

            string candidateDigits = text.Substring(Prefix.Length, digitsLength);
            if (!IsDigitsOnly(candidateDigits))
            {
                return false;
            }

            digits = candidateDigits;
            return true;
        }

        private Result<string> NormalizeRawDigits(string text, bool failOnNull)
        {
            if (text.Contains("."))
            {
                double parsedDouble;
                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedDouble))
                {
                    return Result<string>.Failure("Code values must be integer-like numbers.");
                }

                if (Math.Abs(parsedDouble - Math.Round(parsedDouble)) > 1e-9)
                {
                    return Result<string>.Failure("Code values must be integer-like numbers.");
                }

                text = ((long)Math.Round(parsedDouble)).ToString(CultureInfo.InvariantCulture);
            }

            if (!IsDigitsOnly(text))
            {
                char[] buffer = new char[text.Length];
                int count = 0;
                for (int index = 0; index < text.Length; index++)
                {
                    if (char.IsDigit(text[index]))
                    {
                        buffer[count++] = text[index];
                    }
                }

                if (count == 0)
                {
                    return failOnNull
                        ? Result<string>.Failure("Code values must contain digits.")
                        : Result<string>.Success(null);
                }

                text = new string(buffer, 0, count);
            }

            if (text.Length > TotalDigits)
            {
                return Result<string>.Failure(
                    "Code values must be " + TotalDigits.ToString(CultureInfo.InvariantCulture) + " digits or fewer.");
            }

            return Result<string>.Success(text.PadLeft(TotalDigits, '0'));
        }

        private static bool LooksLikeRawNumericInput(string text)
        {
            for (int index = 0; index < text.Length; index++)
            {
                char character = text[index];
                if (char.IsDigit(character) ||
                    char.IsWhiteSpace(character) ||
                    character == '.' ||
                    character == '-' ||
                    character == '_' ||
                    character == ',')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private static bool IsDigitsOnly(string text)
        {
            for (int index = 0; index < text.Length; index++)
            {
                if (!char.IsDigit(text[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
