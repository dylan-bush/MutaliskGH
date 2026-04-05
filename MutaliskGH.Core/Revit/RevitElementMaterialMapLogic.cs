using System;

namespace MutaliskGH.Core.Revit
{
    public static class RevitElementMaterialMapLogic
    {
        public static RevitElementMaterialMapSettings CreateSettings(object minVolume, object detail, object emitGeometry, object debug)
        {
            return new RevitElementMaterialMapSettings(
                NormalizeMinimumVolume(minVolume),
                NormalizeDetail(detail),
                NormalizeBoolean(emitGeometry, true),
                NormalizeBoolean(debug, false));
        }

        public static double NormalizeMinimumVolume(object value)
        {
            if (value == null)
            {
                return 1e-9;
            }

            try
            {
                double parsed = Convert.ToDouble(value);
                return parsed > 0.0 ? parsed : 1e-9;
            }
            catch
            {
                return 1e-9;
            }
        }

        public static string NormalizeDetail(object value)
        {
            if (value == null)
            {
                return "Coarse";
            }

            if (value is string text)
            {
                string lowered = text.Trim().ToLowerInvariant();
                if (lowered.StartsWith("m"))
                {
                    return "Medium";
                }

                if (lowered.StartsWith("f"))
                {
                    return "Fine";
                }

                return "Coarse";
            }

            try
            {
                int index = Convert.ToInt32(value);
                if (index == 1)
                {
                    return "Medium";
                }

                if (index == 2)
                {
                    return "Fine";
                }
            }
            catch
            {
            }

            return "Coarse";
        }

        public static bool NormalizeBoolean(object value, bool defaultValue)
        {
            if (value == null)
            {
                return defaultValue;
            }

            if (value is bool boolean)
            {
                return boolean;
            }

            try
            {
                return Convert.ToDouble(value) != 0.0;
            }
            catch
            {
            }

            string text = value.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return defaultValue;
            }

            switch (text.Trim().ToLowerInvariant())
            {
                case "true":
                case "t":
                case "yes":
                case "y":
                case "on":
                case "1":
                    return true;
                case "false":
                case "f":
                case "no":
                case "n":
                case "off":
                case "0":
                    return false;
                default:
                    return defaultValue;
            }
        }
    }
}
