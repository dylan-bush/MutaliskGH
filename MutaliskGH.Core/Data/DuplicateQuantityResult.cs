using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public sealed class DuplicateQuantityResult<T>
    {
        public DuplicateQuantityResult(
            IReadOnlyList<T> distinctValues,
            IReadOnlyList<int> quantities)
        {
            DistinctValues = distinctValues;
            Quantities = quantities;
        }

        public IReadOnlyList<T> DistinctValues { get; }

        public IReadOnlyList<int> Quantities { get; }
    }
}
