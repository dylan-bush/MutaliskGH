using MutaliskGH.Core.Geometry;
using System;

namespace MutaliskGH.Core.Revit
{
    public static class RevitViewRangeBrepLogic
    {
        public static Result<RevitViewRangeBrepResult> GetViewData(object view)
        {
            if (view == null)
            {
                return Result<RevitViewRangeBrepResult>.Failure("Input view is null.");
            }

            object document = RevitReflectionHelper.GetPropertyValue(view, "Document");
            if (document == null)
            {
                return Result<RevitViewRangeBrepResult>.Failure("Input must be a valid Revit view.");
            }

            string viewName = RevitReflectionHelper.GetPropertyValue(view, "Name") as string ?? view.ToString();
            string source = "CropBox";

            object volumeBox = RevitReflectionHelper.GetPropertyValue(view, "CropBox");
            if (volumeBox == null)
            {
                volumeBox = RevitReflectionHelper.InvokeMethod(view, "GetSectionBox");
                source = "SectionBox";
            }

            if (volumeBox == null)
            {
                return Result<RevitViewRangeBrepResult>.Failure("The view does not expose a crop or section box.");
            }

            Result<BoundingBoxFrame> frameResult = TryReadBoundingBox(volumeBox);
            if (frameResult.IsFailure)
            {
                return Result<RevitViewRangeBrepResult>.Failure(frameResult.ErrorMessage);
            }

            BoundingBoxFrame frame = frameResult.Value;
            Result<Tuple<double, double>> planRangeResult = TryReadPlanRange(view, frame);
            if (planRangeResult.IsSuccess)
            {
                double minZ = Math.Min(planRangeResult.Value.Item1, planRangeResult.Value.Item2);
                double maxZ = Math.Max(planRangeResult.Value.Item1, planRangeResult.Value.Item2);
                frame = frame.WithZRange(minZ, maxZ);
                source = "CropBox + ViewRange";
            }

            if (Math.Abs(frame.Max.X - frame.Min.X) <= 1e-9 ||
                Math.Abs(frame.Max.Y - frame.Min.Y) <= 1e-9 ||
                Math.Abs(frame.Max.Z - frame.Min.Z) <= 1e-9)
            {
                return Result<RevitViewRangeBrepResult>.Failure("The view volume is degenerate.");
            }

            return Result<RevitViewRangeBrepResult>.Success(
                new RevitViewRangeBrepResult(
                    viewName,
                    frame.Origin,
                    frame.XAxis,
                    frame.YAxis,
                    frame.ZAxis,
                    frame.Min,
                    frame.Max,
                    source));
        }

        private static Result<BoundingBoxFrame> TryReadBoundingBox(object boundingBox)
        {
            if (boundingBox == null)
            {
                return Result<BoundingBoxFrame>.Failure("The view volume box is null.");
            }

            Point3Value min = TryReadPoint(RevitReflectionHelper.GetPropertyValue(boundingBox, "Min"));
            Point3Value max = TryReadPoint(RevitReflectionHelper.GetPropertyValue(boundingBox, "Max"));
            if (min == null || max == null)
            {
                return Result<BoundingBoxFrame>.Failure("The view volume box min/max points could not be read.");
            }

            object transform = RevitReflectionHelper.GetPropertyValue(boundingBox, "Transform");
            Point3Value origin = TryReadPoint(RevitReflectionHelper.GetPropertyValue(transform, "Origin")) ?? new Point3Value(0.0, 0.0, 0.0);
            Vector3Value xAxis = TryReadVector(RevitReflectionHelper.GetPropertyValue(transform, "BasisX")) ?? new Vector3Value(1.0, 0.0, 0.0);
            Vector3Value yAxis = TryReadVector(RevitReflectionHelper.GetPropertyValue(transform, "BasisY")) ?? new Vector3Value(0.0, 1.0, 0.0);
            Vector3Value zAxis = TryReadVector(RevitReflectionHelper.GetPropertyValue(transform, "BasisZ")) ?? new Vector3Value(0.0, 0.0, 1.0);

            return Result<BoundingBoxFrame>.Success(new BoundingBoxFrame(origin, xAxis, yAxis, zAxis, min, max));
        }

