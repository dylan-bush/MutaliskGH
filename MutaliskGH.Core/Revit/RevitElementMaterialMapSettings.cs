namespace MutaliskGH.Core.Revit
{
    public sealed class RevitElementMaterialMapSettings
    {
        public RevitElementMaterialMapSettings(double minVolumeFt3, string detailLevelName, bool emitGeometry, bool debug)
        {
            MinVolumeFt3 = minVolumeFt3;
            DetailLevelName = detailLevelName;
            EmitGeometry = emitGeometry;
            Debug = debug;
        }

        public double MinVolumeFt3 { get; }

        public string DetailLevelName { get; }

        public bool EmitGeometry { get; }

        public bool Debug { get; }
    }
}
