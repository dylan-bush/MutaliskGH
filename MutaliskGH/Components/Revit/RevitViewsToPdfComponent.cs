using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Revit
{
    public sealed class RevitViewsToPdfComponent : BaseComponent
    {
        private bool wasTriggered;

        public RevitViewsToPdfComponent()
            : base(
                "Revit Views to PDF",
                "ViewsPDF",
                "Export one or more Revit views or sheets to PDF on a rising-edge Run trigger.",
                CategoryNames.RevitAutomation)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5f062035-8350-4e82-b4c9-43d4a2153ced"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter("Views", "V", "Revit views or sheets to export.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("File Names", "F", "One file name for all views or one file name per view.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("Folder Path", "Dir", "Target export folder.", GH_ParamAccess.item);
            parameterManager.AddBooleanParameter("Run", "Run", "Run the PDF export on a rising-edge trigger.", GH_ParamAccess.item, false);

            for (int index = 0; index < Params.Input.Count; index++)
            {
                Params.Input[index].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("Output", "out", "Execution summary or error information.", GH_ParamAccess.item);
            parameterManager.AddTextParameter("Paths", "P", "Exported PDF paths.", GH_ParamAccess.list);
            parameterManager.AddGenericParameter("Views", "V", "Views successfully exported.", GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            var rawViews = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawViews) || rawViews.Count == 0)
            {
                return;
            }

            var fileNames = new List<string>();
            dataAccess.GetDataList(1, fileNames);

            string folderPath = null;
            dataAccess.GetData(2, ref folderPath);

            bool run = false;
            dataAccess.GetData(3, ref run);
            if (!run)
            {
                dataAccess.SetData(0, "Ready. Set Run to true to export the supplied views to PDF.");
                wasTriggered = false;
                return;
            }

            if (wasTriggered)
            {
                dataAccess.SetData(0, "Waiting for Run to reset.");
                return;
            }

            List<object> views = GrasshopperValueHelper.UnwrapAll(rawViews);
            Result<IReadOnlyList<RevitExportTarget>> targetResult = RevitExportLogic.PairViewsAndTargetPaths(
                views,
                fileNames,
                folderPath,
                ".pdf");
            if (targetResult.IsFailure)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, targetResult.ErrorMessage);
                dataAccess.SetData(0, targetResult.ErrorMessage);
                wasTriggered = run;
                return;
            }

            object document = RevitReflectionHelper.GetPropertyValue(targetResult.Value[0].View, "Document")
                ?? RevitReflectionHelper.GetStaticPropertyValue("RhinoInside.Revit", "RhinoInside.Revit.Revit", "ActiveDBDocument");

            var exportedPaths = new List<string>();
            var exportedViews = new List<object>();
            var errors = new List<string>();

            for (int index = 0; index < targetResult.Value.Count; index++)
            {
                Result<string> exportResult = RevitViewExportOperations.ExportPdf(document, targetResult.Value[index].View, targetResult.Value[index].Path);
                if (exportResult.IsFailure)
                {
                    errors.Add("[" + index + "] " + exportResult.ErrorMessage);
                    continue;
                }

                exportedPaths.Add(exportResult.Value);
                exportedViews.Add(targetResult.Value[index].View);
            }

            string summary = "Exported " + exportedPaths.Count + " of " + targetResult.Value.Count + " view(s) to PDF.";
            if (errors.Count > 0)
            {
                summary += " Errors: " + string.Join(" | ", errors);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, summary);
            }

            dataAccess.SetData(0, summary);
            dataAccess.SetDataList(1, exportedPaths);
            dataAccess.SetDataList(2, RevitGrasshopperWrapper.WrapElements(exportedViews));
            wasTriggered = run;
        }
    }
}
