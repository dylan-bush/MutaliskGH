namespace MutaliskGH.Core.Geometry
{
    public sealed class OrientedBasisValue
    {
        public OrientedBasisValue(Vector3Value xAxis, Vector3Value yAxis, Vector3Value zAxis)
        {
            XAxis = xAxis;
            YAxis = yAxis;
            ZAxis = zAxis;
        }

        public Vector3Value XAxis { get; }

        public Vector3Value YAxis { get; }

        public Vector3Value ZAxis { get; }
    }
}
