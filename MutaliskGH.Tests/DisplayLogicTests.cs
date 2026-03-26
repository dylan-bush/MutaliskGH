using MutaliskGH.Core;
using MutaliskGH.Core.Display;
using Xunit;

namespace MutaliskGH.Tests
{
    public class DisplayLogicTests
    {
        [Fact]
        public void PaletteEngine_Generate_IsDeterministicForSameSeed()
        {
            Result<PaletteEngineResult> first = PaletteEngineLogic.Generate(
                new object[] { "A", "B", "A" },
                0.65,
                7,
                false,
                0.42,
                0.18);

            Result<PaletteEngineResult> second = PaletteEngineLogic.Generate(
                new object[] { "A", "B", "A" },
                0.65,
                7,
                false,
                0.42,
                0.18);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);
            Assert.Equal(first.Value.OutputColors[0].Red, second.Value.OutputColors[0].Red);
            Assert.Equal(first.Value.OutputColors[1].Green, second.Value.OutputColors[1].Green);
            Assert.Equal(first.Value.OutputColors[2].Blue, second.Value.OutputColors[2].Blue);
        }

        [Fact]
        public void PaletteEngine_Generate_ChangesWithDifferentSeed()
        {
            Result<PaletteEngineResult> first = PaletteEngineLogic.Generate(
                new object[] { "A", "B" },
                0.65,
                7,
                false,
                0.42,
                0.18);

            Result<PaletteEngineResult> second = PaletteEngineLogic.Generate(
                new object[] { "A", "B" },
                0.65,
                8,
                false,
                0.42,
                0.18);

            Assert.True(first.IsSuccess);
            Assert.True(second.IsSuccess);
            bool anyChannelDiffers =
                first.Value.OutputColors[0].Red != second.Value.OutputColors[0].Red ||
                first.Value.OutputColors[0].Green != second.Value.OutputColors[0].Green ||
                first.Value.OutputColors[0].Blue != second.Value.OutputColors[0].Blue;
            Assert.True(anyChannelDiffers);
        }

        [Fact]
        public void PreviewColorByValue_KeepsAllItemsAndBuildsDistinctSet()
        {
            Result<PreviewColorByValueResult> result = PreviewColorByValueLogic.Evaluate(
                new object[] { "G1", "G2", "G3", "G4" },
                new object[] { "A", "", "B", "A" },
                7,
                false);

            Assert.True(result.IsSuccess);
            Assert.Equal(new object[] { "G1", "G2", "G3", "G4" }, result.Value.FilteredGeometry);
            Assert.Equal(new object[] { "A", "", "B", "A" }, result.Value.FilteredValues);
            Assert.Equal(new object[] { "A", "", "B" }, result.Value.DistinctValues);
            Assert.Equal(3, result.Value.BranchColors.Count);
            Assert.Equal(3, result.Value.MatchPatterns.Count);
            Assert.Equal(new[] { true, false, false, true }, result.Value.MatchPatterns[0]);
            Assert.Equal(new[] { false, true, false, false }, result.Value.MatchPatterns[1]);
            Assert.Equal(new[] { false, false, true, false }, result.Value.MatchPatterns[2]);
        }

        [Fact]
        public void PreviewColorByValue_GradientMode_UsesDistinctValueOrder()
        {
            Result<PreviewColorByValueResult> result = PreviewColorByValueLogic.Evaluate(
                new object[] { "G1", "G2", "G3" },
                new object[] { "A", "B", "A" },
                null,
                true);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.DistinctValues.Count);
            Assert.Equal(2, result.Value.BranchColors.Count);
            Assert.Equal(result.Value.MatchPatterns[0], new[] { true, false, true });
            Assert.Equal(result.Value.MatchPatterns[1], new[] { false, true, false });
        }

        [Fact]
        public void PreviewColorByValue_KeepsFalseLikeTextAndZeroValues()
        {
            Result<PreviewColorByValueResult> result = PreviewColorByValueLogic.Evaluate(
                new object[] { "G1", "G2", "G3", "G4", "G5" },
                new object[] { "A", "False", 0, "B", "A" },
                17,
                false);

            Assert.True(result.IsSuccess);
            Assert.Equal(new object[] { "G1", "G2", "G3", "G4", "G5" }, result.Value.FilteredGeometry);
            Assert.Equal(new object[] { "A", "False", 0, "B", "A" }, result.Value.FilteredValues);
            Assert.Equal(new object[] { "A", "False", 0, "B" }, result.Value.DistinctValues);
        }
    }
}
