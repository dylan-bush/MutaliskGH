using System;
using System.Collections.Generic;
using System.Linq;

namespace MutaliskGH.Core.Geometry
{
    public static class RectangleRebuildLogic
    {
        public static Result<RectangleRebuildResult> Create(IReadOnlyList<Point3Value> localCorners, double tolerance)
        {
            if (localCorners == null || localCorners.Count != 4)
            {
                return Result<RectangleRebuildResult>.Failure("Exactly four rectangle corners are required.");
            }

            List<Point3Value> orderedCorners = OrderCorners(localCorners);

            double width = Distance(orderedCorners[0], orderedCorners[3]);
            double height = Distance(orderedCorners[0], orderedCorners[1]);

            if (width <= tolerance)
            {
                return Result<RectangleRebuildResult>.Failure("A rectangle width greater than tolerance is required.");
            }

            if (height <= tolerance)
            {
                return Result<RectangleRebuildResult>.Failure("A rectangle height greater than tolerance is required.");
            }

            return Result<RectangleRebuildResult>.Success(
                new RectangleRebuildResult(width, height, orderedCorners));
        }

        private static List<Point3Value> OrderCorners(IReadOnlyList<Point3Value> corners)
        {
            Point3Value[] orderedByYThenX = corners
                .OrderBy(point => point.Y)
                .ThenBy(point => point.X)
                .ToArray();

            Point3Value[] bottomPair = orderedByYThenX
                .Take(2)
                .OrderBy(point => point.X)
                .ToArray();

            Point3Value[] topPair = orderedByYThenX
                .Skip(2)
                .OrderBy(point => point.X)
                .ToArray();

            Point3Value bottomLeft = bottomPair[0];
            Point3Value bottomRight = bottomPair[1];
            Point3Value topLeft = topPair[0];
            Point3Value topRight = topPair[1];

            return new List<Point3Value>
            {
                bottomLeft,
                topLeft,
                topRight,
                bottomRight
            };
        }

        private static double Distance(Point3Value a, Point3Value b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double dz = b.Z - a.Z;
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }
    }
}
