using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Geometry;
using MutaliskGH.Framework;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Geometry
{
    public sealed class RebuildRectangleComponent : BaseComponent
    {
        public RebuildRectangleComponent()
            : base(
                "Rebuild Rectangle",
                "ReRect",
                "Evaluate rectangles against a reference plane and return the source perimeter, ordered vertices, and ordered edge curves.",
                CategoryNames.Geometry)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("582516ee-903b-4236-b18d-59e538fb5171"); }
        }

        protected override string IconResourceName
        {
            get { return "RebuildRectangle.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddRectangleParameter(
                "Rectangle",
                "Rec",
                "Rectangles to rebuild.",
                GH_ParamAccess.list);

            parameterManager.AddPlaneParameter(
                "Plane",
                "Pln",
                "Optional reference planes used to determine vertex and side ordering. When omitted, each source rectangle orientation is used.",
                GH_ParamAccess.list);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddCurveParameter(
                "Curve",
                "Crv",
                "Source rectangle perimeter curves.",
                GH_ParamAccess.list);

            parameterManager.AddPointParameter(
                "Points",
                "Pts",
                "Rectangle vertices in bottom-left, top-left, top-right, bottom-right order relative to the reference plane.",
                GH_ParamAccess.list);

            parameterManager.AddCurveParameter(
                "Bottom",
                "B",
                "Bottom rectangle edges relative to the reference plane.",
                GH_ParamAccess.list);

            parameterManager.AddCurveParameter(
                "Right",
                "R",
                "Right rectangle edges relative to the reference plane.",
                GH_ParamAccess.list);

            parameterManager.AddCurveParameter(
                "Top",
                "T",
                "Top rectangle edges relative to the reference plane.",
                GH_ParamAccess.list);

            parameterManager.AddCurveParameter(
                "Left",
                "L",
                "Left rectangle edges relative to the reference plane.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<Rectangle3d> rectangles = new List<Rectangle3d>();
            if (!dataAccess.GetDataList(0, rectangles) || rectangles.Count == 0)
            {
                return;
            }

            List<Plane> planes = new List<Plane>();
            dataAccess.GetDataList(1, planes);

            double tolerance = GetTolerance();
            List<Curve> perimeters = new List<Curve>();
            List<Point3d> points = new List<Point3d>();
            List<Curve> bottoms = new List<Curve>();
            List<Curve> rights = new List<Curve>();
            List<Curve> tops = new List<Curve>();
            List<Curve> lefts = new List<Curve>();

            for (int index = 0; index < rectangles.Count; index++)
            {
                Rectangle3d rectangle = rectangles[index];
                if (!rectangle.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid rectangles were skipped.");
                    continue;
                }

                Plane targetPlane = new Plane(rectangle.Center, rectangle.Plane.XAxis, rectangle.Plane.YAxis);
                if (planes.Count > 0)
                {
                    Plane inputPlane = planes[Math.Min(index, planes.Count - 1)];
                    if (inputPlane.IsValid)
                    {
                        targetPlane = new Plane(rectangle.Center, inputPlane.XAxis, inputPlane.YAxis);
                    }
                }

                if (!targetPlane.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid planes were skipped.");
                    continue;
                }

                Point3d[] sourceCorners = GetRectangleCorners(rectangle);
                List<Point3Value> localCorners = new List<Point3Value>(sourceCorners.Length);
                foreach (Point3d corner in sourceCorners)
                {
                    if (!targetPlane.ClosestParameter(corner, out double u, out double v))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Rectangles that could not be evaluated on the reference plane were skipped.");
                        localCorners.Clear();
                        break;
                    }

                    localCorners.Add(new Point3Value(u, v, 0.0));
                }

                if (localCorners.Count != 4)
                {
                    continue;
                }

                Result<RectangleRebuildResult> result = RectangleRebuildLogic.Create(localCorners, tolerance);

                if (ReportFailure(result))
                {
                    return;
                }

                Point3d bottomLeft = targetPlane.PointAt(result.Value.Corners[0].X, result.Value.Corners[0].Y);
                Point3d topLeft = targetPlane.PointAt(result.Value.Corners[1].X, result.Value.Corners[1].Y);
                Point3d topRight = targetPlane.PointAt(result.Value.Corners[2].X, result.Value.Corners[2].Y);
                Point3d bottomRight = targetPlane.PointAt(result.Value.Corners[3].X, result.Value.Corners[3].Y);

                Polyline perimeter = new Polyline(new[] { bottomLeft, topLeft, topRight, bottomRight, bottomLeft });
                perimeters.Add(new PolylineCurve(perimeter));

                points.Add(bottomLeft);
                points.Add(topLeft);
                points.Add(topRight);
                points.Add(bottomRight);

                bottoms.Add(new LineCurve(bottomLeft, bottomRight));
                rights.Add(new LineCurve(bottomRight, topRight));
                tops.Add(new LineCurve(topLeft, topRight));
                lefts.Add(new LineCurve(topLeft, bottomLeft));
            }

            dataAccess.SetDataList(0, perimeters);
            dataAccess.SetDataList(1, points);
            dataAccess.SetDataList(2, bottoms);
            dataAccess.SetDataList(3, rights);
            dataAccess.SetDataList(4, tops);
            dataAccess.SetDataList(5, lefts);
        }

        private static double GetTolerance()
        {
            return RhinoDoc.ActiveDoc != null
                ? RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                : RhinoMath.ZeroTolerance;
        }

        private static Point3d[] GetRectangleCorners(Rectangle3d rectangle)
        {
            return new[]
            {
                rectangle.Corner(0),
                rectangle.Corner(1),
                rectangle.Corner(2),
                rectangle.Corner(3)
            };
        }
    }
}
