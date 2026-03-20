using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public sealed class BranchByMemberResult<T>
    {
        public BranchByMemberResult(
            IReadOnlyList<T> distinctKeys,
            IReadOnlyList<IReadOnlyList<bool>> matchPatterns)
        {
            DistinctKeys = distinctKeys;
            MatchPatterns = matchPatterns;
        }

        public IReadOnlyList<T> DistinctKeys { get; }

        public IReadOnlyList<IReadOnlyList<bool>> MatchPatterns { get; }
    }
}
