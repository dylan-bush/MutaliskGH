using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class BranchByMemberLogic
    {
        public static Result<BranchByMemberResult<T>> Analyze<T>(IReadOnlyList<T> keys)
        {
            if (keys == null)
            {
                return Result<BranchByMemberResult<T>>.Failure("A key list is required.");
            }

            List<T> distinctKeys = new List<T>();
            List<List<bool>> matchPatterns = new List<List<bool>>();
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int index = 0; index < keys.Count; index++)
            {
                T key = keys[index];
                int distinctIndex = IndexOf(distinctKeys, key, comparer);
                if (distinctIndex >= 0)
                {
                    continue;
                }

                distinctKeys.Add(key);
                List<bool> pattern = new List<bool>(keys.Count);
                for (int patternIndex = 0; patternIndex < keys.Count; patternIndex++)
                {
                    pattern.Add(comparer.Equals(keys[patternIndex], key));
                }

                matchPatterns.Add(pattern);
            }

            List<IReadOnlyList<bool>> readOnlyPatterns = new List<IReadOnlyList<bool>>(matchPatterns.Count);
            for (int index = 0; index < matchPatterns.Count; index++)
            {
                readOnlyPatterns.Add(matchPatterns[index]);
            }

            BranchByMemberResult<T> result = new BranchByMemberResult<T>(
                distinctKeys,
                readOnlyPatterns);

            return Result<BranchByMemberResult<T>>.Success(result);
        }

        private static int IndexOf<T>(IReadOnlyList<T> values, T target, EqualityComparer<T> comparer)
        {
            for (int index = 0; index < values.Count; index++)
            {
                if (comparer.Equals(values[index], target))
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
