using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class CullEnfLogic
    {
        public static Result<IReadOnlyList<bool>> EvaluateKeepFlags(IReadOnlyList<IReadOnlyList<object>> branches)
        {
            if (branches == null)
            {
                return Result<IReadOnlyList<bool>>.Failure("A branch collection is required.");
            }

            List<bool> keepFlags = new List<bool>(branches.Count);
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                IReadOnlyList<object> branch = branches[branchIndex];
                bool keepBranch = false;

                if (branch != null)
                {
                    for (int itemIndex = 0; itemIndex < branch.Count; itemIndex++)
                    {
                        Result<bool> itemResult = TestNullOrTextLengthZeroLogic.Evaluate(branch[itemIndex]);
                        if (!itemResult.IsSuccess)
                        {
                            return Result<IReadOnlyList<bool>>.Failure(itemResult.ErrorMessage);
                        }

                        if (!itemResult.Value)
                        {
                            continue;
                        }

                        keepBranch = true;
                        break;
                    }
                }

                keepFlags.Add(keepBranch);
            }

            return Result<IReadOnlyList<bool>>.Success(keepFlags);
        }
    }
}
