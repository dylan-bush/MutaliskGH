using System.Collections.Generic;

namespace MutaliskGH.Core.Data
{
    public static class PartitionBranchesLogic
    {
        public static Result<IReadOnlyList<int>> NormalizePattern(IReadOnlyList<IReadOnlyList<int>> patternBranches)
        {
            if (patternBranches == null || patternBranches.Count == 0)
            {
                return Result<IReadOnlyList<int>>.Failure("A branch-selection pattern is required.");
            }

            List<int> pattern = new List<int>();
            if (patternBranches.Count == 1)
            {
                IReadOnlyList<int> singleBranch = patternBranches[0];
                if (singleBranch == null || singleBranch.Count == 0)
                {
                    return Result<IReadOnlyList<int>>.Failure("A branch-selection pattern is required.");
                }

                for (int itemIndex = 0; itemIndex < singleBranch.Count; itemIndex++)
                {
                    pattern.Add(singleBranch[itemIndex]);
                }

                return Result<IReadOnlyList<int>>.Success(pattern);
            }

            for (int branchIndex = 0; branchIndex < patternBranches.Count; branchIndex++)
            {
                IReadOnlyList<int> branch = patternBranches[branchIndex];
                if (branch == null || branch.Count != 1)
                {
                    return Result<IReadOnlyList<int>>.Failure(
                        "A grafted branch-selection pattern must contain exactly one value per branch.");
                }

                pattern.Add(branch[0]);
            }

            return Result<IReadOnlyList<int>>.Success(pattern);
        }

        public static Result<PartitionBranchesResult<T>> Partition<T>(
            IReadOnlyList<IReadOnlyList<T>> branches,
            IReadOnlyList<int> pattern,
            int size)
        {
            if (branches == null)
            {
                return Result<PartitionBranchesResult<T>>.Failure("An input tree is required.");
            }

            if (pattern == null || pattern.Count == 0)
            {
                return Result<PartitionBranchesResult<T>>.Failure("A branch-selection pattern is required.");
            }

            if (size <= 0)
            {
                return Result<PartitionBranchesResult<T>>.Failure("A partition size greater than zero is required.");
            }

            List<PartitionBranchesGroup<T>> groups = new List<PartitionBranchesGroup<T>>();
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                IReadOnlyList<T> branch = branches[branchIndex];
                bool shouldPartition = pattern[branchIndex % pattern.Count] != 0;
                List<IReadOnlyList<T>> segments = new List<IReadOnlyList<T>>();

                if (branch == null || branch.Count == 0)
                {
                    segments.Add(new List<T>());
                }
                else if (!shouldPartition)
                {
                    List<T> copy = new List<T>(branch.Count);
                    for (int itemIndex = 0; itemIndex < branch.Count; itemIndex++)
                    {
                        copy.Add(branch[itemIndex]);
                    }

                    segments.Add(copy);
                }
                else
                {
                    for (int startIndex = 0; startIndex < branch.Count; startIndex += size)
                    {
                        List<T> segment = new List<T>();
                        for (int localIndex = 0; localIndex < size; localIndex++)
                        {
                            int itemIndex = startIndex + localIndex;
                            if (itemIndex >= branch.Count)
                            {
                                break;
                            }

                            segment.Add(branch[itemIndex]);
                        }

                        segments.Add(segment);
                    }
                }

                groups.Add(new PartitionBranchesGroup<T>(branchIndex, shouldPartition, segments));
            }

            return Result<PartitionBranchesResult<T>>.Success(new PartitionBranchesResult<T>(groups));
        }
    }
}
