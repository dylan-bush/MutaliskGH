using System.Collections.Generic;

namespace MutaliskGH.Core.Text
{
    public sealed class MultipleRegexIndexResult
    {
        public MultipleRegexIndexResult(int? firstMatchIndex, IReadOnlyList<int> allMatchIndexes)
        {
            FirstMatchIndex = firstMatchIndex;
            AllMatchIndexes = allMatchIndexes;
        }

        public int? FirstMatchIndex { get; }

        public IReadOnlyList<int> AllMatchIndexes { get; }
    }
}
