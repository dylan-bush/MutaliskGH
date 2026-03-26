using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Geometry;
using MutaliskGH.Framework;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Geometry
{
    public sealed class ExtendTrimCurvesComponent : BaseComponent
    {
        public ExtendTrimCurvesComponent()
            : base(
                "Extend and Trim Curves",
                "ETC",
                "Extend curves to find intersections, then trim each curve to the resolved intersection span.",
                CategoryNames.Geometry)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0a9d1fd8-72d4-4af2-bc4f-b6d76e5d6234"); }
        }

        protected override string IconResourceName
        {
            get { return "ExtendTrimCurves.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddCurveParameter(
                "Curves",
                "C",
                "Curves to extend and trim.",
                GH_ParamAccess.list);

            parameterManager.AddNumberParameter(
                "Extension",
                "E",
                "Extension length applied to both ends before intersection testing.",
                GH_ParamAccess.item,
                1.0);

            parameterManager.AddBooleanParameter(
                "Trim Single",
                "T",
                "When true, a single interior intersection trims the curve to the longer remaining side. When false, the curve remains extended but untrimmed in that case.",
                GH_ParamAccess.item,
                false);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddCurveParameter(
                "Curves",
                "C",
                "Adjusted curves after extension and trimming.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Intersection Indices",
                "I",
                "Partner curve indices that contributed intersections for each output curve.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<Curve> curves = new List<Curve>();
            if (!dataAccess.GetDataList(0, curves) || curves.Count == 0)
            {
                return;
            }

            double extension = 1.0;
            dataAccess.GetData(1, ref extension);
            if (extension < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Extension length must be non-negative.");
                return;
            }

            bool trimSingle = false;
            dataAccess.GetData(2, ref trimSingle);

            double tolerance = GetTolerance();
            List<Curve> adjustedCurves = new List<Curve>(curves.Count);
            GH_Structure<GH_Integer> intersectionIndices = new GH_Structure<GH_Integer>();

            List<Curve> extendedCurves = new List<Curve>(curves.Count);
            List<double> originalStarts = new List<double>(curves.Count);
            List<double> originalEnds = new List<double>(curves.Count);
            List<List<double>> intersectionParameters = new List<List<double>>(curves.Count);
            List<List<int>> partnerIndices = new List<List<int>>(curves.Count);

            for (int index = 0; index < curves.Count; index++)
            {
                Curve curve = curves[index];
                if (curve == null || !curve.IsValid)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid curves were skipped.");
                    extendedCurves.Add(null);
                    originalStarts.Add(0.0);
                    originalEnds.Add(0.0);
                    intersectionParameters.Add(new List<double>());
                    partnerIndices.Add(new List<int>());
                    continue;
                }

                Curve extended = ExtendCurve(curve, extension);
                if (extended == null || !extended.IsValid)
                {
                    extended = curve.DuplicateCurve();
                }

                extendedCurves.Add(extended);
                originalStarts.Add(GetClosestParameter(extended, curve.PointAtStart));
                originalEnds.Add(GetClosestParameter(extended, curve.PointAtEnd));
                intersectionParameters.Add(new List<double>());
                partnerIndices.Add(new List<int>());
            }

            for (int firstIndex = 0; firstIndex < extendedCurves.Count; firstIndex++)
            {
                Curve first = extendedCurves[firstIndex];
                if (first == null)
                {
                    continue;
                }

                for (int secondIndex = firstIndex + 1; secondIndex < extendedCurves.Count; secondIndex++)
                {
                    Curve second = extendedCurves[secondIndex];
                    if (second == null)
                    {
                        continue;
                    }

                    CurveIntersections intersections = Intersection.CurveCurve(first, second, tolerance, tolerance);
                    if (intersections == null || intersections.Count == 0)
                    {
                        continue;
                    }

                    bool foundPointIntersection = false;
                    for (int eventIndex = 0; eventIndex < intersections.Count; eventIndex++)
                    {
                        IntersectionEvent intersection = intersections[eventIndex];
                        if (!intersection.IsPoint)
                        {
                            continue;
                        }

                        foundPointIntersection = true;
                        intersectionParameters[firstIndex].Add(intersection.ParameterA);
                        intersectionParameters[secondIndex].Add(intersection.ParameterB);
                    }

                    if (foundPointIntersection)
                    {
                        AddUniqueIndex(partnerIndices[firstIndex], secondIndex);
                        AddUniqueIndex(partnerIndices[secondIndex], firstIndex);
                    }
                }
            }

            for (int index = 0; index < curves.Count; index++)
            {
                Curve curve = curves[index];
                if (curve == null || !curve.IsValid)
                {
                    continue;
                }

                Result<CurveTrimSelectionResult> selection = CurveTrimSelectionLogic.Select(
                    extendedCurves[index].Domain.Min,
                    extendedCurves[index].Domain.Max,
                    originalStarts[index],
                    originalEnds[index],
                    intersectionParameters[index],
                    tolerance,
                    trimSingle);

                if (ReportFailure(selection))
                {
                    return;
                }

                Curve adjusted = extendedCurves[index].DuplicateCurve();
                if (selection.Value.ShouldTrim)
                {
                    Curve trimmed = extendedCurves[index].Trim(selection.Value.StartParameter, selection.Value.EndParameter);
                    if (trimmed != null && trimmed.IsValid)
                    {
                        adjusted = trimmed;
                    }
                }

                adjustedCurves.Add(adjusted);

                GH_Path path = new GH_Path(index);
                for (int partnerIndex = 0; partnerIndex < partnerIndices[index].Count; partnerIndex++)
                {
                    intersectionIndices.Append(new GH_Integer(partnerIndices[index][partnerIndex]), path);
                }
            }

            dataAccess.SetDataList(0, adjustedCurves);
            dataAccess.SetDataTree(1, intersectionIndices);
        }

        private static Curve ExtendCurve(Curve curve, double extension)
        {
            if (curve.IsClosed || extension <= RhinoMath.ZeroTolerance)
            {
                return curve.DuplicateCurve();
            }

            Curve extended = curve.Extend(CurveEnd.Both, extension, CurveExtensionStyle.Line);
            return extended ?? curve.DuplicateCurve();
        }

        private static double GetClosestParameter(Curve curve, Point3d point)
        {
            if (curve.ClosestPoint(point, out double parameter))
            {
                return parameter;
            }

            return curve.Domain.Min;
        }

        private static void AddUniqueIndex(List<int> indices, int candidate)
        {
            for (int index = 0; index < indices.Count; index++)
            {
                if (indices[index] == candidate)
                {
                    return;
                }
            }

            indices.Add(candidate);
        }

        private static double GetTolerance()
        {
            return RhinoDoc.ActiveDoc != null
                ? RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
                : RhinoMath.ZeroTolerance;
        }
    }
}
