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
    public sealed class OffsetLargerSmallerComponent : BaseComponent
    {
        public OffsetLargerSmallerComponent()
            : base(
                "Offset Select",
                "OffsetSel",
                "Offset curves and return the smaller result, larger result, or both based on the requested mode.",
                CategoryNames.Geometry)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7f99c95e-6bc9-44ae-bda3-e35184553b81"); }
        }

        protected override string IconResourceName
        {
            get { return "OffsetSelect.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "Mode",
                "M",
                "0 returns the smaller offset, 1 returns the larger offset, and 2 returns both.",
                GH_ParamAccess.item,
                1);

            parameterManager.AddCurveParameter(
                "Curve",
                "C",
                "Curves to offset.",
                GH_ParamAccess.list);

            parameterManager.AddNumberParameter(
                "Distance",
                "D",
                "Offset distance.",
                GH_ParamAccess.item,
                1.0);

            parameterManager.AddPlaneParameter(
                "Plane",
                "Pln",
                "Offset plane.",
                GH_ParamAccess.item,
                Plane.WorldXY);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
            parameterManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddCurveParameter(
                "Selected Offset",
                "O",
                "Offset result selected by the requested mode, or both results in ordered output when mode is set to both.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<Curve> curves = new List<Curve>();
            if (!dataAccess.GetDataList(1, curves) || curves.Count == 0)
            {
                return;
            }

            int mode = 1;
            dataAccess.GetData(0, ref mode);

            double distance = 1.0;
            dataAccess.GetData(2, ref distance);

            if (Math.Abs(distance) <= RhinoMath.ZeroTolerance)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "A non-zero offset distance is required.");
                return;
            }

            Plane plane = Plane.WorldXY;
            dataAccess.GetData(3, ref plane);

            double tolerance = GetTolerance();
            List<Curve> weave = new List<Curve>();

            for (int index = 0; index < curves.Count; index++)
            {
                Curve curve = curves[index];
                if (curve == null || !curve.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid curves were skipped.");
                    continue;
                }

                Curve positive = GetBestOffsetCandidate(curve, plane, Math.Abs(distance), tolerance);
                Curve negative = GetBestOffsetCandidate(curve, plane, -Math.Abs(distance), tolerance);

                OffsetCandidateValue positiveCandidate = BuildCandidate(positive, plane);
                OffsetCandidateValue negativeCandidate = BuildCandidate(negative, plane);

                Result<OffsetSelectionResult> result = OffsetSelectionLogic.Select(mode, positiveCandidate, negativeCandidate);
                if (result.IsFailure)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.ErrorMessage);
                    return;
                }

                for (int sideIndex = 0; sideIndex < result.Value.OrderedSides.Count; sideIndex++)
                {
                    int side = result.Value.OrderedSides[sideIndex];
                    Curve selected = side > 0 ? positive : negative;
                    if (selected != null)
                    {
                        weave.Add(selected);
                    }
                }
            }

            dataAccess.SetDataList(0, weave);
        }

        private static OffsetCandidateValue BuildCandidate(Curve curve, Plane plane)
        {
            if (curve == null)
            {
                return new OffsetCandidateValue(false, 0.0, 0.0);
            }

            return new OffsetCandidateValue(true, ComputeMetric(curve, plane), curve.GetLength());
        }

        private static double ComputeMetric(Curve curve, Plane plane)
        {
            if (curve.IsClosed)
            {
                using (AreaMassProperties area = AreaMassProperties.Compute(curve))
                {
                    if (area != null)
                    {
                        return Math.Abs(area.Area);
                    }
                }
            }

            BoundingBox bounds = curve.GetBoundingBox(plane);
            double width = Math.Abs(bounds.Max.X - bounds.Min.X);
            double height = Math.Abs(bounds.Max.Y - bounds.Min.Y);
            double boxArea = width * height;
            if (boxArea > 0.0)
            {
                return boxArea;
            }

            return curve.GetLength();
        }

        private static Curve GetBestOffsetCandidate(Curve source, Plane plane, double signedDistance, double tolerance)
        {
            Curve[] offsets = source.Offset(plane, signedDistance, tolerance, CurveOffsetCornerStyle.Sharp);
            if (offsets == null || offsets.Length == 0)
            {
                return null;
            }

            Curve best = null;
            double bestMetric = double.MinValue;
            double bestLength = double.MinValue;

            for (int index = 0; index < offsets.Length; index++)
            {
                Curve candidate = offsets[index];
                if (candidate == null || !candidate.IsValid)
                {
                    continue;
                }

                double metric = ComputeMetric(candidate, plane);
                double length = candidate.GetLength();
                if (metric > bestMetric || (Math.Abs(metric - bestMetric) <= 1e-9 && length > bestLength))
                {
                    best = candidate;
                    bestMetric = metric;
                    bestLength = length;
                }
            }

            return best;
        }

        private static double GetTolerance()
        {
            return RhinoDoc.ActiveDoc != null
                ? RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                : RhinoMath.ZeroTolerance;
        }
    }
}
