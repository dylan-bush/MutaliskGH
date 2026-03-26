using MutaliskGH.Core.Data;
using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public static class PreviewColorByValueLogic
    {
        public static Result<PreviewColorByValueResult> Evaluate(
            IReadOnlyList<object> geometry,
            IReadOnlyList<object> values,
            object seed,
            bool useGradient)
        {
            if (geometry == null || values == null)
            {
                return Result<PreviewColorByValueResult>.Failure("Geometry and value lists are required.");
            }

            int count = System.Math.Min(geometry.Count, values.Count);
            List<object> filteredGeometry = new List<object>(count);
            List<object> filteredValues = new List<object>(count);

            for (int index = 0; index < count; index++)
            {
                filteredGeometry.Add(geometry[index]);
                filteredValues.Add(values[index]);
            }

            Result<BranchByMemberResult<object>> groupingResult = BranchByMemberLogic.Analyze(filteredValues);
            if (groupingResult.IsFailure)
            {
                return Result<PreviewColorByValueResult>.Failure(groupingResult.ErrorMessage);
            }

            IReadOnlyList<object> distinctValues = groupingResult.Value.DistinctKeys;
            IReadOnlyList<IReadOnlyList<bool>> matchPatterns = groupingResult.Value.MatchPatterns;

            IReadOnlyList<PaletteColorValue> branchColors;
            if (useGradient)
            {
                Result<IReadOnlyList<PaletteColorValue>> gradientResult = GradientPaletteLogic.CreateRainbow(distinctValues.Count);
                if (gradientResult.IsFailure)
                {
                    return Result<PreviewColorByValueResult>.Failure(gradientResult.ErrorMessage);
                }

                branchColors = gradientResult.Value;
            }
            else
            {
                Result<PaletteEngineResult> paletteResult = PaletteEngineLogic.Generate(distinctValues, 0.65, seed, false, 0.42, 0.18);
                if (paletteResult.IsFailure)
                {
                    return Result<PreviewColorByValueResult>.Failure(paletteResult.ErrorMessage);
                }

                branchColors = paletteResult.Value.OutputColors;
            }

            return Result<PreviewColorByValueResult>.Success(
                new PreviewColorByValueResult(
                    filteredGeometry,
                    filteredValues,
                    distinctValues,
                    matchPatterns,
                    branchColors));
        }
    }
}
