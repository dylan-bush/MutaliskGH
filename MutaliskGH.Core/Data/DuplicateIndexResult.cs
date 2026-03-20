using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public sealed class DuplicateIndexResult<T>
    {
        public DuplicateIndexResult(
            IReadOnlyList<T> distinctValues,
            IReadOnlyList<int> retainedIndexes,
            IReadOnlyList<int> culledIndexes)
        {
            DistinctValues = distinctValues;
            RetainedIndexes = retainedIndexes;
            CulledIndexes = culledIndexes;
        }

        public IReadOnlyList<T> DistinctValues { get; }

        public IReadOnlyList<int> RetainedIndexes { get; }

        public IReadOnlyList<int> CulledIndexes { get; }
    }
}
