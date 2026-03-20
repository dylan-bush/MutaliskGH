using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class IntegerSeriesLogic
    {
        public static Result<IReadOnlyList<int>> CreateInclusive(int start, int end)
        {
            long count = Math.Abs((long)end - start) + 1L;
            if (count > int.MaxValue)
            {
                return Result<IReadOnlyList<int>>.Failure("Integer series range is too large.");
            }

            List<int> values = new List<int>((int)count);
            int step = start <= end ? 1 : -1;

            for (int value = start; ; value += step)
            {
                values.Add(value);
                if (value == end)
                {
                    break;
                }
            }

            return Result<IReadOnlyList<int>>.Success(values);
        }
    }
}
