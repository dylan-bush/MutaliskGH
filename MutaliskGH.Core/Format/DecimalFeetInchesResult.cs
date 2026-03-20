namespace MutaliskGH.Core.Format
{
    public sealed class DecimalFeetInchesResult
    {
        public DecimalFeetInchesResult(string formattedText, double roundedInches)
        {
            FormattedText = formattedText;
            RoundedInches = roundedInches;
        }

        public string FormattedText { get; }

        public double RoundedInches { get; }
    }
}
