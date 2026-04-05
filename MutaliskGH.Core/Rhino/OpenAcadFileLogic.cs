using System;
using System.IO;

namespace MutaliskGH.Core.Rhino
{
    public static class OpenAcadFileLogic
    {
        public const string StatusNoOp = "NoOp";
        public const string StatusFileNotFound = "FileNotFound";
        public const string StatusInvalidType = "InvalidType";
        public const string StatusOpenedReadOnly = "OpenedReadOnly";
        public const string StatusAlreadyOpen = "AlreadyOpen";
        public const string StatusAutoCadUnavailable = "AutoCADUnavailable";
        public const string StatusOpenFailed = "OpenFailed";

        public static string NormalizePath(string pathValue)
        {
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                return null;
            }

            try
            {
                string expanded = Environment.ExpandEnvironmentVariables(pathValue.Trim());
                return Path.GetFullPath(expanded);
            }
            catch
            {
                return null;
            }
        }

        public static Result<string> ValidateDwgPath(string pathValue)
        {
            string normalized = NormalizePath(pathValue);
            if (string.IsNullOrWhiteSpace(normalized) || !File.Exists(normalized))
            {
                return Result<string>.Failure(StatusFileNotFound);
            }

            if (!normalized.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.Failure(StatusInvalidType);
            }

            return Result<string>.Success(normalized);
        }

        public static OpenAcadFileResult NoOp()
        {
            return new OpenAcadFileResult(StatusNoOp, false, false, "activate=False", null);
        }

        public static OpenAcadFileResult Error(string status, string details, string normalizedPath = null)
        {
            return new OpenAcadFileResult(status, false, false, details ?? string.Empty, normalizedPath);
        }

        public static OpenAcadFileResult Success(string status, bool usedExistingInstance, string details, string normalizedPath)
        {
            return new OpenAcadFileResult(status, true, usedExistingInstance, details ?? string.Empty, normalizedPath);
        }
    }
}
