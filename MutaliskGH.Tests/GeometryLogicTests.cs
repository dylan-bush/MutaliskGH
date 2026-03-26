using MutaliskGH.Core;
using MutaliskGH.Core.Geometry;
using System.Collections.Generic;
using Xunit;

namespace MutaliskGH.Tests
{
    public class GeometryLogicTests
    {
        [Fact]
        public void PointRoundingLogic_RoundsPositiveAndNegativeCoordinates()
        {
            Result<Point3Value> result = PointRoundingLogic.Round(new Point3Value(1.24, -1.26, 0.5), 0.25);

            Assert.True(result.IsSuccess);
            Assert.Equal(1.25, result.Value.X, 6);
            Assert.Equal(-1.25, result.Value.Y, 6);
            Assert.Equal(0.5, result.Value.Z, 6);
        }

        [Fact]
        public void RectangleRebuildLogic_OrdersExistingCornersStartingAtBottomLeftAndMovingClockwise()
        {
            Result<RectangleRebuildResult> result = RectangleRebuildLogic.Create(
                new List<Point3Value>
                {
                    new Point3Value(5.0, 2.5, 0.0),
                    new Point3Value(-5.0, -2.5, 0.0),
                    new Point3Value(-5.0, 2.5, 0.0),
                    new Point3Value(5.0, -2.5, 0.0)
                },
                0.001);

            Assert.True(result.IsSuccess);
            Assert.Equal(4, result.Value.Corners.Count);
            Assert.Equal(-5.0, result.Value.Corners[0].X, 6);
            Assert.Equal(-2.5, result.Value.Corners[0].Y, 6);
            Assert.Equal(-5.0, result.Value.Corners[1].X, 6);
            Assert.Equal(2.5, result.Value.Corners[1].Y, 6);
            Assert.Equal(5.0, result.Value.Corners[2].X, 6);
            Assert.Equal(2.5, result.Value.Corners[2].Y, 6);
            Assert.Equal(5.0, result.Value.Corners[3].X, 6);
            Assert.Equal(-2.5, result.Value.Corners[3].Y, 6);
            Assert.Equal(10.0, result.Value.Width, 6);
            Assert.Equal(5.0, result.Value.Height, 6);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_AccumulateDirection_FlipsOpposingVectors()
        {
            Result<Vector3Value> result = OrientedBoundingBoxLogic.AccumulateDirection(
                new List<Vector3Value>
                {
                    new Vector3Value(1.0, 0.0, 0.0),
                    new Vector3Value(-1.0, 0.0, 0.0),
                    new Vector3Value(2.0, 0.0, 0.0)
                });

            Assert.True(result.IsSuccess);
            Assert.Equal(1.0, result.Value.X, 6);
            Assert.Equal(0.0, result.Value.Y, 6);
            Assert.Equal(0.0, result.Value.Z, 6);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_AccumulateWeightedDirection_FavorsLongerEdges()
        {
            Result<Vector3Value> result = OrientedBoundingBoxLogic.AccumulateWeightedDirection(
                new List<Vector3Value>
                {
                    new Vector3Value(1.0, 0.0, 0.0),
                    new Vector3Value(0.0, 4.0, 0.0)
                });

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.Y > result.Value.X);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_SelectPrimaryDirection_ClusteredMode_IgnoresSingleDiagonalOutlier()
        {
            Result<Vector3Value> result = OrientedBoundingBoxLogic.SelectPrimaryDirection(
                new List<Vector3Value>
                {
                    new Vector3Value(10.0, 0.0, 0.0),
                    new Vector3Value(-12.0, 0.0, 0.0),
                    new Vector3Value(8.0, 0.2, 0.0),
                    new Vector3Value(0.0, 0.0, 30.0)
                },
                0);

            Assert.True(result.IsSuccess);
            Assert.True(System.Math.Abs(result.Value.X) > 0.9);
            Assert.True(System.Math.Abs(result.Value.Z) < 0.2);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_CreateBasis_ClusteredMode_UsesSecondaryClusterWhenAvailable()
        {
            Result<OrientedBasisValue> result = OrientedBoundingBoxLogic.CreateBasis(
                new List<Vector3Value>
                {
                    new Vector3Value(10.0, 0.0, 0.0),
                    new Vector3Value(-8.0, 0.0, 0.0),
                    new Vector3Value(0.0, 6.0, 0.0),
                    new Vector3Value(0.0, -5.0, 0.0)
                },
                0);

            Assert.True(result.IsSuccess);
            Assert.True(System.Math.Abs(result.Value.XAxis.X) > 0.9);
            Assert.True(System.Math.Abs(result.Value.YAxis.Y) > 0.9);
            Assert.Equal(0.0, result.Value.ZAxis.X, 6);
            Assert.Equal(0.0, result.Value.ZAxis.Y, 6);
            Assert.True(System.Math.Abs(result.Value.ZAxis.Z) > 0.9);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_CreateBasis_UsesFallbackUpForVerticalDirection()
        {
            Result<OrientedBasisValue> result = OrientedBoundingBoxLogic.CreateBasis(new Vector3Value(0.0, 0.0, 5.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(0.0, result.Value.XAxis.X, 6);
            Assert.Equal(0.0, result.Value.XAxis.Y, 6);
            Assert.Equal(1.0, result.Value.XAxis.Z, 6);
            Assert.Equal(0.0, result.Value.YAxis.Y, 6);
            Assert.Equal(1.0, System.Math.Abs(result.Value.YAxis.X), 6);
        }

        [Fact]
        public void OrientedBoundingBoxLogic_SelectPrimaryDirection_MeanMethod_MatchesMeanPath()
        {
            Result<Vector3Value> result = OrientedBoundingBoxLogic.SelectPrimaryDirection(
                new List<Vector3Value>
                {
                    new Vector3Value(1.0, 0.0, 0.0),
                    new Vector3Value(-1.0, 0.0, 0.0),
                    new Vector3Value(2.0, 0.0, 0.0)
                },
                1);

            Assert.True(result.IsSuccess);
            Assert.Equal(1.0, result.Value.X, 6);
        }

        [Fact]
        public void OffsetSelectionLogic_LargerMode_SelectsHigherMetricCandidate()
        {
            Result<OffsetSelectionResult> result = OffsetSelectionLogic.Select(
                1,
                new OffsetCandidateValue(true, 20.0, 10.0),
                new OffsetCandidateValue(true, 12.0, 9.0));

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value.OrderedSides);
            Assert.Equal(1, result.Value.OrderedSides[0]);
        }

        [Fact]
        public void OffsetSelectionLogic_BothMode_OrdersSmallerThenLarger()
        {
            Result<OffsetSelectionResult> result = OffsetSelectionLogic.Select(
                2,
                new OffsetCandidateValue(true, 5.0, 10.0),
                new OffsetCandidateValue(true, 15.0, 9.0));

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.OrderedSides.Count);
            Assert.Equal(1, result.Value.OrderedSides[0]);
            Assert.Equal(-1, result.Value.OrderedSides[1]);
        }

        [Fact]
        public void CurveTrimSelectionLogic_WithNoIntersections_KeepsOriginalSpan()
        {
            Result<CurveTrimSelectionResult> result = CurveTrimSelectionLogic.Select(
                -1.0,
                11.0,
                0.0,
                10.0,
                new double[0],
                0.001,
                false);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.ShouldTrim);
            Assert.Equal(-1.0, result.Value.StartParameter, 6);
            Assert.Equal(11.0, result.Value.EndParameter, 6);
        }

        [Fact]
        public void CurveTrimSelectionLogic_WithSingleOuterIntersection_ExtendsOneSideOnly()
        {
            Result<CurveTrimSelectionResult> result = CurveTrimSelectionLogic.Select(
                0.5,
                10.5,
                2.0,
                8.0,
                new[] { 10.5 },
                0.001,
                false);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.ShouldTrim);
            Assert.Equal(0.5, result.Value.StartParameter, 6);
            Assert.Equal(10.5, result.Value.EndParameter, 6);
        }

        [Fact]
        public void CurveTrimSelectionLogic_WithMultipleIntersections_UsesOuterSpan()
        {
            Result<CurveTrimSelectionResult> result = CurveTrimSelectionLogic.Select(
                0.5,
                9.5,
                2.0,
                8.0,
                new[] { 1.25, 8.75, 5.0 },
                0.001,
                false);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.ShouldTrim);
            Assert.Equal(1.25, result.Value.StartParameter, 6);
            Assert.Equal(8.75, result.Value.EndParameter, 6);
        }

        [Fact]
        public void CurveTrimSelectionLogic_WithSingleInteriorIntersection_CanKeepCurveUntrimmed()
        {
            Result<CurveTrimSelectionResult> result = CurveTrimSelectionLogic.Select(
                0.5,
                9.5,
                2.0,
                8.0,
                new[] { 4.0 },
                0.001,
                false);

            Assert.True(result.IsSuccess);
            Assert.False(result.Value.ShouldTrim);
            Assert.Equal(0.5, result.Value.StartParameter, 6);
            Assert.Equal(9.5, result.Value.EndParameter, 6);
        }

        [Fact]
        public void CurveTrimSelectionLogic_WithSingleInteriorIntersection_CanTrimToLongerSide()
        {
            Result<CurveTrimSelectionResult> result = CurveTrimSelectionLogic.Select(
                0.5,
                9.5,
                2.0,
                8.0,
                new[] { 4.0 },
                0.001,
                true);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.ShouldTrim);
            Assert.Equal(4.0, result.Value.StartParameter, 6);
            Assert.Equal(9.5, result.Value.EndParameter, 6);
        }
    }
}
