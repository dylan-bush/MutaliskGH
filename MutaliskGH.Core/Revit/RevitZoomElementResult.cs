namespace MutaliskGH.Core.Revit
{
    public sealed class RevitZoomElementResult
    {
        public RevitZoomElementResult(object element, object elementId, int? elementIdInteger, string categoryName)
        {
            Element = element;
            ElementId = elementId;
            ElementIdInteger = elementIdInteger;
            CategoryName = categoryName;
        }

        public object Element { get; }

        public object ElementId { get; }

        public int? ElementIdInteger { get; }

        public string CategoryName { get; }
    }
}
