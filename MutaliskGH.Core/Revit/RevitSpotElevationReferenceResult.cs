namespace MutaliskGH.Core.Revit
{
    public sealed class RevitSpotElevationReferenceResult
    {
        public RevitSpotElevationReferenceResult(
            object reference,
            object referenceParent,
            int? parentElementId,
            string categoryName)
        {
            Reference = reference;
            ReferenceParent = referenceParent;
            ParentElementId = parentElementId;
            CategoryName = categoryName;
        }

        public object Reference { get; }

        public object ReferenceParent { get; }

        public int? ParentElementId { get; }

        public string CategoryName { get; }
    }
}
