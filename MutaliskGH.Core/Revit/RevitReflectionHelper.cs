using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MutaliskGH.Core.Revit
{
    internal static class RevitReflectionHelper
    {
        public static object GetPropertyValue(object target, string propertyName)
        {
            if (target == null)
            {
                return null;
            }

            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            return property == null ? null : property.GetValue(target, null);
        }

        public static object InvokeMethod(object target, string methodName, params object[] arguments)
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo method = null;
            foreach (MethodInfo candidate in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (candidate.Name == methodName && candidate.GetParameters().Length == arguments.Length)
                {
                    method = candidate;
                    break;
                }
            }

            return method == null ? null : method.Invoke(target, arguments);
        }

        public static IReadOnlyList<object> ToObjectList(object value)
        {
            var results = new List<object>();
            if (value == null)
            {
                return results;
            }

            IEnumerable enumerable = value as IEnumerable;
            if (enumerable == null || value is string)
            {
                results.Add(value);
                return results;
            }

            foreach (object item in enumerable)
            {
                results.Add(item);
            }

            return results;
        }

        public static string GetCategoryName(object element)
        {
            object category = GetPropertyValue(element, "Category");
            object categoryName = GetPropertyValue(category, "Name");
            return categoryName == null ? null : categoryName.ToString();
        }

        public static bool IsElementIdValid(object elementId)
        {
            if (elementId == null)
            {
                return false;
            }

            object invalid = elementId.GetType().GetField("InvalidElementId", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
            if (invalid != null && invalid.Equals(elementId))
            {
                return false;
            }

            int? integerValue = GetElementIdInteger(elementId);
            return !integerValue.HasValue || integerValue.Value >= 0;
        }

        public static int? GetElementIdInteger(object elementId)
        {
            if (elementId == null)
            {
                return null;
            }

            object integerValue = GetPropertyValue(elementId, "IntegerValue") ?? GetPropertyValue(elementId, "Value");
            if (integerValue is int)
            {
                return (int)integerValue;
            }

            int parsed;
            return int.TryParse(elementId.ToString(), out parsed) ? parsed : (int?)null;
        }

        public static object GetDocumentElement(object document, object elementId)
        {
            return document == null || elementId == null ? null : InvokeMethod(document, "GetElement", elementId);
        }

        public static bool TypeNameEquals(object value, string name)
        {
            return value != null && string.Equals(value.GetType().Name, name, StringComparison.Ordinal);
        }
    }
}
