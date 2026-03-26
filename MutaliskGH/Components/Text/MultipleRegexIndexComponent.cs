using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Text
{
    public sealed class MultipleRegexIndexComponent : BaseComponent
    {
        public MultipleRegexIndexComponent()
            : base(
                "Multiple RegEx Index",
                "MRI",
                "Evaluate one text value against multiple regex patterns and return the matching indexes.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("8f07d1ca-e6cf-49eb-b40d-20a3545f2ca4"); }
        }

        protected override string IconResourceName
        {
            get { return "MultipleRegexIndex.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "T",
                "String value to evaluate against patterns.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "RegEx",
                "Re",
                "RegEx patterns to test.",
                GH_ParamAccess.list);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "First",
                "F",
                "Index of the first matched pattern.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "All",
                "A",
                "Indexes of all matched patterns.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string text = null;
            if (!dataAccess.GetData(0, ref text))
            {
                return;
            }

            List<string> patterns = new List<string>();
            if (!dataAccess.GetDataList(1, patterns))
            {
                return;
            }

            Result<MultipleRegexIndexResult> result = MultipleRegexIndexLogic.FindMatches(text, patterns);
            if (ReportFailure(result))
            {
                return;
            }

            if (result.Value.FirstMatchIndex.HasValue)
            {
                dataAccess.SetData(0, result.Value.FirstMatchIndex.Value);
            }

            dataAccess.SetDataList(1, result.Value.AllMatchIndexes);
        }
    }
}
