using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MutaliskGH.Components.Revit
{
    public sealed class MatchFilterElementsComponent : BaseComponent
    {
        public MatchFilterElementsComponent()
            : base(
                "Match Filter Elements",
                "MatchFilter",
                "Query Revit elements from a filter, read a match-key value from both Revit and referenced Rhino geometry, and return both sides in identical matched order.",
                CategoryNames.RevitQuery)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("1b3c1f2f-75b1-4f6b-a7b3-d345087cb31d"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Filter",
                "Filter",
                "Revit ElementFilter used to query the active document.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "Limit",
                "Limit",
                "Maximum number of filtered Revit elements to inspect. Use 0 or less for no limit.",
                GH_ParamAccess.item,
                2000);

            parameterManager.AddGenericParameter(
                "Geometry",
                "G",
                "Referenced Rhino geometry to match against the Revit results.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Match Key",
                "MatchKey",
                "Revit parameter name to read. Rhino references use a matching user string first, then fall back to object name.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
            parameterManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Matched Elements",
                "mE",
                "Filtered Revit elements reordered to match the Rhino side.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Matched Revit Values",
                "mRV",
                "Match-key values read from the matched Revit elements.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Matched Geometry",
                "mG",
                "Referenced Rhino geometry reordered to match the Revit side.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Matched Rhino Values",
                "mRhV",
                "Match-key values read from the Rhino references.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawFilter = null;
            if (!dataAccess.GetData(0, ref rawFilter))
            {
                return;
            }

            int limit = 2000;
            dataAccess.GetData(1, ref limit);

            var geometry = new List<IGH_Goo>();
            dataAccess.GetDataList(2, geometry);

            string matchKey = null;
            dataAccess.GetData(3, ref matchKey);
            if (string.IsNullOrWhiteSpace(matchKey))
            {
                return;
            }

            object filter = ResolveRuntimeValue(rawFilter);
            Result<IReadOnlyList<object>> queryResult = QueryFilteredElements(filter, limit);
            if (ReportFailure(queryResult))
            {
                return;
            }

            var revitValues = new List<string>(queryResult.Value.Count);
            for (int index = 0; index < queryResult.Value.Count; index++)
            {
                revitValues.Add(ReadRevitMatchValue(queryResult.Value[index], matchKey));
            }

            var rhinoValues = new List<string>(geometry.Count);
            for (int index = 0; index < geometry.Count; index++)
            {
                rhinoValues.Add(ReadRhinoMatchValue(geometry[index], matchKey));
            }

            Result<RevitMatchFilterElementsResult> matchResult =
                RevitMatchFilterElementsLogic.MatchByValue(queryResult.Value, revitValues, rhinoValues);
            if (ReportFailure(matchResult))
            {
                return;
            }

            var matchedGeometry = new List<object>(matchResult.Value.MatchedRhinoIndices.Count);
            for (int index = 0; index < matchResult.Value.MatchedRhinoIndices.Count; index++)
            {
                int rhinoIndex = matchResult.Value.MatchedRhinoIndices[index];
                if (rhinoIndex >= 0 && rhinoIndex < geometry.Count)
                {
                    matchedGeometry.Add(geometry[rhinoIndex]);
                }
            }

            dataAccess.SetDataList(0, RevitGrasshopperWrapper.WrapElements(matchResult.Value.MatchedElements));
            dataAccess.SetDataList(1, matchResult.Value.MatchedRevitValues);
            dataAccess.SetDataList(2, matchedGeometry);
            dataAccess.SetDataList(3, matchResult.Value.MatchedRhinoValues);
        }

        private static object ResolveRuntimeValue(IGH_Goo goo)
        {
            object value = GrasshopperValueHelper.Unwrap(goo);
            if (value == null)
            {
                return null;
            }

            PropertyInfo valueProperty = value.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueProperty != null)
            {
                try
                {
                    object propertyValue = valueProperty.GetValue(value, null);
                    if (propertyValue != null)
                    {
                        return propertyValue;
                    }
                }
                catch
                {
                }
            }

            return value;
        }

        private static Result<IReadOnlyList<object>> QueryFilteredElements(object filter, int limit)
        {
            if (filter == null)
            {
                return Result<IReadOnlyList<object>>.Failure("A valid Revit filter is required.");
            }

            object document = RevitReflectionHelper.GetStaticPropertyValue(
                "RhinoInside.Revit",
                "RhinoInside.Revit.Revit",
                "ActiveDBDocument");
            if (document == null)
            {
                return Result<IReadOnlyList<object>>.Failure("Rhino.Inside Revit ActiveDBDocument is unavailable.");
            }

            Assembly revitApiAssembly = filter.GetType().Assembly;
            Type collectorType = revitApiAssembly.GetType("Autodesk.Revit.DB.FilteredElementCollector", false, false);
            if (collectorType == null)
            {
                return Result<IReadOnlyList<object>>.Failure("FilteredElementCollector type could not be found.");
            }

            object collector = Activator.CreateInstance(collectorType, document);
            if (collector == null)
            {
                return Result<IReadOnlyList<object>>.Failure("FilteredElementCollector could not be created.");
            }

            collector = RevitReflectionHelper.InvokeMethod(collector, "WherePasses", filter) ?? collector;
            object elements = RevitReflectionHelper.InvokeMethod(collector, "ToElements");
            var queriedElements = new List<object>();

            IEnumerable enumerable = elements as IEnumerable;
            if (enumerable != null)
            {
                foreach (object element in enumerable)
                {
                    queriedElements.Add(element);
                    if (limit > 0 && queriedElements.Count >= limit)
                    {
                        break;
                    }
                }
            }

            return Result<IReadOnlyList<object>>.Success(queriedElements);
        }

        private static string ReadRevitMatchValue(object element, string matchKey)
        {
            if (element == null || string.IsNullOrWhiteSpace(matchKey))
            {
                return null;
            }

            object parameter = RevitReflectionHelper.InvokeMethod(element, "LookupParameter", matchKey);
            if (parameter == null)
            {
                foreach (object candidate in RevitReflectionHelper.ToObjectList(RevitReflectionHelper.GetPropertyValue(element, "Parameters")))
                {
                    object definition = RevitReflectionHelper.GetPropertyValue(candidate, "Definition");
                    string definitionName = RevitReflectionHelper.GetPropertyValue(definition, "Name") as string;
                    if (string.Equals(definitionName, matchKey, StringComparison.OrdinalIgnoreCase))
                    {
                        parameter = candidate;
                        break;
                    }
                }
            }

            if (parameter == null)
            {
                return null;
            }

            object valueString = RevitReflectionHelper.InvokeMethod(parameter, "AsValueString");
            if (valueString != null && !string.IsNullOrWhiteSpace(valueString.ToString()))
            {
                return valueString.ToString();
            }

            object stringValue = RevitReflectionHelper.InvokeMethod(parameter, "AsString");
            if (stringValue != null && !string.IsNullOrWhiteSpace(stringValue.ToString()))
            {
                return stringValue.ToString();
            }

            object integerValue = RevitReflectionHelper.InvokeMethod(parameter, "AsInteger");
            if (integerValue != null && integerValue.GetType() == typeof(int))
            {
                return integerValue.ToString();
            }

            object doubleValue = RevitReflectionHelper.InvokeMethod(parameter, "AsDouble");
            if (doubleValue != null && doubleValue.GetType() == typeof(double))
            {
                return doubleValue.ToString();
            }

            object elementId = RevitReflectionHelper.InvokeMethod(parameter, "AsElementId");
            int? id = RevitReflectionHelper.GetElementIdInteger(elementId);
            return id.HasValue ? id.Value.ToString() : null;
        }

        private static string ReadRhinoMatchValue(IGH_Goo goo, string matchKey)
        {
            if (goo == null)
            {
                return null;
            }

            var geometricGoo = goo as IGH_GeometricGoo;
            if (geometricGoo != null && geometricGoo.IsReferencedGeometry && geometricGoo.ReferenceID != Guid.Empty)
            {
                var doc = global::Rhino.RhinoDoc.ActiveDoc;
                var rhinoObject = doc == null ? null : doc.Objects.FindId(geometricGoo.ReferenceID);
                if (rhinoObject != null)
                {
                    string userValue = rhinoObject.Attributes.GetUserString(matchKey);
                    if (!string.IsNullOrWhiteSpace(userValue))
                    {
                        return userValue;
                    }

                    if (!string.IsNullOrWhiteSpace(rhinoObject.Attributes.Name))
                    {
                        return rhinoObject.Attributes.Name;
                    }
                }
            }

            object value = GrasshopperValueHelper.Unwrap(goo);
            return value == null ? null : value.ToString();
        }
    }
}
