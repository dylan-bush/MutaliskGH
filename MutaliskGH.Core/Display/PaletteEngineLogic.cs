using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MutaliskGH.Core.Display
{
    public static class PaletteEngineLogic
    {
        public static Result<PaletteEngineResult> Generate(
            IReadOnlyList<object> values,
            double variationStrength,
            object seed,
            bool overdrive,
            double minimumSaturation,
            double saturationBoost)
        {
            if (values == null)
            {
                return Result<PaletteEngineResult>.Failure("A value list is required.");
            }

            if (values.Count == 0)
            {
                return Result<PaletteEngineResult>.Success(
                    new PaletteEngineResult(
                        Array.Empty<PaletteColorValue>(),
                        new Dictionary<string, IReadOnlyList<PaletteColorValue>>()));
            }

            if (minimumSaturation < 0.0 || minimumSaturation > 1.0)
            {
                return Result<PaletteEngineResult>.Failure("Minimum saturation must be in the range 0..1.");
            }

            if (saturationBoost < 0.0 || saturationBoost > 1.0)
            {
                return Result<PaletteEngineResult>.Failure("Saturation boost must be in the range 0..1.");
            }

            double strength = Math.Max(0.0, variationStrength);
            string seedText = NormalizeSeed(seed);

            List<string> normalizedValues = values
                .Select(NormalizeLabel)
                .ToList();

            List<string> groupOrder = new List<string>();
            Dictionary<string, int> counts = new Dictionary<string, int>();
            for (int index = 0; index < normalizedValues.Count; index++)
            {
                string label = normalizedValues[index];
                if (!counts.ContainsKey(label))
                {
                    counts[label] = 0;
                    groupOrder.Add(label);
                }

                counts[label] += 1;
            }

            Dictionary<string, double> baseHues = AssignGroupBaseHues(groupOrder, seedText, 0.085, 48);
            Dictionary<string, IReadOnlyList<PaletteColorValue>> palettes =
                new Dictionary<string, IReadOnlyList<PaletteColorValue>>();

            foreach (string label in groupOrder)
            {
                palettes[label] = GenerateGroupColors(
                    baseHues[label],
                    counts[label],
                    strength,
                    overdrive,
                    minimumSaturation,
                    saturationBoost);
            }

            Dictionary<string, int> cursors = groupOrder.ToDictionary(label => label, label => 0);
            List<PaletteColorValue> output = new List<PaletteColorValue>(normalizedValues.Count);
            for (int index = 0; index < normalizedValues.Count; index++)
            {
                string label = normalizedValues[index];
                int cursor = cursors[label];
                output.Add(palettes[label][cursor]);
                cursors[label] = cursor + 1;
            }

            return Result<PaletteEngineResult>.Success(new PaletteEngineResult(output, palettes));
        }

        public static string NormalizeLabel(object value)
        {
            if (value == null)
            {
                return "<null>";
            }

            return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
        }

        private static Dictionary<string, double> AssignGroupBaseHues(
            IReadOnlyList<string> labels,
            string seedText,
            double minimumSeparation,
            int maximumTries)
        {
            Dictionary<string, double> hues = new Dictionary<string, double>();
            List<double> placed = new List<double>();

            for (int labelIndex = 0; labelIndex < labels.Count; labelIndex++)
            {
                string label = labels[labelIndex];
                double hue = 0.0;
                bool placedSuccessfully = false;

                for (int attempt = 0; attempt < maximumTries; attempt++)
                {
                    double candidate = HueFromLabelAndSeed(label, seedText, attempt);
                    bool acceptable = true;
                    for (int existingIndex = 0; existingIndex < placed.Count; existingIndex++)
                    {
                        if (HueDistance(candidate, placed[existingIndex]) < minimumSeparation)
                        {
                            acceptable = false;
                            break;
                        }
                    }

                    hue = candidate;
                    if (acceptable)
                    {
                        placedSuccessfully = true;
                        break;
                    }
                }

                if (!placedSuccessfully)
                {
                    hue = HueFromLabelAndSeed(label, seedText, maximumTries);
                }

                hues[label] = hue;
                placed.Add(hue);
            }

            return hues;
        }

        private static IReadOnlyList<PaletteColorValue> GenerateGroupColors(
            double baseHue,
            int count,
            double strength,
            bool overdrive,
            double minimumSaturation,
            double saturationBoost)
        {
            List<PaletteColorValue> colors = new List<PaletteColorValue>(count);
            if (count <= 0)
            {
                return colors;
            }

            double normal = Clamp01(strength);
            double extra = Math.Max(0.0, strength - 1.0);

            double hueVariation = 0.03 + (0.10 * normal);
            double lightVariation = 0.07 + (0.18 * normal);
            double saturationVariation = 0.06 + (0.16 * normal);

            if (overdrive || extra > 0.0)
            {
                double boost = Math.Log(1.0 + (extra * 3.0));
                hueVariation = Math.Min(0.35, hueVariation + (0.08 * boost));
                lightVariation = Math.Min(0.38, lightVariation + (0.12 * boost));
                saturationVariation = Math.Min(0.35, saturationVariation + (0.10 * boost));
            }

            if (count == 1)
            {
                double saturation = ApplySaturationFloorAndBoost(0.56, 0.58, minimumSaturation, saturationBoost);
                colors.Add(PaletteColorValue.FromHsl(baseHue, 0.56, saturation));
                return colors;
            }

            double step = (overdrive || extra > 0.0) ? 0.61803398875 : 0.38196601125;
            for (int index = 0; index < count; index++)
            {
                double t = index / (double)(count - 1);
                double u = (t * 2.0) - 1.0;
                double sign = index % 2 == 0 ? 1.0 : -1.0;
                double phase = (((index * step) % 1.0) - 0.5) * 2.0;

                double hue = Wrap01(baseHue + (sign * hueVariation * phase));
                double lightness = 0.56 + (u * lightVariation);
                double saturation = 0.58 - (u * saturationVariation * 0.85);

                if (overdrive || extra > 0.0)
                {
                    double wobble = Math.Sin((index + 1) * 2.09439510239);
                    double amount = Math.Min(1.0, 0.5 + extra);
                    lightness += wobble * 0.04 * amount;
                    saturation -= wobble * 0.04 * amount;
                }

                lightness = Clamp01(Math.Min(0.86, Math.Max(0.22, lightness)));
                saturation = Clamp01(Math.Min(0.92, Math.Max(0.12, saturation)));
                saturation = ApplySaturationFloorAndBoost(lightness, saturation, minimumSaturation, saturationBoost);

                colors.Add(PaletteColorValue.FromHsl(hue, lightness, saturation));
            }

            return colors;
        }

        private static string NormalizeSeed(object seed)
        {
            if (seed == null)
            {
                return "None";
            }

            if (seed is sbyte || seed is byte || seed is short || seed is ushort ||
                seed is int || seed is uint || seed is long || seed is ulong ||
                seed is float || seed is double || seed is decimal)
            {
                double numericSeed = Convert.ToDouble(seed, System.Globalization.CultureInfo.InvariantCulture);
                return numericSeed.ToString("0.00000000", System.Globalization.CultureInfo.InvariantCulture);
            }

            return seed.ToString();
        }

        private static double HueFromLabelAndSeed(string label, string seedText, int attempt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(label + "|" + seedText + "|" + attempt);
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                uint value = BitConverter.ToUInt32(hash, 0);
                return (value % 1000000U) / 1000000.0;
            }
        }

        private static double HueDistance(double a, double b)
        {
            double distance = Math.Abs(a - b) % 1.0;
            return Math.Min(distance, 1.0 - distance);
        }

        private static double ApplySaturationFloorAndBoost(
            double lightness,
            double saturation,
            double minimumSaturation,
            double saturationBoost)
        {
            double mid = 1.0 - Math.Abs((lightness * 2.0) - 1.0);
            double clamped = Math.Max(saturation, minimumSaturation);
            return Clamp01(clamped + (saturationBoost * mid));
        }

        private static double Wrap01(double value)
        {
            double wrapped = value % 1.0;
            return wrapped < 0.0 ? wrapped + 1.0 : wrapped;
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0.0, Math.Min(1.0, value));
        }
    }
}
