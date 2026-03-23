namespace MutaliskGH.Core.Geometry
{
    public sealed class OffsetCandidateValue
    {
        public OffsetCandidateValue(bool exists, double metric, double length)
        {
            Exists = exists;
            Metric = metric;
            Length = length;
        }

        public bool Exists { get; }

        public double Metric { get; }

        public double Length { get; }
    }
}
