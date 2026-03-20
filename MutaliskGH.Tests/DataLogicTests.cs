using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using Xunit;

namespace MutaliskGH.Tests
{
    public class DataLogicTests
    {
        [Fact]
        public void ConvertToBoolean_MapsCommonFalseLikeValuesToFalse()
        {
            Result<bool> nullResult = TestNullOrTextLengthZeroLogic.Evaluate(null);
            Result<bool> emptyResult = TestNullOrTextLengthZeroLogic.Evaluate(string.Empty);
            Result<bool> falseTextResult = TestNullOrTextLengthZeroLogic.Evaluate("False");
            Result<bool> zeroTextResult = TestNullOrTextLengthZeroLogic.Evaluate("0");
            Result<bool> falseBoolResult = TestNullOrTextLengthZeroLogic.Evaluate(false);
            Result<bool> zeroNumberResult = TestNullOrTextLengthZeroLogic.Evaluate(0);

            Assert.True(nullResult.IsSuccess);
            Assert.False(nullResult.Value);
            Assert.True(emptyResult.IsSuccess);
            Assert.False(emptyResult.Value);
            Assert.True(falseTextResult.IsSuccess);
            Assert.False(falseTextResult.Value);
            Assert.True(zeroTextResult.IsSuccess);
            Assert.False(zeroTextResult.Value);
            Assert.True(falseBoolResult.IsSuccess);
            Assert.False(falseBoolResult.Value);
            Assert.True(zeroNumberResult.IsSuccess);
            Assert.False(zeroNumberResult.Value);
        }

        [Fact]
        public void ConvertToBoolean_MapsValidValuesToTrue()
        {
            Result<bool> textResult = TestNullOrTextLengthZeroLogic.Evaluate("alpha");
            Result<bool> trueTextResult = TestNullOrTextLengthZeroLogic.Evaluate("True");
            Result<bool> oneNumberResult = TestNullOrTextLengthZeroLogic.Evaluate(1);
            Result<bool> objectResult = TestNullOrTextLengthZeroLogic.Evaluate(new object());

            Assert.True(textResult.IsSuccess);
            Assert.True(textResult.Value);
            Assert.True(trueTextResult.IsSuccess);
            Assert.True(trueTextResult.Value);
            Assert.True(oneNumberResult.IsSuccess);
            Assert.True(oneNumberResult.Value);
            Assert.True(objectResult.IsSuccess);
            Assert.True(objectResult.Value);
        }

