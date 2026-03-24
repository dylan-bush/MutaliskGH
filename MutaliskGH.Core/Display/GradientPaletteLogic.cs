using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public static class GradientPaletteLogic
    {
        public static Result<IReadOnlyList<PaletteColorValue>> CreateRainbow(int count)
        {
            if (count < 0)
            {
                return Result<IReadOnlyList<PaletteColorValue>>.Failure("A non-negative gradient count is required.");
            }

            List<PaletteColorValue> colors = new List<PaletteColorValue>(count);
            for (int index = 0; index < count; index++)
            {
                double t = count <= 1 ? 0.5 : index / (double)(count - 1);
                double hue = (2.0 / 3.0) * (1.0 - t);
                colors.Add(PaletteColorValue.FromHsl(hue, 0.55, 0.85));
            }

            return Result<IReadOnlyList<PaletteColorValue>>.Success(colors);
        }
    }
}
