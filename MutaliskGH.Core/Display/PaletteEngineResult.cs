using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public sealed class PaletteEngineResult
    {
        public PaletteEngineResult(
            IReadOnlyList<PaletteColorValue> outputColors,
            IReadOnlyList<string> groupLabels,
            IReadOnlyDictionary<string, IReadOnlyList<PaletteColorValue>> groupPalettes)
        {
            OutputColors = outputColors;
            GroupLabels = groupLabels;
            GroupPalettes = groupPalettes;
        }

        public IReadOnlyList<PaletteColorValue> OutputColors { get; }

        public IReadOnlyList<string> GroupLabels { get; }

        public IReadOnlyDictionary<string, IReadOnlyList<PaletteColorValue>> GroupPalettes { get; }
    }
}
