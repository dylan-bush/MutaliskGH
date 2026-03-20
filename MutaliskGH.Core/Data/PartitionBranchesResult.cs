using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public sealed class PartitionBranchesResult<T>
    {
        public PartitionBranchesResult(IReadOnlyList<PartitionBranchesGroup<T>> groups)
        {
            Groups = groups;
        }

        public IReadOnlyList<PartitionBranchesGroup<T>> Groups { get; }
    }
}
