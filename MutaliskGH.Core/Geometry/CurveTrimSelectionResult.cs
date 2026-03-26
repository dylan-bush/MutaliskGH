namespace MutaliskGH.Core.Geometry
{
    public sealed class CurveTrimSelectionResult
    {
        public CurveTrimSelectionResult(double startParameter, double endParameter, bool shouldTrim)
        {
            StartParameter = startParameter;
            EndParameter = endParameter;
            ShouldTrim = shouldTrim;
        }

        public double StartParameter { get; }

        public double EndParameter { get; }

        public bool ShouldTrim { get; }
    }
}