        private static Result<Tuple<double, double>> TryReadPlanRange(object view, BoundingBoxFrame frame)
        {
            object range = RevitReflectionHelper.InvokeMethod(view, "GetViewRange");
            if (range == null)
            {
                return Result<Tuple<double, double>>.Failure("No plan range is available for this view.");
            }

            object level = RevitReflectionHelper.GetPropertyValue(view, "GenLevel");
            double? levelElevation = TryReadDouble(RevitReflectionHelper.GetPropertyValue(level, "Elevation"));
            if (!levelElevation.HasValue)
            {
                return Result<Tuple<double, double>>.Failure("The plan view level elevation is unavailable.");
            }

            double? topOffset = TryReadPlanOffset(range, "TopClipPlane");
            double? depthOffset = TryReadPlanOffset(range, "ViewDepthPlane") ?? TryReadPlanOffset(range, "BottomClipPlane");
            if (!topOffset.HasValue || !depthOffset.HasValue)
            {
                return Result<Tuple<double, double>>.Failure("The plan view top/depth offsets are unavailable.");
            }

            double topWorldZ = levelElevation.Value + topOffset.Value;
            double depthWorldZ = levelElevation.Value + depthOffset.Value;
            double topLocalZ = ProjectWorldZToLocalZ(frame, topWorldZ);
            double depthLocalZ = ProjectWorldZToLocalZ(frame, depthWorldZ);
            return Result<Tuple<double, double>>.Success(Tuple.Create(depthLocalZ, topLocalZ));
        }

        private static double? TryReadPlanOffset(object range, string planeName)
        {
            object enumValue = RevitReflectionHelper.ParseEnumArgument(range.GetType().Assembly, "Autodesk.Revit.DB.PlanViewPlane", planeName);
            if (enumValue == null)
            {
                return null;
            }

            object value = RevitReflectionHelper.InvokeMethod(range, "GetOffset", enumValue);
            return TryReadDouble(value);
        }

        private static double ProjectWorldZToLocalZ(BoundingBoxFrame frame, double worldZ)
        {
            Vector3Value unitZ = Normalize(frame.ZAxis);
            double deltaWorldZ = worldZ - frame.Origin.Z;
            return deltaWorldZ * unitZ.Z;
        }

        private static Point3Value TryReadPoint(object point)
        {
            if (point == null)
            {
                return null;
            }

            double? x = TryReadDouble(RevitReflectionHelper.GetPropertyValue(point, "X"));
            double? y = TryReadDouble(RevitReflectionHelper.GetPropertyValue(point, "Y"));
            double? z = TryReadDouble(RevitReflectionHelper.GetPropertyValue(point, "Z"));
            if (!x.HasValue || !y.HasValue || !z.HasValue)
            {
                return null;
            }

            return new Point3Value(x.Value, y.Value, z.Value);
        }

        private static Vector3Value TryReadVector(object vector)
        {
            Point3Value point = TryReadPoint(vector);
            return point == null ? null : new Vector3Value(point.X, point.Y, point.Z);
        }

        private static double? TryReadDouble(object value)
        {
            if (value is double doubleValue)
            {
                return doubleValue;
            }

            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            if (value == null)
            {
                return null;
            }

            if (double.TryParse(value.ToString(), out double parsed))
            {
                return parsed;
            }

            return null;
        }

        private static Vector3Value Normalize(Vector3Value vector)
        {
            double length = Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
            if (length <= 1e-9)
            {
                return new Vector3Value(0.0, 0.0, 1.0);
            }

            return new Vector3Value(vector.X / length, vector.Y / length, vector.Z / length);
        }

        private sealed class BoundingBoxFrame
        {
            public BoundingBoxFrame(
                Point3Value origin,
                Vector3Value xAxis,
                Vector3Value yAxis,
                Vector3Value zAxis,
                Point3Value min,
                Point3Value max)
            {
                Origin = origin;
                XAxis = xAxis;
                YAxis = yAxis;
                ZAxis = zAxis;
                Min = min;
                Max = max;
            }

            public Point3Value Origin { get; }

            public Vector3Value XAxis { get; }

            public Vector3Value YAxis { get; }

            public Vector3Value ZAxis { get; }

            public Point3Value Min { get; }

            public Point3Value Max { get; }

            public BoundingBoxFrame WithZRange(double minZ, double maxZ)
            {
                return new BoundingBoxFrame(
                    Origin,
                    XAxis,
                    YAxis,
                    ZAxis,
                    new Point3Value(Min.X, Min.Y, minZ),
                    new Point3Value(Max.X, Max.Y, maxZ));
            }
        }
    }
}
