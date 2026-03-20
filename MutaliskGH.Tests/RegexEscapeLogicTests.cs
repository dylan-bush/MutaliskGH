using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using Xunit;

namespace MutaliskGH.Tests
{
    public class RegexEscapeLogicTests
    {
        [Fact]
        public void EscapeLiteral_EscapesRegexMetacharacters()
        {
            var result = RegexEscapeLogic.EscapeLiteral(@"a+b(c)?[d]");

            Assert.True(result.IsSuccess);
            Assert.Equal(@"a\+b\(c\)\?\[d]", result.Value);
        }

        [Fact]
        public void BasicStrip_TrimsWhitespaceOrSpecifiedCharacters()
        {
            Result<string> whitespaceResult = BasicStripLogic.Strip("  alpha  ", null);
            Result<string> characterResult = BasicStripLogic.Strip("--alpha--", "-");

            Assert.True(whitespaceResult.IsSuccess);
            Assert.Equal("alpha", whitespaceResult.Value);

            Assert.True(characterResult.IsSuccess);
            Assert.Equal("alpha", characterResult.Value);
        }

        [Fact]
        public void TextMatchMultiple_EvaluatesRawAndExactModes()
        {
            Result<TextMatchMultipleResult> rawResult = TextMatchMultipleLogic.Evaluate(
                "alpha_beta",
                new[] { "alpha.*", "beta", "alpha_beta" },
                RegexQueryMode.Raw);

            Assert.True(rawResult.IsSuccess);
            Assert.Equal(new[] { true, true, true }, rawResult.Value.Matches);
            Assert.Equal(3, rawResult.Value.MatchCount);
            Assert.Equal(new[] { 0, 1, 2 }, rawResult.Value.MatchingIndexes);
            Assert.Empty(rawResult.Value.NonMatchingIndexes);

            Result<TextMatchMultipleResult> exactResult = TextMatchMultipleLogic.Evaluate(
                "alpha_beta",
                new[] { "alpha.*", "beta", "alpha_beta" },
                RegexQueryMode.Exact);

            Assert.True(exactResult.IsSuccess);
            Assert.Equal(new[] { false, false, true }, exactResult.Value.Matches);
            Assert.Equal(1, exactResult.Value.MatchCount);
            Assert.Equal(new[] { 2 }, exactResult.Value.MatchingIndexes);
            Assert.Equal(new[] { 0, 1 }, exactResult.Value.NonMatchingIndexes);
        }

        [Fact]
        public void MultipleRegexIndex_ReturnsFirstAndAllMatches()
        {
            Result<MultipleRegexIndexResult> result = MultipleRegexIndexLogic.FindMatches(
                "A-12",
                new[] { @"^\d+$", @"^[A-Z]-\d+$", @"12" });

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.FirstMatchIndex);
            Assert.Equal(new[] { 1, 2 }, result.Value.AllMatchIndexes);
        }

        [Fact]
        public void RegexTextReplace_ReplacesTextAndReportsChange()
        {
            Result<RegexTextReplaceResult> result = RegexTextReplaceLogic.Replace(
                "PNL-DAB-FBA01",
                "FBA\\d+",
                "XXX");

            Assert.True(result.IsSuccess);
            Assert.Equal("PNL-DAB-XXX", result.Value.Text);
            Assert.True(result.Value.Success);
            Assert.Equal(string.Empty, result.Value.ErrorMessage);
            Assert.True(result.Value.WasChanged);
        }

        [Fact]
        public void RegexTextReplace_RejectsEmptyPattern()
        {
            Result<RegexTextReplaceResult> result = RegexTextReplaceLogic.Replace(
                "alpha",
                string.Empty,
                "beta");

            Assert.True(result.IsFailure);
            Assert.Equal("Regex pattern cannot be empty.", result.ErrorMessage);
        }

        [Fact]
        public void RegexCull_ReturnMatchesFiltersParallelData()
        {
            Result<RegexCullResult<string>> result = RegexCullLogic.Filter(
                true,
                new[] { "alpha", "beta", "alphabet" },
                new[] { "alpha" },
                new[] { "A", "B", "AB" });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { true, false, true }, result.Value.MatchFlags);
            Assert.Equal(new[] { "A", "AB" }, result.Value.FilteredItems);
            Assert.Equal(new[] { "B" }, result.Value.CulledItems);
        }

        [Fact]
        public void RegexCull_CullMatchesFiltersOutMatchingItems()
        {
            Result<RegexCullResult<int>> result = RegexCullLogic.Filter(
                false,
                new[] { "alpha", "beta", "alphabet" },
                new[] { "alpha" },
                new[] { 10, 20, 30 });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { true, false, true }, result.Value.MatchFlags);
            Assert.Equal(new[] { 20 }, result.Value.FilteredItems);
            Assert.Equal(new[] { 10, 30 }, result.Value.CulledItems);
        }

        [Fact]
        public void RegexCull_MultiplePatternsUseOrLogic()
        {
            Result<RegexCullResult<string>> result = RegexCullLogic.Filter(
                true,
                new[] { "alpha", "beta", "gamma" },
                new[] { "alpha", "beta" },
                new[] { "A", "B", "G" });

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { true, true, false }, result.Value.MatchFlags);
            Assert.Equal(new[] { "A", "B" }, result.Value.FilteredItems);
            Assert.Equal(new[] { "G" }, result.Value.CulledItems);
        }
    }
}
