using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Text
{
    public sealed class RegexEscapeComponent : BaseComponent
    {
        public RegexEscapeComponent()
            : base(
                "RegEx Escape",
                "RxEsc",
                "Escape regex metacharacters in a text string for safe literal use in regular expressions.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("bbca6909-2d12-4516-afbe-d521dfe5f213"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "T",
                "Text string to escape for safe literal regex use.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Escaped",
                "E",
                "Escaped string with regex metacharacters prefixed with backslashes.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string input = null;
            if (!dataAccess.GetData(0, ref input))
            {
                return;
            }

            Result<string> result = RegexEscapeLogic.EscapeLiteral(input);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetData(0, result.Value);
        }
    }
}
