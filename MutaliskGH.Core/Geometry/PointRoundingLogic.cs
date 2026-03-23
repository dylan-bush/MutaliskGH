using System;

namespace MutaliskGH.Core.Geometry
{
    public static class PointRoundingLogic
    {
        public static Result<Point3Value> Round(Point3Value point, double increment)
        {
            if (point == null)
            {
                return Result<Point3Value>.Failure("A point is required.");
            }

            if (increment <= 0.0 || double.IsNaN(increment) || double.IsInfinity(increment))
            {
                return Result<Point3Value>.Failure("A rounding increment greater than zero is required.");
            }

            return Result<Point3Value>.Success(
                new Point3Value(
                    RoundToIncrement(point.X, increment),
                    RoundToIncrement(point.Y, increment),
                    RoundToIncrement(point.Z, increment)));
        }

        private static double RoundToIncrement(double value, double increment)
        {
            return Math.Round(value / increment, MidpointRounding.AwayFromZero) * increment;
        }
    }
}
