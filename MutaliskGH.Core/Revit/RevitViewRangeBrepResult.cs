using MutaliskGH.Core.Geometry;

namespace MutaliskGH.Core.Revit
{
    public sealed class RevitViewRangeBrepResult
    {
        public RevitViewRangeBrepResult(
            string viewName,
            Point3Value origin,
            Vector3Value xAxis,
            Vector3Value yAxis,
            Vector3Value zAxis,
            Point3Value min,
            Point3Value max,
            string sourceDescription)
        {
            ViewName = viewName;
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
            Min = min;
            Max = max;
            SourceDescription = sourceDescription;
        }

        public string ViewName { get; }

        public Point3Value Origin { get; }

        public Vector3Value XAxis { get; }

        public Vector3Value YAxis { get; }

        public Vector3Value ZAxis { get; }

        public Point3Value Min { get; }

        public Point3Value Max { get; }

        public string SourceDescription { get; }
    }
}
