using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Revit
{
    public sealed class SpotElevationReferenceComponent : BaseComponent
    {
        public SpotElevationReferenceComponent()
            : base(
                "Spot Elevation Reference",
                "SpotRef",
                "Extract the Revit element measured by a spot elevation and resolve its adaptive parent when needed.",
                CategoryNames.RevitQuery)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("522ba19e-d73f-431d-b9b0-f36981af7586"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Spot Dimension",
                "S",
                "Revit SpotDimension element.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Reference",
                "R",
                "Original referenced element.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Reference Parent",
                "P",
                "Reference parent element. Adaptive points resolve to the owning adaptive component when possible.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "Parent Id",
                "Id",
                "ElementId integer value of the reference parent.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Category",
                "C",
                "Category name of the original referenced element.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawSpotDimension = null;
            if (!dataAccess.GetData(0, ref rawSpotDimension))
            {
                return;
            }

            object spotDimension = GrasshopperValueHelper.Unwrap(rawSpotDimension);
            Result<RevitSpotElevationReferenceResult> result = RevitSpotElevationReferenceLogic.GetReferenceData(spotDimension);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetData(0, Wrap(result.Value.Reference));
            dataAccess.SetData(1, Wrap(result.Value.ReferenceParent));
            if (result.Value.ParentElementId.HasValue)
            {
                dataAccess.SetData(2, result.Value.ParentElementId.Value);
            }

            dataAccess.SetData(3, result.Value.CategoryName);
        }

        private static GH_ObjectWrapper Wrap(object value)
        {
            return value == null ? null : new GH_ObjectWrapper(value);
        }
    }
}
