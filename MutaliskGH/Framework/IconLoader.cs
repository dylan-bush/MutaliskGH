using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MutaliskGH.Framework
{
    internal static class IconLoader
    {
        private static readonly Dictionary<string, Bitmap> Cache = new Dictionary<string, Bitmap>(StringComparer.OrdinalIgnoreCase);

        public static Bitmap Load(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                return null;
            }

            Bitmap bitmap;
            if (Cache.TryGetValue(resourceName, out bitmap))
            {
                return bitmap;
            }

            Assembly assembly = typeof(IconLoader).Assembly;
            string manifestName = assembly
                .GetManifestResourceNames()
                .FirstOrDefault(name => name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));

            if (manifestName == null)
            {
                return null;
            }

            using (Stream stream = assembly.GetManifestResourceStream(manifestName))
            {
                if (stream == null)
                {
                    return null;
                }

                bitmap = new Bitmap(stream);
            }

            Cache[resourceName] = bitmap;
            return bitmap;
        }
    }
}
