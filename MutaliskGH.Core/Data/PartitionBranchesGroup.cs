using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public sealed class PartitionBranchesGroup<T>
    {
        public PartitionBranchesGroup(
            int branchIndex,
            bool shouldPartition,
            IReadOnlyList<IReadOnlyList<T>> segments)
        {
            BranchIndex = branchIndex;
            ShouldPartition = shouldPartition;
            Segments = segments;
        }

        public int BranchIndex { get; }

        public bool ShouldPartition { get; }

        public IReadOnlyList<IReadOnlyList<T>> Segments { get; }
    }
}
