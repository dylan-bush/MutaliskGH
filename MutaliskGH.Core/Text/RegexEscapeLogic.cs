using System;
using System.Text.RegularExpressions;

namespace MutaliskGH.Core.Text
{
    public static class RegexEscapeLogic
    {
        public static Result<string> EscapeLiteral(string input)
        {
            if (input == null)
            {
                return Result<string>.Success(null);
            }

            try
            {
                return Result<string>.Success(Regex.Escape(input));
            }
            catch (Exception exception)
            {
                return Result<string>.Failure("Failed to escape regex characters: " + exception.Message);
            }
        }
    }
}
