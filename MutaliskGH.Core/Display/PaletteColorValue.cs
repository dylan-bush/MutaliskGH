using System;

namespace MutaliskGH.Core.Display
{
    public sealed class PaletteColorValue
    {
        public PaletteColorValue(int red, int green, int blue)
        {
            Red = Clamp(red);
            Green = Clamp(green);
            Blue = Clamp(blue);
        }

        public int Red { get; }

        public int Green { get; }

        public int Blue { get; }

        public static PaletteColorValue FromHsl(double hue, double lightness, double saturation)
        {
            hue = Wrap01(hue);
            lightness = Clamp01(lightness);
            saturation = Clamp01(saturation);

            double r;
            double g;
            double b;

            if (saturation <= 0.0)
            {
                r = lightness;
                g = lightness;
                b = lightness;
            }
            else
            {
                double q = lightness < 0.5
                    ? lightness * (1.0 + saturation)
                    : lightness + saturation - (lightness * saturation);
                double p = (2.0 * lightness) - q;

                r = HueToRgb(p, q, hue + (1.0 / 3.0));
                g = HueToRgb(p, q, hue);
                b = HueToRgb(p, q, hue - (1.0 / 3.0));
            }

            return new PaletteColorValue(
                (int)Math.Round(Clamp01(r) * 255.0),
                (int)Math.Round(Clamp01(g) * 255.0),
                (int)Math.Round(Clamp01(b) * 255.0));
        }

        private static int Clamp(int value)
        {
            return Math.Max(0, Math.Min(255, value));
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0.0, Math.Min(1.0, value));
        }

        private static double Wrap01(double value)
        {
            double wrapped = value % 1.0;
            return wrapped < 0.0 ? wrapped + 1.0 : wrapped;
        }

        private static double HueToRgb(double p, double q, double t)
        {
            t = Wrap01(t);

            if (t < (1.0 / 6.0))
            {
                return p + ((q - p) * 6.0 * t);
            }

            if (t < 0.5)
            {
                return q;
            }

            if (t < (2.0 / 3.0))
            {
                return p + ((q - p) * ((2.0 / 3.0) - t) * 6.0);
            }

            return p;
        }
    }
}
