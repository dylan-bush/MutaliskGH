using System.Collections.Generic;

namespace MutaliskGH.Core.Geometry
{
    public sealed class RectangleRebuildResult
    {
        public RectangleRebuildResult(double width, double height, IReadOnlyList<Point3Value> corners)
        {
            Width = width;
            Height = height;
            Corners = corners;
        }

        public double Width { get; }

        public double Height { get; }

        public IReadOnlyList<Point3Value> Corners { get; }
    }
}
