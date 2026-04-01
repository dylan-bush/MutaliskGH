using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Rhino
{
    public static class RhinoMetadataLogic
    {
        public static Result<Guid> TryParseObjectId(object value)
        {
            if (value is Guid guidValue)
            {
                return Result<Guid>.Success(guidValue);
            }

            if (value is string stringValue && Guid.TryParse(stringValue, out Guid parsedGuid))
            {
                return Result<Guid>.Success(parsedGuid);
            }

            return Result<Guid>.Failure("The value could not be converted to a Rhino object GUID.");
        }

        public static IReadOnlyList<string> NormalizeGroupNames(IEnumerable<string> groupNames)
        {
            var names = new List<string>();
            if (groupNames != null)
            {
                foreach (string groupName in groupNames)
                {
                    if (!string.IsNullOrWhiteSpace(groupName))
                    {
                        names.Add(groupName);
                    }
                }
            }

            if (names.Count == 0)
            {
                names.Add("Ungrouped");
            }

            return names;
        }

        public static IReadOnlyList<string> NormalizeLayerPaths(IEnumerable<string> layerPaths)
        {
            var paths = new List<string>();
            if (layerPaths == null)
            {
                return paths;
            }

            foreach (string layerPath in layerPaths)
            {
                if (!string.IsNullOrWhiteSpace(layerPath))
                {
                    paths.Add(layerPath);
                }
            }

            return paths;
        }
    }
}
