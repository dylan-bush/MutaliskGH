using MutaliskGH.Core;
using MutaliskGH.Core.Format;
using Xunit;

namespace MutaliskGH.Tests
{
    public class FormatLogicTests
    {
        [Fact]
        public void SerializePlane_UsesOriginalOxyzPattern()
        {
            PlaneValue plane = new PlaneValue(
                new Vector3Value(0, 0, 0),
                new Vector3Value(1, 0, 0),
                new Vector3Value(0, 1, 0),
                new Vector3Value(0, 0, 1));

            Result<string> result = PlaneSerializationLogic.Serialize(plane);

            Assert.True(result.IsSuccess);
            Assert.Equal("O0,0,0X1,0,0Y0,1,0Z0,0,1", result.Value);
        }

        [Fact]
        public void DeserializePlane_AcceptsWrappedTriples()
        {
            Result<PlaneValue> result = PlaneSerializationLogic.Deserialize(
                "O(0, 0, 0)X(1, 0, 0)Y(0, 1, 0)Z(0, 0, 1)");

            Assert.True(result.IsSuccess);
            Assert.Equal(0, result.Value.Origin.X);
            Assert.Equal(0, result.Value.Origin.Y);
            Assert.Equal(0, result.Value.Origin.Z);
            Assert.Equal(1, result.Value.XAxis.X);
            Assert.Equal(0, result.Value.YAxis.X);
            Assert.Equal(1, result.Value.YAxis.Y);
            Assert.Equal(1, result.Value.ZAxis.Z);
        }

        [Fact]
        public void DecimalFeetInches_FormatsArchitecturalString()
        {
            Result<DecimalFeetInchesResult> result = DecimalFeetInchesLogic.Convert(15.125, 16, 0, true);

            Assert.True(result.IsSuccess);
            Assert.Equal("1'-3 1/8\"", result.Value.FormattedText);
            Assert.Equal(15.125, result.Value.RoundedInches, 6);
        }

        [Fact]
        public void DecimalFeetInches_RespectsRoundingModes()
        {
            Result<DecimalFeetInchesResult> roundedUp = DecimalFeetInchesLogic.Convert(14.1, 4, 1, true);
            Result<DecimalFeetInchesResult> roundedDown = DecimalFeetInchesLogic.Convert(14.1, 4, 2, true);

            Assert.True(roundedUp.IsSuccess);
            Assert.Equal("1'-2 1/4\"", roundedUp.Value.FormattedText);
            Assert.Equal(14.25, roundedUp.Value.RoundedInches, 6);

            Assert.True(roundedDown.IsSuccess);
            Assert.Equal("1'-2\"", roundedDown.Value.FormattedText);
            Assert.Equal(14.0, roundedDown.Value.RoundedInches, 6);
        }

        [Fact]
        public void NextAvailableCode_PreservesPrefixAndSearchesNearest()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "042069",
                new object[] { "042069", "042070", "042068", "043000" });

            Assert.True(result.IsSuccess);
            Assert.Equal("042071", result.Value);
        }

        [Fact]
        public void NextAvailableCode_ReturnsNullWhenPrefixIsFull()
        {
            object[] taken = new object[1000];
            for (int index = 0; index < taken.Length; index++)
            {
                taken[index] = "042" + index.ToString("000");
            }

            Result<string> result = NextAvailableCodeLogic.FindNext("042500", taken);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Value);
        }

        [Fact]
        public void NextAvailableCode_FormatWithFixedSlots_PreservesOriginalBehavior()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "042069",
                new object[] { "042069", "042070", "042068", "041071" },
                "{000###}");

            Assert.True(result.IsSuccess);
            Assert.Equal("042071", result.Value);
        }

        [Fact]
        public void NextAvailableCode_FormatWithOnlyZeroes_TreatsAllDigitsAsSearchable()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "042069",
                new object[] { "042069", "042070", "042068" },
                "{000000}");

            Assert.True(result.IsSuccess);
            Assert.Equal("042071", result.Value);
        }

        [Fact]
        public void NextAvailableCode_FormatWithLiteralWrapper_FormatsOutput()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "LVL-042069",
                new object[] { "LVL-042069", "LVL-042070", "BAD-042068" },
                "LVL-{000###}");

            Assert.True(result.IsSuccess);
            Assert.Equal("LVL-042068", result.Value);
        }

        [Fact]
        public void NextAvailableCode_FormatIgnoresTakenValuesThatDoNotMatchFixedSlots()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "042069",
                new object[] { "042069", "042070", "041068", "043070" },
                "{000###}");

            Assert.True(result.IsSuccess);
            Assert.Equal("042068", result.Value);
        }

        [Fact]
        public void NextAvailableCode_FormatRejectsMultipleSearchSpans()
        {
            Result<string> result = NextAvailableCodeLogic.FindNext(
                "042069",
                new object[] { "042069" },
                "{0#0#00}");

            Assert.True(result.IsFailure);
            Assert.Equal("Searchable # slots must form one contiguous span.", result.ErrorMessage);
        }
    }
}
