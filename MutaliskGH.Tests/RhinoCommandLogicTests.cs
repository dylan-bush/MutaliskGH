using MutaliskGH.Core.Rhino;
using System;
using System.Linq;
using Xunit;

namespace MutaliskGH.Tests
{
    public class RhinoCommandLogicTests
    {
        [Fact]
        public void BuildSelValueCommand_WrapsInputInQuotes()
        {
            var result = RhinoCommandLogic.BuildSelValueCommand("Panel-101");

            Assert.True(result.IsSuccess);
            Assert.Equal("_-SelValue \"Panel-101\"", result.Value);
        }

        [Fact]
        public void BuildSelValueCommand_EscapesEmbeddedQuotes()
        {
            var result = RhinoCommandLogic.BuildSelValueCommand("A\"B");

            Assert.True(result.IsSuccess);
            Assert.Equal("_-SelValue \"A\"\"B\"", result.Value);
        }

        [Fact]
        public void BuildSelValueCommands_ReturnsCommandPerNonEmptyValue()
        {
            var result = RhinoCommandLogic.BuildSelValueCommands(new[] { "042031", "", "042032" });

            Assert.True(result.IsSuccess);
            Assert.Equal(
                new[] { "_-SelValue \"042031\"", "_-SelValue \"042032\"" },
                result.Value.ToArray());
        }

        [Fact]
        public void BuildSelValueCommands_FailsWhenNoUsableValuesExist()
        {
            var result = RhinoCommandLogic.BuildSelValueCommands(new[] { "", "   " });

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void TryParseObjectId_AcceptsGuidAndStringValues()
        {
            Guid guid = Guid.NewGuid();

            var directResult = RhinoMetadataLogic.TryParseObjectId(guid);
            var stringResult = RhinoMetadataLogic.TryParseObjectId(guid.ToString());

            Assert.True(directResult.IsSuccess);
            Assert.Equal(guid, directResult.Value);
            Assert.True(stringResult.IsSuccess);
            Assert.Equal(guid, stringResult.Value);
        }

        [Fact]
        public void TryParseObjectId_FailsForUnsupportedValues()
        {
            var result = RhinoMetadataLogic.TryParseObjectId("not-a-guid");

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void NormalizeGroupNames_ReturnsUngroupedWhenNoValidNamesExist()
        {
            var emptyResult = RhinoMetadataLogic.NormalizeGroupNames(null);
            var invalidResult = RhinoMetadataLogic.NormalizeGroupNames(new[] { "", " " });

            Assert.Equal(new[] { "Ungrouped" }, emptyResult.ToArray());
            Assert.Equal(new[] { "Ungrouped" }, invalidResult.ToArray());
        }

        [Fact]
        public void NormalizeLayerPaths_RemovesBlankValuesAndPreservesOrder()
        {
            var result = RhinoMetadataLogic.NormalizeLayerPaths(new[] { "A", "", "B::C", " " });

            Assert.Equal(new[] { "A", "B::C" }, result.ToArray());
        }
    }
}
