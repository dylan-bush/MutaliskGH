using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MutaliskGH.Core.Revit
{
    public static class RevitExportLogic
    {
        public static Result<IReadOnlyList<string>> BuildTargetPaths(
            int itemCount,
            IReadOnlyList<string> fileNames,
            string folderPath,
            string extension)
        {
            if (itemCount <= 0)
            {
                return Result<IReadOnlyList<string>>.Failure("At least one Revit view is required.");
            }

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return Result<IReadOnlyList<string>>.Failure("A target folder path is required.");
            }

            if (fileNames == null || fileNames.Count == 0)
            {
                return Result<IReadOnlyList<string>>.Failure("At least one target file name is required.");
            }

            if (fileNames.Count != 1 && fileNames.Count != itemCount)
            {
                return Result<IReadOnlyList<string>>.Failure("Provide either one file name for all views or one file name per view.");
            }

            string normalizedExtension = NormalizeExtension(extension);
            var outputPaths = new List<string>(itemCount);

            for (int index = 0; index < itemCount; index++)
            {
                string candidate = fileNames[Math.Min(index, fileNames.Count - 1)] ?? string.Empty;
                candidate = candidate.Trim();
                if (candidate.Length == 0)
                {
                    return Result<IReadOnlyList<string>>.Failure("Export file names cannot be blank.");
                }

                if (!candidate.EndsWith(normalizedExtension, StringComparison.OrdinalIgnoreCase))
                {
                    candidate += normalizedExtension;
                }

                outputPaths.Add(Path.Combine(folderPath, candidate));
            }

            return Result<IReadOnlyList<string>>.Success(outputPaths);
        }

        public static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
            {
                return string.Empty;
            }

            return extension.StartsWith(".") ? extension : "." + extension;
        }

        public static Result<IReadOnlyList<RevitExportTarget>> PairViewsAndTargetPaths(
            IReadOnlyList<object> views,
            IReadOnlyList<string> fileNames,
            string folderPath,
            string extension)
        {
            if (views == null || views.Count == 0)
            {
                return Result<IReadOnlyList<RevitExportTarget>>.Failure("At least one Revit view is required.");
            }

            Result<IReadOnlyList<string>> pathResult = BuildTargetPaths(views.Count, fileNames, folderPath, extension);
            if (pathResult.IsFailure)
            {
                return Result<IReadOnlyList<RevitExportTarget>>.Failure(pathResult.ErrorMessage);
            }

            var duplicatePath = pathResult.Value
                .GroupBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicatePath != null)
            {
                return Result<IReadOnlyList<RevitExportTarget>>.Failure(
                    "Export target paths must be unique. Duplicate path: " + duplicatePath.Key);
            }

            var targets = new List<RevitExportTarget>(views.Count);
            for (int index = 0; index < views.Count; index++)
            {
                if (views[index] == null)
                {
                    return Result<IReadOnlyList<RevitExportTarget>>.Failure("Export views cannot contain null items.");
                }

                targets.Add(new RevitExportTarget(views[index], pathResult.Value[index]));
            }

            return Result<IReadOnlyList<RevitExportTarget>>.Success(targets);
        }
    }
}
