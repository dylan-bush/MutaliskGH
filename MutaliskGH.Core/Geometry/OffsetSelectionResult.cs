using System.Collections.Generic;

namespace MutaliskGH.Core.Geometry
{
    public sealed class OffsetSelectionResult
    {
        public OffsetSelectionResult(IReadOnlyList<int> orderedSides)
        {
            OrderedSides = orderedSides;
        }

        public IReadOnlyList<int> OrderedSides { get; }
    }
}