        [Fact]
        public void ReturnDuplicateIndex_PreservesFirstOccurrenceOrder()
        {
            Result<DuplicateIndexResult<string>> result = ReturnDuplicateIndexLogic.Analyze(
                new[] { "A", "B", "A", "C", "B", "D" });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "A", "B", "C", "D" }, result.Value.DistinctValues);
            Assert.Equal(new[] { 0, 1, 3, 5 }, result.Value.RetainedIndexes);
            Assert.Equal(new[] { 2, 4 }, result.Value.CulledIndexes);
        }

        [Fact]
        public void ReturnDuplicateQuantity_CountsDistinctValues()
        {
            Result<DuplicateQuantityResult<string>> result = ReturnDuplicateQuantityLogic.Analyze(
                new[] { "A", "B", "A", "C", "B", "A" });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "A", "B", "C" }, result.Value.DistinctValues);
            Assert.Equal(new[] { 3, 2, 1 }, result.Value.Quantities);
        }

        [Fact]
        public void DuplicateLogic_HandlesNullValues()
        {
            Result<DuplicateIndexResult<string>> indexResult = ReturnDuplicateIndexLogic.Analyze(
                new string[] { null, "A", null, "B" });
            Result<DuplicateQuantityResult<string>> quantityResult = ReturnDuplicateQuantityLogic.Analyze(
                new string[] { null, "A", null, "B", "A" });

            Assert.True(indexResult.IsSuccess);
            Assert.Equal(new string[] { null, "A", "B" }, indexResult.Value.DistinctValues);
            Assert.Equal(new[] { 0, 1, 3 }, indexResult.Value.RetainedIndexes);
            Assert.Equal(new[] { 2 }, indexResult.Value.CulledIndexes);

            Assert.True(quantityResult.IsSuccess);
            Assert.Equal(new string[] { null, "A", "B" }, quantityResult.Value.DistinctValues);
            Assert.Equal(new[] { 2, 2, 1 }, quantityResult.Value.Quantities);
        }

        [Fact]
        public void IntegerSeries_CreatesInclusiveAscendingAndDescendingRanges()
        {
            Result<System.Collections.Generic.IReadOnlyList<int>> ascending = IntegerSeriesLogic.CreateInclusive(2, 5);
            Result<System.Collections.Generic.IReadOnlyList<int>> descending = IntegerSeriesLogic.CreateInclusive(5, 2);

            Assert.True(ascending.IsSuccess);
            Assert.Equal(new[] { 2, 3, 4, 5 }, ascending.Value);

            Assert.True(descending.IsSuccess);
            Assert.Equal(new[] { 5, 4, 3, 2 }, descending.Value);
        }

        [Fact]
        public void BranchByMember_CreatesPatternsInFirstOccurrenceOrder()
        {
            Result<BranchByMemberResult<string>> result = BranchByMemberLogic.Analyze(
                new[] { "A", "B", "A", "C", "B" });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "A", "B", "C" }, result.Value.DistinctKeys);
            Assert.Equal(new[] { true, false, true, false, false }, result.Value.MatchPatterns[0]);
            Assert.Equal(new[] { false, true, false, false, true }, result.Value.MatchPatterns[1]);
            Assert.Equal(new[] { false, false, false, true, false }, result.Value.MatchPatterns[2]);
        }

        [Fact]
        public void CullEnf_RemovesEmptyNullAndFalseOnlyBranches()
        {
            System.Collections.Generic.IReadOnlyList<System.Collections.Generic.IReadOnlyList<object>> branches =
                new System.Collections.Generic.IReadOnlyList<object>[]
                {
                    new object[] { },
                    new object[] { null },
                    new object[] { "False", false, 0 },
                    new object[] { "A", false },
                    new object[] { true }
                };

            Result<System.Collections.Generic.IReadOnlyList<bool>> result = CullEnfLogic.EvaluateKeepFlags(branches);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { false, false, false, true, true }, result.Value);
        }

        [Fact]
        public void PartitionBranches_UnweavesEachPartitionIntoGuideGroups()
        {
            Result<PartitionBranchesResult<string>> result = PartitionBranchesLogic.Partition(
                new System.Collections.Generic.IReadOnlyList<string>[]
                {
                    new[] { "A", "B", "C" },
                    new[] { "D", "E", "F", "G" },
                    new[] { "H", "I", "J", "K" }
                },
                new[] { 0, 1, 1 },
                3);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.Groups.Count);

            Assert.Equal(0, result.Value.Groups[0].BranchIndex);
            Assert.False(result.Value.Groups[0].ShouldPartition);
            Assert.Single(result.Value.Groups[0].Segments);
            Assert.Equal(new[] { "A", "B", "C" }, result.Value.Groups[0].Segments[0]);

            Assert.Equal(1, result.Value.Groups[1].BranchIndex);
            Assert.True(result.Value.Groups[1].ShouldPartition);
            Assert.Equal(2, result.Value.Groups[1].Segments.Count);
            Assert.Equal(new[] { "D", "E", "F" }, result.Value.Groups[1].Segments[0]);
            Assert.Equal(new[] { "G" }, result.Value.Groups[1].Segments[1]);

            Assert.Equal(2, result.Value.Groups[2].BranchIndex);
            Assert.True(result.Value.Groups[2].ShouldPartition);
            Assert.Equal(2, result.Value.Groups[2].Segments.Count);
            Assert.Equal(new[] { "H", "I", "J" }, result.Value.Groups[2].Segments[0]);
            Assert.Equal(new[] { "K" }, result.Value.Groups[2].Segments[1]);
        }

        [Fact]
        public void PartitionBranches_CyclesPatternAcrossBranches()
        {
            Result<PartitionBranchesResult<int>> result = PartitionBranchesLogic.Partition(
                new System.Collections.Generic.IReadOnlyList<int>[]
                {
                    new[] { 1, 2 },
                    new[] { 3, 4 },
                    new[] { 5, 6 },
                    new[] { 7, 8 }
                },
                new[] { 0, 1 },
                1);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.Groups[0].ShouldPartition);
            Assert.True(result.Value.Groups[1].ShouldPartition);
            Assert.False(result.Value.Groups[2].ShouldPartition);
            Assert.True(result.Value.Groups[3].ShouldPartition);
            Assert.Equal(2, result.Value.Groups[1].Segments.Count);
            Assert.Equal(new[] { 3 }, result.Value.Groups[1].Segments[0]);
            Assert.Equal(new[] { 4 }, result.Value.Groups[1].Segments[1]);
        }

        [Fact]
        public void PartitionBranches_NormalizePattern_AcceptsFlatListBranch()
        {
            Result<System.Collections.Generic.IReadOnlyList<int>> result = PartitionBranchesLogic.NormalizePattern(
                new System.Collections.Generic.IReadOnlyList<int>[]
                {
                    new[] { 0, 1, 1 }
                });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 0, 1, 1 }, result.Value);
        }

        [Fact]
        public void PartitionBranches_NormalizePattern_AcceptsGraftedBranches()
        {
            Result<System.Collections.Generic.IReadOnlyList<int>> result = PartitionBranchesLogic.NormalizePattern(
                new System.Collections.Generic.IReadOnlyList<int>[]
                {
                    new[] { 0 },
                    new[] { 1 },
                    new[] { 1 }
                });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { 0, 1, 1 }, result.Value);
        }
    }
}
