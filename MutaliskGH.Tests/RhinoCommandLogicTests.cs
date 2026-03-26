using MutaliskGH.Core.Rhino;
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
    }
}
