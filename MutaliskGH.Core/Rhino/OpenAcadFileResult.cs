namespace MutaliskGH.Core.Rhino
{
    public sealed class OpenAcadFileResult
    {
        public OpenAcadFileResult(string status, bool opened, bool usedExistingInstance, string details, string normalizedPath)
        {
            Status = status;
            Opened = opened;
            UsedExistingInstance = usedExistingInstance;
            Details = details;
            NormalizedPath = normalizedPath;
        }

        public string Status { get; }

        public bool Opened { get; }

        public bool UsedExistingInstance { get; }

        public string Details { get; }

        public string NormalizedPath { get; }
    }
}
