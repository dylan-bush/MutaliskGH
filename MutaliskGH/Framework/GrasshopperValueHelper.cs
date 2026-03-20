using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace MutaliskGH.Framework
{
    internal static class GrasshopperValueHelper
    {
        public static object Unwrap(IGH_Goo goo)
        {
            if (goo == null)
            {
                return null;
            }

            object value = goo.ScriptVariable();
            IGH_Goo nestedGoo = value as IGH_Goo;
            if (nestedGoo != null)
            {
                return Unwrap(nestedGoo);
            }

            return value;
        }

        public static List<object> UnwrapAll(IReadOnlyList<IGH_Goo> values)
        {
            List<object> unwrappedValues = new List<object>(values.Count);
            for (int index = 0; index < values.Count; index++)
            {
                unwrappedValues.Add(Unwrap(values[index]));
            }

            return unwrappedValues;
        }

    }
}
