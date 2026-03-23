namespace MutaliskGH.Core.Geometry
{
    public sealed class Point3Value
    {
        public Point3Value(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }
}
