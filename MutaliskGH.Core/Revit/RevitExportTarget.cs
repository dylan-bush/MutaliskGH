namespace MutaliskGH.Core.Revit
{
    public sealed class RevitExportTarget
    {
        public RevitExportTarget(object view, string path)
        {
            View = view;
            Path = path;
        }

        public object View { get; }

        public string Path { get; }
    }
}
