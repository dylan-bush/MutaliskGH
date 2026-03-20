using System.Collections.Generic;

namespace MutaliskGH.Core.Text
{
    public sealed class RegexCullResult<T>
    {
        public RegexCullResult(
            IReadOnlyList<T> filteredItems,
            IReadOnlyList<bool> matchFlags,
            string pattern,
            IReadOnlyList<T> culledItems)
        {
            FilteredItems = filteredItems;
            MatchFlags = matchFlags;
            Pattern = pattern;
            CulledItems = culledItems;
        }

        public IReadOnlyList<T> FilteredItems { get; }

        public IReadOnlyList<bool> MatchFlags { get; }

        public string Pattern { get; }

        public IReadOnlyList<T> CulledItems { get; }
    }
}
