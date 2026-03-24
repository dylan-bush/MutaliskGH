using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public sealed class PreviewColorByValueResult
    {
        public PreviewColorByValueResult(
            IReadOnlyList<object> filteredGeometry,
            IReadOnlyList<object> filteredValues,
            IReadOnlyList<object> distinctValues,
            IReadOnlyList<IReadOnlyList<bool>> matchPatterns,
            IReadOnlyList<PaletteColorValue> branchColors)
        {
            FilteredGeometry = filteredGeometry;
            FilteredValues = filteredValues;
            DistinctValues = distinctValues;
            MatchPatterns = matchPatterns;
            BranchColors = branchColors;
        }

        public IReadOnlyList<object> FilteredGeometry { get; }

        public IReadOnlyList<object> FilteredValues { get; }

        public IReadOnlyList<object> DistinctValues { get; }

        public IReadOnlyList<IReadOnlyList<bool>> MatchPatterns { get; }

        public IReadOnlyList<PaletteColorValue> BranchColors { get; }
    }
}
