using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Revit
{
    public static class RevitMatchFilterElementsLogic
    {
        public static Result<RevitMatchFilterElementsResult> MatchByValue(
            IReadOnlyList<object> revitElements,
            IReadOnlyList<string> revitValues,
            IReadOnlyList<string> rhinoValues)
        {
            if (revitElements == null)
            {
                return Result<RevitMatchFilterElementsResult>.Failure("Revit element list is null.");
            }

            if (revitValues == null)
            {
                return Result<RevitMatchFilterElementsResult>.Failure("Revit value list is null.");
            }

            if (rhinoValues == null)
            {
                return Result<RevitMatchFilterElementsResult>.Failure("Rhino value list is null.");
            }

            var lookup = new Dictionary<string, Queue<int>>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < revitValues.Count; index++)
            {
                string normalized = NormalizeValue(revitValues[index]);
                if (string.IsNullOrEmpty(normalized))
                {
                    continue;
                }

                Queue<int> queue;
                if (!lookup.TryGetValue(normalized, out queue))
                {
                    queue = new Queue<int>();
                    lookup.Add(normalized, queue);
                }

                queue.Enqueue(index);
            }

            var matchedElements = new List<object>();
            var matchedRevitValues = new List<string>();
            var matchedRevitIndices = new List<int>();
            var matchedRhinoValues = new List<string>();
            var matchedRhinoIndices = new List<int>();

            for (int rhinoIndex = 0; rhinoIndex < rhinoValues.Count; rhinoIndex++)
            {
                string normalized = NormalizeValue(rhinoValues[rhinoIndex]);
                if (string.IsNullOrEmpty(normalized))
                {
                    continue;
                }

                Queue<int> queue;
                if (!lookup.TryGetValue(normalized, out queue) || queue.Count == 0)
                {
                    continue;
                }

                int revitIndex = queue.Dequeue();
                matchedElements.Add(revitIndex < revitElements.Count ? revitElements[revitIndex] : null);
                matchedRevitValues.Add(revitValues[revitIndex]);
                matchedRevitIndices.Add(revitIndex);
                matchedRhinoValues.Add(rhinoValues[rhinoIndex]);
                matchedRhinoIndices.Add(rhinoIndex);
            }

            return Result<RevitMatchFilterElementsResult>.Success(
                new RevitMatchFilterElementsResult(
                    matchedElements,
                    matchedRevitValues,
                    matchedRevitIndices,
                    matchedRhinoValues,
                    matchedRhinoIndices));
        }

        public static string NormalizeValue(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
