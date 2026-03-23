using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Geometry
{
    public static class OrientedBoundingBoxLogic
    {
        private const int ClusteredMethod = 0;
        private const int MeanMethod = 1;
        private const int LengthWeightedMeanMethod = 2;
        private const double ZeroTolerance = 1e-9;
        private const double ClusterAngularToleranceDegrees = 10.0;
        private const double OrthogonalToleranceDegrees = 20.0;

        public static Result<Vector3Value> SelectPrimaryDirection(IEnumerable<Vector3Value> directions, int method)
        {
            switch (method)
            {
                case ClusteredMethod:
                    return SelectClusteredPrimaryDirection(directions);
                case MeanMethod:
                    return AccumulateDirection(directions);
                case LengthWeightedMeanMethod:
                    return AccumulateWeightedDirection(directions);
                default:
                    return Result<Vector3Value>.Failure("Method must be 0 for clustered, 1 for mean, or 2 for length-weighted mean.");
            }
        }

        public static Result<OrientedBasisValue> CreateBasis(IEnumerable<Vector3Value> directions, int method)
        {
            if (method == ClusteredMethod)
            {
                return CreateClusteredBasis(directions);
            }

            Result<Vector3Value> primaryDirection = SelectPrimaryDirection(directions, method);
            if (primaryDirection.IsFailure)
            {
                return Result<OrientedBasisValue>.Failure(primaryDirection.ErrorMessage);
            }

            return CreateBasis(primaryDirection.Value);
        }

        public static Result<Vector3Value> AccumulateDirection(IEnumerable<Vector3Value> directions)
        {
            if (directions == null)
            {
                return Result<Vector3Value>.Failure("At least one direction vector is required.");
            }

            bool hasDirection = false;
            double sumX = 0.0;
            double sumY = 0.0;
            double sumZ = 0.0;

            foreach (Vector3Value direction in directions)
            {
                if (direction == null)
                {
                    continue;
                }

                double length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y) + (direction.Z * direction.Z));
                if (length <= ZeroTolerance)
                {
                    continue;
                }

                double x = direction.X / length;
                double y = direction.Y / length;
                double z = direction.Z / length;

                if (hasDirection)
                {
                    double dot = (sumX * x) + (sumY * y) + (sumZ * z);
                    if (dot < 0.0)
                    {
                        x = -x;
                        y = -y;
                        z = -z;
                    }
                }

                sumX += x;
                sumY += y;
                sumZ += z;
                hasDirection = true;
            }

            if (!hasDirection)
            {
                return Result<Vector3Value>.Failure("At least one non-zero direction vector is required.");
            }

            return Result<Vector3Value>.Success(Normalize(sumX, sumY, sumZ));
        }

        public static Result<Vector3Value> AccumulateWeightedDirection(IEnumerable<Vector3Value> directions)
        {
            if (directions == null)
            {
                return Result<Vector3Value>.Failure("At least one direction vector is required.");
            }

            bool hasDirection = false;
            double sumX = 0.0;
            double sumY = 0.0;
            double sumZ = 0.0;

            foreach (Vector3Value direction in directions)
            {
                if (direction == null)
                {
                    continue;
                }

                double length = Math.Sqrt((direction.X * direction.X) + (direction.Y * direction.Y) + (direction.Z * direction.Z));
                if (length <= ZeroTolerance)
                {
                    continue;
                }

                double x = direction.X / length;
                double y = direction.Y / length;
                double z = direction.Z / length;

                if (hasDirection)
                {
                    double dot = (sumX * x) + (sumY * y) + (sumZ * z);
                    if (dot < 0.0)
                    {
                        x = -x;
                        y = -y;
                        z = -z;
                    }
                }

                sumX += x * length;
                sumY += y * length;
                sumZ += z * length;
                hasDirection = true;
            }

            if (!hasDirection)
            {
                return Result<Vector3Value>.Failure("At least one non-zero direction vector is required.");
            }

            return Result<Vector3Value>.Success(Normalize(sumX, sumY, sumZ));
        }

        public static Result<OrientedBasisValue> CreateBasis(Vector3Value xDirection)
        {
            if (xDirection == null)
            {
                return Result<OrientedBasisValue>.Failure("A primary box direction is required.");
            }

            Vector3Value xAxis = Normalize(xDirection.X, xDirection.Y, xDirection.Z);
            if (IsZero(xAxis))
            {
                return Result<OrientedBasisValue>.Failure("A non-zero primary box direction is required.");
            }

            Vector3Value worldZ = new Vector3Value(0.0, 0.0, 1.0);
            Vector3Value worldY = new Vector3Value(0.0, 1.0, 0.0);

            Vector3Value upSeed = Math.Abs(Dot(xAxis, worldZ)) > 0.98 ? worldY : worldZ;
            Vector3Value yAxis = Normalize(Cross(upSeed, xAxis));
            if (IsZero(yAxis))
            {
                return Result<OrientedBasisValue>.Failure("Unable to derive an oriented box basis.");
            }

            Vector3Value zAxis = Normalize(Cross(xAxis, yAxis));
            return Result<OrientedBasisValue>.Success(new OrientedBasisValue(xAxis, yAxis, zAxis));
        }

        private static Result<Vector3Value> SelectClusteredPrimaryDirection(IEnumerable<Vector3Value> directions)
        {
            List<DirectionCluster> clusters = BuildClusters(directions);
            if (clusters.Count == 0)
            {
                return Result<Vector3Value>.Failure("At least one non-zero direction vector is required.");
            }

            DirectionCluster primaryCluster = clusters[0];
            for (int index = 1; index < clusters.Count; index++)
            {
                if (clusters[index].Score > primaryCluster.Score)
                {
                    primaryCluster = clusters[index];
                }
            }

            return Result<Vector3Value>.Success(primaryCluster.Direction);
        }

        private static Result<OrientedBasisValue> CreateClusteredBasis(IEnumerable<Vector3Value> directions)
        {
            List<DirectionCluster> clusters = BuildClusters(directions);
            if (clusters.Count == 0)
            {
                return Result<OrientedBasisValue>.Failure("At least one non-zero direction vector is required.");
            }

            DirectionCluster primaryCluster = clusters[0];
            for (int index = 1; index < clusters.Count; index++)
            {
                if (clusters[index].Score > primaryCluster.Score)
                {
                    primaryCluster = clusters[index];
                }
            }

            double orthogonalDotTolerance = Math.Cos(ToRadians(90.0 - OrthogonalToleranceDegrees));
            DirectionCluster secondaryCluster = null;
            for (int index = 0; index < clusters.Count; index++)
            {
                DirectionCluster candidate = clusters[index];
                if (ReferenceEquals(candidate, primaryCluster))
                {
                    continue;
                }

                double dot = Math.Abs(Dot(primaryCluster.Direction, candidate.Direction));
                if (dot <= orthogonalDotTolerance)
                {
                    if (secondaryCluster == null || candidate.Score > secondaryCluster.Score)
                    {
                        secondaryCluster = candidate;
                    }
                }
            }

            if (secondaryCluster == null)
            {
                return CreateBasis(primaryCluster.Direction);
            }

            Vector3Value xAxis = Normalize(primaryCluster.Direction);
            Vector3Value projectedSecondary = ProjectPerpendicular(secondaryCluster.Direction, xAxis);
            Vector3Value yAxis = Normalize(projectedSecondary);
            if (IsZero(yAxis))
            {
                return CreateBasis(primaryCluster.Direction);
            }

            Vector3Value zAxis = Normalize(Cross(xAxis, yAxis));
            if (IsZero(zAxis))
            {
                return CreateBasis(primaryCluster.Direction);
            }

            return Result<OrientedBasisValue>.Success(new OrientedBasisValue(xAxis, yAxis, zAxis));
        }

        private static List<DirectionCluster> BuildClusters(IEnumerable<Vector3Value> directions)
        {
            List<DirectionCluster> clusters = new List<DirectionCluster>();
            if (directions == null)
            {
                return clusters;
            }

            double minimumDot = Math.Cos(ToRadians(ClusterAngularToleranceDegrees));

            foreach (Vector3Value direction in directions)
            {
                if (direction == null)
                {
                    continue;
                }

                double length = Length(direction);
                if (length <= ZeroTolerance)
                {
                    continue;
                }

                Vector3Value normalized = Normalize(direction);
                DirectionCluster bestCluster = null;
                double bestDot = double.MinValue;

                for (int index = 0; index < clusters.Count; index++)
                {
                    DirectionCluster cluster = clusters[index];
                    double dot = Dot(cluster.Direction, normalized);
                    double absDot = Math.Abs(dot);
                    if (absDot >= minimumDot && absDot > bestDot)
                    {
                        bestDot = absDot;
                        bestCluster = cluster;
                    }
                }

                if (bestCluster == null)
                {
                    clusters.Add(new DirectionCluster(normalized, length));
                    continue;
                }

                if (Dot(bestCluster.Direction, normalized) < 0.0)
                {
                    normalized = Scale(normalized, -1.0);
                }

                bestCluster.Add(normalized, length);
            }

            return clusters;
        }

        private static bool IsZero(Vector3Value vector)
        {
            return Math.Abs(vector.X) <= ZeroTolerance
                && Math.Abs(vector.Y) <= ZeroTolerance
                && Math.Abs(vector.Z) <= ZeroTolerance;
        }

        private static double Dot(Vector3Value a, Vector3Value b)
        {
            return (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);
        }

        private static double Length(Vector3Value vector)
        {
            return Math.Sqrt((vector.X * vector.X) + (vector.Y * vector.Y) + (vector.Z * vector.Z));
        }

        private static Vector3Value Cross(Vector3Value a, Vector3Value b)
        {
            return new Vector3Value(
                (a.Y * b.Z) - (a.Z * b.Y),
                (a.Z * b.X) - (a.X * b.Z),
                (a.X * b.Y) - (a.Y * b.X));
        }

        private static Vector3Value ProjectPerpendicular(Vector3Value vector, Vector3Value axis)
        {
            double dot = Dot(vector, axis);
            return new Vector3Value(
                vector.X - (dot * axis.X),
                vector.Y - (dot * axis.Y),
                vector.Z - (dot * axis.Z));
        }

        private static Vector3Value Scale(Vector3Value vector, double scalar)
        {
            return new Vector3Value(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
        }

        private static Vector3Value Normalize(double x, double y, double z)
        {
            double length = Math.Sqrt((x * x) + (y * y) + (z * z));
            if (length <= ZeroTolerance)
            {
                return new Vector3Value(0.0, 0.0, 0.0);
            }

            return new Vector3Value(x / length, y / length, z / length);
        }

        private static Vector3Value Normalize(Vector3Value vector)
        {
            return Normalize(vector.X, vector.Y, vector.Z);
        }

        private static double ToRadians(double degrees)
        {
            return degrees * (Math.PI / 180.0);
        }

        private sealed class DirectionCluster
        {
            private double sumX;
            private double sumY;
            private double sumZ;

            public DirectionCluster(Vector3Value seedDirection, double seedScore)
            {
                Add(seedDirection, seedScore);
            }

            public double Score { get; private set; }

            public Vector3Value Direction
            {
                get { return Normalize(sumX, sumY, sumZ); }
            }

            public void Add(Vector3Value direction, double score)
            {
                sumX += direction.X * score;
                sumY += direction.Y * score;
                sumZ += direction.Z * score;
                Score += score;
            }
        }
    }
}
