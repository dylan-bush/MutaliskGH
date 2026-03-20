using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class ReturnDuplicateIndexLogic
    {
        public static Result<DuplicateIndexResult<T>> Analyze<T>(IReadOnlyList<T> values)
        {
            if (values == null)
            {
                return Result<DuplicateIndexResult<T>>.Failure("A value list is required.");
            }

            List<T> distinctValues = new List<T>();
            List<int> retainedIndexes = new List<int>();
            List<int> culledIndexes = new List<int>();
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int index = 0; index < values.Count; index++)
            {
                T value = values[index];
                bool isDuplicate = false;
                for (int distinctIndex = 0; distinctIndex < distinctValues.Count; distinctIndex++)
                {
                    if (!comparer.Equals(distinctValues[distinctIndex], value))
                    {
                        continue;
                    }

                    isDuplicate = true;
                    break;
                }

                if (!isDuplicate)
                {
                    distinctValues.Add(value);
                    retainedIndexes.Add(index);
                    continue;
                }

                culledIndexes.Add(index);
            }

            DuplicateIndexResult<T> result = new DuplicateIndexResult<T>(
                distinctValues,
                retainedIndexes,
                culledIndexes);

            return Result<DuplicateIndexResult<T>>.Success(result);
        }
    }
}
