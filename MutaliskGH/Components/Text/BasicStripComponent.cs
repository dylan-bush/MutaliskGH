using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Text
{
    public sealed class BasicStripComponent : BaseComponent
    {
        public BasicStripComponent()
            : base(
                "Basic Strip",
                "BStrip",
                "Trim whitespace or specified characters from both ends of a text string.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("d38c3697-2f75-4d32-a61f-267896285f2f"); }
        }

        protected override string IconResourceName
        {
            get { return "BasicStrip.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "T",
                "Text string to strip.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Strip",
                "S",
                "Optional characters to strip from both ends. Leave empty to trim whitespace.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Stripped",
                "R",
                "Trimmed text.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string text = null;
            if (!dataAccess.GetData(0, ref text))
            {
                return;
            }

            string characters = null;
            dataAccess.GetData(1, ref characters);

            Result<string> result = BasicStripLogic.Strip(text, characters);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetData(0, result.Value);
        }
    }
}
