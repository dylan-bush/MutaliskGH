using System.Globalization;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Format
{
    public static class PlaneSerializationLogic
    {
        private static readonly Regex PlanePattern = new Regex(
            "^\\s*O(?<origin>.*?)X(?<x>.*?)Y(?<y>.*?)Z(?<z>.*?)\\s*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public static Result<string> Serialize(PlaneValue plane)
        {
            if (plane == null)
            {
                return Result<string>.Failure("A plane value is required.");
            }

            if (plane.Origin == null || plane.XAxis == null || plane.YAxis == null || plane.ZAxis == null)
            {
                return Result<string>.Failure("Plane origin and axes are required.");
            }

            string serialized = string.Format(
                CultureInfo.InvariantCulture,
                "O{0}X{1}Y{2}Z{3}",
                FormatTriple(plane.Origin),
                FormatTriple(plane.XAxis),
                FormatTriple(plane.YAxis),
                FormatTriple(plane.ZAxis));

            return Result<string>.Success(serialized);
        }

        public static Result<PlaneValue> Deserialize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Result<PlaneValue>.Failure("Serialized plane text is required.");
            }

            Match match = PlanePattern.Match(text);
            if (!match.Success)
            {
                return Result<PlaneValue>.Failure("Serialized plane text must follow the format O...X...Y...Z....");
            }

            Result<Vector3Value> originResult = ParseTriple(match.Groups["origin"].Value, "origin");
            if (originResult.IsFailure)
            {
                return Result<PlaneValue>.Failure(originResult.ErrorMessage);
            }

            Result<Vector3Value> xAxisResult = ParseTriple(match.Groups["x"].Value, "X axis");
            if (xAxisResult.IsFailure)
            {
                return Result<PlaneValue>.Failure(xAxisResult.ErrorMessage);
            }

            Result<Vector3Value> yAxisResult = ParseTriple(match.Groups["y"].Value, "Y axis");
            if (yAxisResult.IsFailure)
            {
                return Result<PlaneValue>.Failure(yAxisResult.ErrorMessage);
            }

            Result<Vector3Value> zAxisResult = ParseTriple(match.Groups["z"].Value, "Z axis");
            if (zAxisResult.IsFailure)
            {
                return Result<PlaneValue>.Failure(zAxisResult.ErrorMessage);
            }

            return Result<PlaneValue>.Success(
                new PlaneValue(
                    originResult.Value,
                    xAxisResult.Value,
                    yAxisResult.Value,
                    zAxisResult.Value));
        }

        private static string FormatTriple(Vector3Value value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0},{1},{2}",
                FormatNumber(value.X),
                FormatNumber(value.Y),
                FormatNumber(value.Z));
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.###############", CultureInfo.InvariantCulture);
        }

        private static Result<Vector3Value> ParseTriple(string text, string label)
        {
            string normalized = (text ?? string.Empty).Trim();
            normalized = normalized.Trim('(', ')', '[', ']', '{', '}');
            normalized = normalized.Replace(";", ",");

            string[] commaParts = normalized.Split(',');
            if (commaParts.Length != 3)
            {
                string[] whitespaceParts = Regex.Split(normalized, "\\s+");
                if (whitespaceParts.Length == 3)
                {
                    commaParts = whitespaceParts;
                }
            }

            if (commaParts.Length != 3)
            {
                return Result<Vector3Value>.Failure(label + " must contain exactly three numeric values.");
            }

            double x;
            if (!double.TryParse(commaParts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out x))
            {
                return Result<Vector3Value>.Failure("Could not parse the " + label + " X value.");
            }

            double y;
            if (!double.TryParse(commaParts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out y))
            {
                return Result<Vector3Value>.Failure("Could not parse the " + label + " Y value.");
            }

            double z;
            if (!double.TryParse(commaParts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out z))
            {
                return Result<Vector3Value>.Failure("Could not parse the " + label + " Z value.");
            }

            return Result<Vector3Value>.Success(new Vector3Value(x, y, z));
        }
    }
}
