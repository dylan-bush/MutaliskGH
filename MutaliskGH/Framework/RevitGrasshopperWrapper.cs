using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MutaliskGH.Framework
{
    internal static class RevitGrasshopperWrapper
    {
        private const string RhinoInsideRevitGhAssemblyName = "RhinoInside.Revit.GH";
        private const string RhinoInsideRevitElementTypeName = "RhinoInside.Revit.GH.Types.Element";

        private static readonly object SyncRoot = new object();
        private static Assembly _assembly;
        private static Type _elementWrapperType;
        private static MethodInfo _fromValueMethod;
        private static bool _initialized;

        public static object WrapElement(object value)
        {
            if (value == null)
            {
                return null;
            }

            EnsureInitialized();
            if (_fromValueMethod != null)
            {
                try
                {
                    object wrapped = _fromValueMethod.Invoke(null, new[] { value });
                    if (wrapped != null)
                    {
                        return wrapped;
                    }
                }
                catch
                {
                    // Fall back to a generic wrapper when RiR goo creation fails.
                }
            }

            return new GH_ObjectWrapper(value);
        }

        public static List<object> WrapElements(IReadOnlyList<object> values)
        {
            var wrappedValues = new List<object>(values.Count);
            for (int index = 0; index < values.Count; index++)
            {
                object wrapped = WrapElement(values[index]);
                if (wrapped != null)
                {
                    wrappedValues.Add(wrapped);
                }
            }

            return wrappedValues;
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            lock (SyncRoot)
            {
                if (_initialized)
                {
                    return;
                }

                _assembly = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(assembly =>
                        string.Equals(assembly.GetName().Name, RhinoInsideRevitGhAssemblyName, StringComparison.Ordinal));

                if (_assembly == null)
                {
                    try
                    {
                        _assembly = Assembly.Load(RhinoInsideRevitGhAssemblyName);
                    }
                    catch
                    {
                        _assembly = null;
                    }
                }

                if (_assembly != null)
                {
                    _elementWrapperType = _assembly.GetType(RhinoInsideRevitElementTypeName, false, false);
                    _fromValueMethod = _elementWrapperType?.GetMethod(
                        "FromValue",
                        BindingFlags.Public | BindingFlags.Static,
                        binder: null,
                        types: new[] { typeof(object) },
                        modifiers: null);
                }

                _initialized = true;
            }
        }
    }
}
