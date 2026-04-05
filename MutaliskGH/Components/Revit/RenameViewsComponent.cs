using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MutaliskGH.Components.Revit
{
    public sealed class RenameViewsComponent : BaseComponent
    {
        public RenameViewsComponent()
            : base(
                "Rename Views",
                "RenameViews",
                "Rename Revit views. Uses a Run trigger to avoid renaming on every Grasshopper solve.",
                CategoryNames.RevitAutomation)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("58c0d88a-af5d-4b67-8c66-fab3f94291f5"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter("Views", "V", "Revit views to rename.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("New Names", "N", "One name for all views or one name per view.", GH_ParamAccess.list);
            parameterManager.AddBooleanParameter("Run", "Run", "Set to true to execute the rename transaction.", GH_ParamAccess.item, false);

            for (int index = 0; index < Params.Input.Count; index++)
            {
                Params.Input[index].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("Output", "out", "Execution summary or error information.", GH_ParamAccess.item);
            parameterManager.AddGenericParameter("Renamed Views", "a", "Renamed Revit views.", GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            var rawViews = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawViews) || rawViews.Count == 0)
            {
                return;
            }

            var names = new List<string>();
            dataAccess.GetDataList(1, names);

            bool run = false;
            dataAccess.GetData(2, ref run);
            if (!run)
            {
                dataAccess.SetData(0, "Ready. Set Run to true to rename the supplied views.");
                return;
            }

            List<object> views = GrasshopperValueHelper.UnwrapAll(rawViews);
            Result<IReadOnlyList<(object View, string Name)>> pairResult = RevitRenameViewsLogic.PairViewsAndNames(views, names);
            if (ReportFailure(pairResult))
            {
                dataAccess.SetData(0, pairResult.ErrorMessage);
                return;
            }

            object document = RevitReflectionHelper.GetPropertyValue(pairResult.Value[0].View, "Document")
                ?? RevitReflectionHelper.GetStaticPropertyValue("RhinoInside.Revit", "RhinoInside.Revit.Revit", "ActiveDBDocument");
            if (document == null)
            {
                const string message = "No active Revit document is available.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(0, message);
                return;
            }

            object transaction = CreateTransaction(document, "Rename Views");
            if (transaction == null)
            {
                const string message = "Revit Transaction could not be created.";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(0, message);
                return;
            }

            int successCount = 0;
            var renamedViews = new List<object>();
            var errors = new List<string>();

            try
            {
                RevitReflectionHelper.InvokeMethod(transaction, "Start");

                for (int index = 0; index < pairResult.Value.Count; index++)
                {
                    (object view, string name) = pairResult.Value[index];
                    try
                    {
                        SetName(view, name);
                        successCount++;
                        renamedViews.Add(view);
                    }
                    catch (Exception exception)
                    {
                        errors.Add("[" + index + "] " + exception.Message);
                    }
                }

                RevitReflectionHelper.InvokeMethod(transaction, "Commit");
            }
            catch (TargetInvocationException exception)
            {
                RevitReflectionHelper.InvokeMethod(transaction, "RollBack");
                string message = exception.InnerException?.Message ?? exception.Message;
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(0, message);
                return;
            }
            catch (Exception exception)
            {
                RevitReflectionHelper.InvokeMethod(transaction, "RollBack");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, exception.Message);
                dataAccess.SetData(0, exception.Message);
                return;
            }

            string summary = "Renamed " + successCount + " view(s).";
            if (errors.Count > 0)
            {
                summary += " Errors: " + string.Join(" | ", errors);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, summary);
            }

            dataAccess.SetData(0, summary);
            dataAccess.SetDataList(1, RevitGrasshopperWrapper.WrapElements(renamedViews));
        }

        private static object CreateTransaction(object document, string name)
        {
            Assembly assembly = document.GetType().Assembly;
            Type transactionType = assembly.GetType("Autodesk.Revit.DB.Transaction", false, false);
            return transactionType == null ? null : Activator.CreateInstance(transactionType, document, name);
        }

        private static void SetName(object view, string name)
        {
            PropertyInfo nameProperty = view.GetType().GetProperty("Name", BindingFlags.Instance | BindingFlags.Public);
            if (nameProperty == null || !nameProperty.CanWrite)
            {
                throw new InvalidOperationException("Input object does not expose a writable Revit view Name property.");
            }

            nameProperty.SetValue(view, name, null);
        }
    }
}
