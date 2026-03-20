namespace MutaliskGH.Core.Text
{
    public sealed class RegexTextReplaceResult
    {
        public RegexTextReplaceResult(string text, bool success, string errorMessage, bool wasChanged)
        {
            Text = text;
            Success = success;
            ErrorMessage = errorMessage;
            WasChanged = wasChanged;
        }

        public string Text { get; }

        public bool Success { get; }

        public string ErrorMessage { get; }

        public bool WasChanged { get; }
    }
}
