using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Revit
{
    public static class RevitRenameViewsLogic
    {
        public static Result<IReadOnlyList<(object View, string Name)>> PairViewsAndNames(
            IReadOnlyList<object> views,
            IReadOnlyList<string> names)
        {
            if (views == null || views.Count == 0)
            {
                return Result<IReadOnlyList<(object View, string Name)>>.Failure("At least one Revit view is required.");
            }

            if (names == null || names.Count == 0)
            {
                return Result<IReadOnlyList<(object View, string Name)>>.Failure("At least one new view name is required.");
            }

            if (names.Count != 1 && names.Count != views.Count)
            {
                return Result<IReadOnlyList<(object View, string Name)>>.Failure("Provide either one name for all views or one name per view.");
            }

            var pairs = new List<(object View, string Name)>(views.Count);
            for (int index = 0; index < views.Count; index++)
            {
                object view = views[index];
                if (view == null)
                {
                    continue;
                }

                string name = names[Math.Min(index, names.Count - 1)] ?? string.Empty;
                name = name.Trim();
                if (name.Length == 0)
                {
                    return Result<IReadOnlyList<(object View, string Name)>>.Failure("New view names cannot be blank.");
                }

                pairs.Add((view, name));
            }

            if (pairs.Count == 0)
            {
                return Result<IReadOnlyList<(object View, string Name)>>.Failure("No valid Revit views were supplied.");
            }

            return Result<IReadOnlyList<(object View, string Name)>>.Success(pairs);
        }
    }
}
