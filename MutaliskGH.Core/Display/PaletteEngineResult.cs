using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public sealed class PaletteEngineResult
    {
        public PaletteEngineResult(
            IReadOnlyList<PaletteColorValue> outputColors,
            IReadOnlyDictionary<string, IReadOnlyList<PaletteColorValue>> groupPalettes)
        {
            OutputColors = outputColors;
            GroupPalettes = groupPalettes;
        }

        public IReadOnlyList<PaletteColorValue> OutputColors { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<PaletteColorValue>> GroupPalettes { get; }
    }
}
