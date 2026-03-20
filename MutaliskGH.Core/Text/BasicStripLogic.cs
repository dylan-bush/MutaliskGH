using System;

namespace MutaliskGH.Core.Text
{
    public static class BasicStripLogic
    {
        public static Result<string> Strip(string input, string charactersToStrip)
        {
            if (input == null)
            {
                return Result<string>.Success(null);
            }

            try
            {
                if (string.IsNullOrEmpty(charactersToStrip))
                {
                    return Result<string>.Success(input.Trim());
                }

                return Result<string>.Success(input.Trim(charactersToStrip.ToCharArray()));
            }
            catch (Exception exception)
            {
                return Result<string>.Failure("Failed to strip text: " + exception.Message);
            }
        }
    }
}
