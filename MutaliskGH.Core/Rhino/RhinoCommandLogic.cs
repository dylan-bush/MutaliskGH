using System.Collections.Generic;
using System.Linq;

namespace MutaliskGH.Core.Rhino
{
    public static class RhinoCommandLogic
    {
        public static Result<string> BuildSelValueCommand(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<string>.Failure("A non-empty value string is required.");
            }

            string escaped = value.Replace("\"", "\"\"");
            return Result<string>.Success("_-SelValue \"" + escaped + "\"");
        }

        public static Result<IReadOnlyList<string>> BuildSelValueCommands(IEnumerable<string> values)
        {
            if (values == null)
            {
                return Result<IReadOnlyList<string>>.Failure("At least one non-empty value string is required.");
            }

            var commands = new List<string>();
            foreach (string value in values)
            {
                Result<string> commandResult = BuildSelValueCommand(value);
                if (!commandResult.IsSuccess)
                {
                    continue;
                }

                commands.Add(commandResult.Value);
            }

            if (commands.Count == 0)
            {
                return Result<IReadOnlyList<string>>.Failure("At least one non-empty value string is required.");
            }

            return Result<IReadOnlyList<string>>.Success(commands.ToArray());
        }
    }
}
