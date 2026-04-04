using System;

namespace MutaliskGH.Core.Revit
{
    public static class RevitSpotElevationReferenceLogic
    {
        public static Result<RevitSpotElevationReferenceResult> GetReferenceData(object spotDimension)
        {
            if (spotDimension == null)
            {
                return Result<RevitSpotElevationReferenceResult>.Failure("Input spot dimension is null.");
            }

            object document = RevitReflectionHelper.GetPropertyValue(spotDimension, "Document");
            if (document == null)
            {
                return Result<RevitSpotElevationReferenceResult>.Failure("Input must be a valid Revit element.");
            }

            if (!string.Equals(spotDimension.GetType().Name, "SpotDimension", StringComparison.Ordinal))
            {
                return Result<RevitSpotElevationReferenceResult>.Failure("Input must be a SpotDimension element.");
            }

            var references = RevitReflectionHelper.ToObjectList(RevitReflectionHelper.GetPropertyValue(spotDimension, "References"));
            if (references.Count == 0)
            {
                return Result<RevitSpotElevationReferenceResult>.Success(new RevitSpotElevationReferenceResult(null, null, null, null));
            }

            object reference = references[0];
            object elementId = RevitReflectionHelper.GetPropertyValue(reference, "ElementId");
            if (!RevitReflectionHelper.IsElementIdValid(elementId))
            {
                return Result<RevitSpotElevationReferenceResult>.Success(new RevitSpotElevationReferenceResult(null, null, null, null));
            }

            object referencedElement = RevitReflectionHelper.GetDocumentElement(document, elementId);
            if (referencedElement == null)
            {
                return Result<RevitSpotElevationReferenceResult>.Success(new RevitSpotElevationReferenceResult(null, null, null, null));
            }

            object parent = RevitParentElementLogic.ResolveAdaptivePointParent(referencedElement);
            int? parentId = RevitReflectionHelper.GetElementIdInteger(RevitReflectionHelper.GetPropertyValue(parent, "Id"));

            return Result<RevitSpotElevationReferenceResult>.Success(
                new RevitSpotElevationReferenceResult(
                    referencedElement,
                    parent,
                    parentId,
                    RevitReflectionHelper.GetCategoryName(referencedElement)));
        }
    }
}
