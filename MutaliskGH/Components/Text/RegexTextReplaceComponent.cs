using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Text
{
    public sealed class RegexTextReplaceComponent : BaseComponent
    {
        public RegexTextReplaceComponent()
            : base(
                "RegEx Text Replace",
                "RxRep",
                "Replace regex matches in a text string and report success, errors, and whether the text changed.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("fa76c7fd-f518-4284-a4ba-cd0ff1916d6f"); }
        }

        protected override string IconResourceName
        {
            get { return "RegexTextReplace.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "T",
                "Input text string to modify.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "RegEx",
                "Re",
                "RegEx pattern to match.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Replacement",
                "R",
                "Replacement text. Leave empty to remove matches.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Result",
                "T",
                "Modified text after regex replacement.",
                GH_ParamAccess.item);

            parameterManager.AddBooleanParameter(
                "Success",
                "S",
                "True when the replacement completed successfully.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Error",
                "E",
                "Error message when the operation fails.",
                GH_ParamAccess.item);

            parameterManager.AddBooleanParameter(
                "Changed",
                "C",
                "True when the output text differs from the input text.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string text = null;
            if (!dataAccess.GetData(0, ref text))
            {
                return;
            }

            string pattern = null;
            if (!dataAccess.GetData(1, ref pattern))
            {
                return;
            }

            string replacement = null;
            dataAccess.GetData(2, ref replacement);

            Result<RegexTextReplaceResult> result = RegexTextReplaceLogic.Replace(text, pattern, replacement);
            if (ReportFailure(result))
            {
                dataAccess.SetData(0, text);
                dataAccess.SetData(1, false);
                dataAccess.SetData(2, result.ErrorMessage);
                dataAccess.SetData(3, false);
                return;
            }

            dataAccess.SetData(0, result.Value.Text);
            dataAccess.SetData(1, result.Value.Success);
            dataAccess.SetData(2, result.Value.ErrorMessage);
            dataAccess.SetData(3, result.Value.WasChanged);
        }
    }
}
