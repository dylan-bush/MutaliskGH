using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;

namespace MutaliskGH.Components.Revit
{
    public sealed class ViewRangeBrepComponent : BaseComponent
    {
        public ViewRangeBrepComponent()
            : base(
                "View Range Brep",
                "VRB",
                "Create a Brep representation of a Revit view volume using crop or section-box data, with plan-view top/depth range overrides when available.",
                CategoryNames.RevitQuery)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a2fd7b36-8fa1-4f63-a0a5-28f57c33ca7a"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "View",
                "View",
                "Revit view to evaluate.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "View Name",
                "Name",
                "Name of the input view.",
                GH_ParamAccess.item);

            parameterManager.AddBrepParameter(
                "View Range Brep",
                "VRB",
                "Brep representation of the resolved view volume.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawView = null;
            if (!dataAccess.GetData(0, ref rawView))
            {
                return;
            }

            object view = GrasshopperValueHelper.Unwrap(rawView);
            Result<RevitViewRangeBrepResult> result = RevitViewRangeBrepLogic.GetViewData(view);
            if (ReportFailure(result))
            {
                return;
            }

            Plane plane = new Plane(
                new Point3d(result.Value.Origin.X, result.Value.Origin.Y, result.Value.Origin.Z),
                new Vector3d(result.Value.XAxis.X, result.Value.XAxis.Y, result.Value.XAxis.Z),
                new Vector3d(result.Value.YAxis.X, result.Value.YAxis.Y, result.Value.YAxis.Z));

            Interval x = CreateInterval(result.Value.Min.X, result.Value.Max.X);
            Interval y = CreateInterval(result.Value.Min.Y, result.Value.Max.Y);
            Interval z = CreateInterval(result.Value.Min.Z, result.Value.Max.Z);
            Box box = new Box(plane, x, y, z);
            Brep brep = box.ToBrep();

            dataAccess.SetData(0, result.Value.ViewName);
            dataAccess.SetData(1, brep);
        }

        private static Interval CreateInterval(double a, double b)
        {
            return new Interval(Math.Min(a, b), Math.Max(a, b));
        }
    }
}
