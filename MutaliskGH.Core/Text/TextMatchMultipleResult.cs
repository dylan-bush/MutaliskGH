using System.Collections.Generic;

namespace MutaliskGH.Core.Text
{
    public sealed class TextMatchMultipleResult
    {
        public TextMatchMultipleResult(
            IReadOnlyList<bool> matches,
            IReadOnlyList<int> matchingIndexes,
            IReadOnlyList<int> nonMatchingIndexes)
        {
            Matches = matches;
            MatchingIndexes = matchingIndexes;
            NonMatchingIndexes = nonMatchingIndexes;
        }

        public IReadOnlyList<bool> Matches { get; }

        public int MatchCount
        {
            get { return MatchingIndexes.Count; }
        }

        public IReadOnlyList<int> MatchingIndexes { get; }

        public IReadOnlyList<int> NonMatchingIndexes { get; }
    }
}
