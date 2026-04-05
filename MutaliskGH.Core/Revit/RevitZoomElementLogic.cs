namespace MutaliskGH.Core.Revit
{
    public static class RevitZoomElementLogic
    {
        public static Result<RevitZoomElementResult> GetTargetData(object element)
        {
            if (element == null)
            {
                return Result<RevitZoomElementResult>.Failure("A Revit element is required.");
            }

            object elementId = RevitReflectionHelper.GetPropertyValue(element, "Id");
            if (!RevitReflectionHelper.IsElementIdValid(elementId))
            {
                return Result<RevitZoomElementResult>.Failure("The supplied element does not expose a valid Revit ElementId.");
            }

            return Result<RevitZoomElementResult>.Success(
                new RevitZoomElementResult(
                    element,
                    elementId,
                    RevitReflectionHelper.GetElementIdInteger(elementId),
                    RevitReflectionHelper.GetCategoryName(element) ?? "No Category"));
        }
    }
}
