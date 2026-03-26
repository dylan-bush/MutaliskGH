using System.Collections.Generic;

namespace MutaliskGH.Core.Geometry
{
    public static class CurveTrimSelectionLogic
    {
        public static Result<CurveTrimSelectionResult> Select(
            double extendedStartParameter,
            double extendedEndParameter,
            double originalStartParameter,
            double originalEndParameter,
            IReadOnlyList<double> intersectionParameters,
            double tolerance,
            bool trimSingleInteriorIntersection)
        {
            if (extendedEndParameter < extendedStartParameter)
            {
                return Result<CurveTrimSelectionResult>.Failure("Extended curve parameters must be in ascending order.");
            }

            if (originalEndParameter < originalStartParameter)
            {
                return Result<CurveTrimSelectionResult>.Failure("Curve parameters must be in ascending order.");
            }

            if (intersectionParameters == null || intersectionParameters.Count == 0)
            {
                return Result<CurveTrimSelectionResult>.Success(
                    new CurveTrimSelectionResult(extendedStartParameter, extendedEndParameter, false));
            }

            List<double> ordered = new List<double>(intersectionParameters.Count);
            for (int index = 0; index < intersectionParameters.Count; index++)
            {
                AddUnique(ordered, intersectionParameters[index], tolerance);
            }

            ordered.Sort();

            if (ordered.Count == 1)
            {
                double parameter = ordered[0];
                if (parameter < originalStartParameter - tolerance)
                {
                    return Result<CurveTrimSelectionResult>.Success(
                        new CurveTrimSelectionResult(parameter, extendedEndParameter, true));
                }

                if (parameter > originalEndParameter + tolerance)
                {
                    return Result<CurveTrimSelectionResult>.Success(
                        new CurveTrimSelectionResult(extendedStartParameter, parameter, true));
                }

                if (trimSingleInteriorIntersection)
                {
                    double leftSpan = parameter - originalStartParameter;
                    double rightSpan = originalEndParameter - parameter;

                    if (rightSpan >= leftSpan)
                    {
                        return Result<CurveTrimSelectionResult>.Success(
                            new CurveTrimSelectionResult(parameter, extendedEndParameter, true));
                    }

                    return Result<CurveTrimSelectionResult>.Success(
                        new CurveTrimSelectionResult(extendedStartParameter, parameter, true));
                }

                return Result<CurveTrimSelectionResult>.Success(
                    new CurveTrimSelectionResult(extendedStartParameter, extendedEndParameter, false));
            }

            return Result<CurveTrimSelectionResult>.Success(
                new CurveTrimSelectionResult(ordered[0], ordered[ordered.Count - 1], true));
        }

        private static void AddUnique(List<double> values, double candidate, double tolerance)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (System.Math.Abs(values[index] - candidate) <= tolerance)
                {
                    return;
                }
            }

            values.Add(candidate);
        }
    }
}
