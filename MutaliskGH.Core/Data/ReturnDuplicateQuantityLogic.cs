using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class ReturnDuplicateQuantityLogic
    {
        public static Result<DuplicateQuantityResult<T>> Analyze<T>(IReadOnlyList<T> values)
        {
            if (values == null)
            {
                return Result<DuplicateQuantityResult<T>>.Failure("A value list is required.");
            }

            List<T> distinctValues = new List<T>();
            List<int> quantities = new List<int>();
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int index = 0; index < values.Count; index++)
            {
                T value = values[index];
                int existingIndex = -1;
                for (int distinctIndex = 0; distinctIndex < distinctValues.Count; distinctIndex++)
                {
                    if (!comparer.Equals(distinctValues[distinctIndex], value))
                    {
                        continue;
                    }

                    existingIndex = distinctIndex;
                    break;
                }

                if (existingIndex >= 0)
                {
                    quantities[existingIndex]++;
                    continue;
                }

                distinctValues.Add(value);
                quantities.Add(1);
            }

            DuplicateQuantityResult<T> result = new DuplicateQuantityResult<T>(distinctValues, quantities);
            return Result<DuplicateQuantityResult<T>>.Success(result);
        }
    }
}
