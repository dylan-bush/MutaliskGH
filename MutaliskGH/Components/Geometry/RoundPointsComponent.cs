using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Geometry;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;

namespace MutaliskGH.Components.Geometry
{
    public sealed class RoundPointsComponent : BaseComponent
    {
        public RoundPointsComponent()
            : base(
                "Round Points",
                "rPts",
                "Round point coordinates to the requested factor while preserving tree structure.",
                CategoryNames.Geometry)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("fb72e53b-e889-4d9d-aa9f-91a5feecdccf"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddPointParameter(
                "Points",
                "Pt",
                "Tree of points to round.",
                GH_ParamAccess.tree);

            parameterManager.AddNumberParameter(
                "Factor",
                "F",
                "Rounding factor applied to each coordinate.",
                GH_ParamAccess.item,
                1.0);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddPointParameter(
                "Points",
                "Pt",
                "Rounded points with the original tree shape preserved.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            if (Params.Input[0].VolatileDataCount == 0)
            {
                return;
            }

            GH_Structure<GH_Point> points;
            if (!dataAccess.GetDataTree(0, out points))
            {
                return;
            }

            double factor = 1.0;
            dataAccess.GetData(1, ref factor);

            GH_Structure<GH_Point> output = new GH_Structure<GH_Point>();
            for (int pathIndex = 0; pathIndex < points.PathCount; pathIndex++)
            {
                GH_Path path = points.Paths[pathIndex];
                output.EnsurePath(path);

                for (int itemIndex = 0; itemIndex < points.Branches[pathIndex].Count; itemIndex++)
                {
                    GH_Point point = points.Branches[pathIndex][itemIndex];
                    if (point == null)
                    {
                        continue;
                    }

                    Result<Point3Value> result = PointRoundingLogic.Round(
                        new Point3Value(point.Value.X, point.Value.Y, point.Value.Z),
                        factor);

                    if (ReportFailure(result))
                    {
                        return;
                    }

                    output.Append(
                        new GH_Point(new Point3d(result.Value.X, result.Value.Y, result.Value.Z)),
                        path);
                }
            }

            dataAccess.SetDataTree(0, output);
        }
    }
}
