using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;
using System.Collections;
using System.Reflection;

namespace MutaliskGH.Components.Revit
{
    public sealed class ZoomElementComponent : BaseComponent
    {
        private bool wasTriggered;

        public ZoomElementComponent()
            : base(
                "Zoom Element",
                "ZoomElem",
                "Zoom the active Revit view to the supplied element on a rising-edge Run trigger.",
                CategoryNames.RevitAutomation)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6debc8e1-f7d8-4e0a-b801-aaf96a3ee966"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter("Element", "E", "Revit element to zoom to.", GH_ParamAccess.item);
            parameterManager.AddBooleanParameter("Run", "Run", "Run the zoom action on a rising-edge trigger.", GH_ParamAccess.item, false);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("Status", "out", "Execution summary or error information.", GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawElement = null;
            if (!dataAccess.GetData(0, ref rawElement))
            {
                return;
            }

            bool run = false;
            dataAccess.GetData(1, ref run);
            if (!run)
            {
                dataAccess.SetData(0, "Ready. Set Run to true to zoom to the supplied element.");
                wasTriggered = false;
                return;
            }

            if (wasTriggered)
            {
                dataAccess.SetData(0, "Waiting for Run to reset.");
                return;
            }

            object element = GrasshopperValueHelper.Unwrap(rawElement);
            Result<RevitZoomElementResult> targetResult = RevitZoomElementLogic.GetTargetData(element);
            if (targetResult.IsFailure)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, targetResult.ErrorMessage);
                dataAccess.SetData(0, targetResult.ErrorMessage);
                wasTriggered = run;
                return;
            }

            object activeUiApplication = RevitReflectionHelper.GetStaticPropertyValue(
                "RhinoInside.Revit",
                "RhinoInside.Revit.Revit",
                "ActiveUIApplication");
            object activeUiDocument = RevitReflectionHelper.GetPropertyValue(activeUiApplication, "ActiveUIDocument");
            if (activeUiDocument == null)
            {
                const string message = "No active Revit UI document is available.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(0, message);
                wasTriggered = run;
                return;
            }

            if (!TryShowElement(activeUiDocument, targetResult.Value.ElementId))
            {
                const string message = "Revit could not zoom to the supplied element.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(0, message);
                wasTriggered = run;
                return;
            }

            string elementIdText = targetResult.Value.ElementIdInteger.HasValue
                ? targetResult.Value.ElementIdInteger.Value.ToString()
                : targetResult.Value.ElementId.ToString();
            dataAccess.SetData(0, "OK: Zoomed to element " + elementIdText + " (" + targetResult.Value.CategoryName + ").");
            wasTriggered = run;
        }

        private static bool TryShowElement(object activeUiDocument, object elementId)
        {
            foreach (MethodInfo method in activeUiDocument.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!string.Equals(method.Name, "ShowElements", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    continue;
                }

                try
                {
                    if (parameters[0].ParameterType.IsInstanceOfType(elementId))
                    {
                        method.Invoke(activeUiDocument, new[] { elementId });
                        return true;
                    }

                    object collection = CreateElementIdCollection(parameters[0].ParameterType, elementId);
                    if (collection != null)
                    {
                        method.Invoke(activeUiDocument, new[] { collection });
                        return true;
                    }
                }
                catch
                {
                }
            }

            return false;
        }

        private static object CreateElementIdCollection(Type targetType, object elementId)
        {
            if (targetType == null || elementId == null)
            {
                return null;
            }

            if (targetType.IsArray && targetType.GetElementType().IsInstanceOfType(elementId))
            {
                Array array = Array.CreateInstance(targetType.GetElementType(), 1);
                array.SetValue(elementId, 0);
                return array;
            }

            Type itemType = targetType.IsGenericType ? targetType.GetGenericArguments()[0] : null;
            if (itemType == null || !itemType.IsInstanceOfType(elementId))
            {
                return null;
            }

            Type listType = typeof(System.Collections.Generic.List<>).MakeGenericType(itemType);
            object list = Activator.CreateInstance(listType);
            listType.GetMethod("Add", new[] { itemType })?.Invoke(list, new[] { elementId });
            return targetType.IsAssignableFrom(listType) ? list : null;
        }
    }
}
