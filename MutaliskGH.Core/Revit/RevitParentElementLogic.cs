using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Revit
{
    public static class RevitParentElementLogic
    {
        public static Result<RevitParentElementResult> GetParentData(object element)
        {
            if (element == null)
            {
                return Result<RevitParentElementResult>.Failure("Input element is null.");
            }

            object document = RevitReflectionHelper.GetPropertyValue(element, "Document");
            if (document == null)
            {
                return Result<RevitParentElementResult>.Failure("Input must be a valid Revit element.");
            }

            var hierarchy = new List<object>();
            var relationshipTypes = new List<string>();
            object parent = FindImmediateParent(element, document, hierarchy, relationshipTypes);

            object parentType = null;
            object parentFamily = null;
            if (parent != null)
            {
                parentType = GetElementType(parent, document);
                parentFamily = GetFamily(parent);
                AppendAncestorHierarchy(parent, document, hierarchy, relationshipTypes);
            }

            return Result<RevitParentElementResult>.Success(
                new RevitParentElementResult(parent, parentType, parentFamily, hierarchy, relationshipTypes));
        }

        public static object ResolveAdaptivePointParent(object element)
        {
            if (element == null)
            {
                return null;
            }

            string categoryName = RevitReflectionHelper.GetCategoryName(element);
            if (!string.Equals(categoryName, "Adaptive Points", StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }

            object document = RevitReflectionHelper.GetPropertyValue(element, "Document");
            if (document == null)
            {
                return element;
            }

            object superComponent = RevitReflectionHelper.GetPropertyValue(element, "SuperComponent");
            if (superComponent != null)
            {
                return superComponent;
            }

            foreach (object dependent in GetDependentElements(element, document))
            {
                if (RevitReflectionHelper.TypeNameEquals(dependent, "FamilyInstance"))
                {
                    return dependent;
                }
            }

            object parameters = RevitReflectionHelper.GetPropertyValue(element, "Parameters");
            foreach (object parameter in RevitReflectionHelper.ToObjectList(parameters))
            {
                object storageType = RevitReflectionHelper.GetPropertyValue(parameter, "StorageType");
                if (storageType == null || !string.Equals(storageType.ToString(), "ElementId", StringComparison.Ordinal))
                {
                    continue;
                }

                object ownerId = RevitReflectionHelper.InvokeMethod(parameter, "AsElementId");
                if (!RevitReflectionHelper.IsElementIdValid(ownerId))
                {
                    continue;
                }

                object owner = RevitReflectionHelper.GetDocumentElement(document, ownerId);
                if (RevitReflectionHelper.TypeNameEquals(owner, "FamilyInstance"))
                {
                    return owner;
                }
            }

            return element;
        }

        private static object FindImmediateParent(object element, object document, List<object> hierarchy, List<string> relationshipTypes)
        {
            object parent = RevitReflectionHelper.GetPropertyValue(element, "SuperComponent");
            if (parent != null)
            {
                hierarchy.Add(parent);
                relationshipTypes.Add("SuperComponent");
                return parent;
            }

            parent = GetLinkedElement(document, RevitReflectionHelper.GetPropertyValue(element, "GroupId"));
            if (parent != null)
            {
                hierarchy.Add(parent);
                relationshipTypes.Add("Group");
                return parent;
            }

            parent = GetLinkedElement(document, RevitReflectionHelper.GetPropertyValue(element, "AssemblyInstanceId"));
            if (parent != null)
            {
                hierarchy.Add(parent);
                relationshipTypes.Add("Assembly");
                return parent;
            }

            parent = RevitReflectionHelper.GetPropertyValue(element, "Host");
            if (parent != null)
            {
                hierarchy.Add(parent);
                relationshipTypes.Add("Host");
                return parent;
            }

            foreach (var candidate in FindDependents(document, element))
            {
                hierarchy.Add(candidate.Parent);
                relationshipTypes.Add(candidate.Relationship);
                return candidate.Parent;
            }

            return null;
        }

        private static void AppendAncestorHierarchy(object current, object document, List<object> hierarchy, List<string> relationshipTypes)
        {
            object pointer = current;
            while (pointer != null)
            {
                object next = RevitReflectionHelper.GetPropertyValue(pointer, "SuperComponent");
                string relationship = "SuperComponent";

                if (next == null)
                {
                    next = GetLinkedElement(document, RevitReflectionHelper.GetPropertyValue(pointer, "GroupId"));
                    relationship = "Group";
                }

                if (next == null)
                {
                    next = GetLinkedElement(document, RevitReflectionHelper.GetPropertyValue(pointer, "AssemblyInstanceId"));
                    relationship = "Assembly";
                }

                if (next == null)
                {
                    next = RevitReflectionHelper.GetPropertyValue(pointer, "Host");
                    relationship = "Host";
                }

                if (next == null || hierarchy.Contains(next))
                {
                    break;
                }

                hierarchy.Add(next);
                relationshipTypes.Add(relationship);
                pointer = next;
            }
        }

        private static object GetElementType(object element, object document)
        {
            object typeId = RevitReflectionHelper.InvokeMethod(element, "GetTypeId");
            return GetLinkedElement(document, typeId);
        }

        private static object GetFamily(object element)
        {
            object symbol = RevitReflectionHelper.GetPropertyValue(element, "Symbol");
            return RevitReflectionHelper.GetPropertyValue(symbol, "Family");
        }

        private static object GetLinkedElement(object document, object elementId)
        {
            return RevitReflectionHelper.IsElementIdValid(elementId)
                ? RevitReflectionHelper.GetDocumentElement(document, elementId)
                : null;
        }

        private static IEnumerable<(object Parent, string Relationship)> FindDependents(object document, object target)
        {
            object targetId = RevitReflectionHelper.GetPropertyValue(target, "Id");
            if (!RevitReflectionHelper.IsElementIdValid(targetId))
            {
                yield break;
            }

            object allElements = RevitReflectionHelper.InvokeMethod(document, "GetElements");
            foreach (object candidate in RevitReflectionHelper.ToObjectList(allElements))
            {
                foreach (object dependent in GetDependentElements(candidate, document))
                {
                    if (dependent != null && Equals(RevitReflectionHelper.GetPropertyValue(dependent, "Id"), targetId))
                    {
                        string relationship = RevitReflectionHelper.TypeNameEquals(target, "FamilyInstance")
                            ? "Nested Family"
                            : "Dependent";
                        yield return (candidate, relationship);
                        yield break;
                    }
                }
            }
        }

        private static IReadOnlyList<object> GetDependentElements(object element, object document)
        {
            object dependentIds = RevitReflectionHelper.InvokeMethod(element, "GetDependentElements", new object[] { null });
            var resolved = new List<object>();
            foreach (object dependentId in RevitReflectionHelper.ToObjectList(dependentIds))
            {
                object dependent = RevitReflectionHelper.GetDocumentElement(document, dependentId);
                if (dependent != null)
                {
                    resolved.Add(dependent);
                }
            }

            return resolved;
        }
    }
}
