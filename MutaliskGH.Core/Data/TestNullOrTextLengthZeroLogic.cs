using System;
using System.Globalization;

namespace MutaliskGH.Core.Data
{
    public static class TestNullOrTextLengthZeroLogic
    {
        public static Result<bool> Evaluate(object value)
        {
            if (value == null)
            {
                return Result<bool>.Success(false);
            }

            bool booleanValue;
            if (value is bool)
            {
                booleanValue = (bool)value;
                return Result<bool>.Success(booleanValue);
            }

            string text = value as string;
            if (text != null)
            {
                string normalized = text.Trim();
                if (normalized.Length == 0)
                {
                    return Result<bool>.Success(false);
                }

                if (string.Equals(normalized, "false", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalized, "0", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalized, "off", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalized, "null", StringComparison.OrdinalIgnoreCase))
                {
                    return Result<bool>.Success(false);
                }

                return Result<bool>.Success(true);
            }

            if (value is IConvertible)
            {
                IConvertible convertible = (IConvertible)value;
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        decimal numericValue = convertible.ToDecimal(CultureInfo.InvariantCulture);
                        return Result<bool>.Success(numericValue != 0m);
                }
            }

            return Result<bool>.Success(true);
        }
    }
}
