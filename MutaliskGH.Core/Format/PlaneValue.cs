namespace MutaliskGH.Core.Format
{
    public sealed class PlaneValue
    {
        public PlaneValue(
            Vector3Value origin,
            Vector3Value xAxis,
            Vector3Value yAxis,
            Vector3Value zAxis)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
        }

        public Vector3Value Origin { get; }

        public Vector3Value XAxis { get; }

        public Vector3Value YAxis { get; }

        public Vector3Value ZAxis { get; }
    }
}
