using System.Collections.Generic;

namespace MutaliskGH.Core.Revit
{
    public sealed class RevitMatchFilterElementsResult
    {
        public RevitMatchFilterElementsResult(
            IReadOnlyList<object> matchedElements,
            IReadOnlyList<string> matchedRevitValues,
            IReadOnlyList<int> matchedRevitIndices,
            IReadOnlyList<string> matchedRhinoValues,
            IReadOnlyList<int> matchedRhinoIndices)
        {
            MatchedElements = matchedElements;
            MatchedRevitValues = matchedRevitValues;
            MatchedRevitIndices = matchedRevitIndices;
            MatchedRhinoValues = matchedRhinoValues;
            MatchedRhinoIndices = matchedRhinoIndices;
        }

        public IReadOnlyList<object> MatchedElements { get; }

        public IReadOnlyList<string> MatchedRevitValues { get; }

        public IReadOnlyList<int> MatchedRevitIndices { get; }

        public IReadOnlyList<string> MatchedRhinoValues { get; }

        public IReadOnlyList<int> MatchedRhinoIndices { get; }
    }
}
