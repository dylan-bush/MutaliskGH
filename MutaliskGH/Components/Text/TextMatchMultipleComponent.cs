using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Text
{
    public sealed class TextMatchMultipleComponent : BaseComponent
    {
        public TextMatchMultipleComponent()
            : base(
                "Text Match Multiple",
                "TMM",
                "Test one text value against multiple queries using raw regex or exact literal matching.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("3f0a2a7d-51af-45f4-8f51-5247664ff25e"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "T",
                "Text value to test.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Query",
                "Q",
                "Query strings to evaluate as raw regex or exact literal matches.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Rule",
                "R",
                "0 = raw regex search, 1 = exact literal match.",
                GH_ParamAccess.item,
                0);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddBooleanParameter(
                "Bool",
                "B",
                "Boolean match result for each query.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Count",
                "R",
                "Total number of matches.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "True",
                "T",
                "Indexes of matching queries.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "False",
                "F",
                "Indexes of non-matching queries.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string text = null;
            if (!dataAccess.GetData(0, ref text))
            {
                return;
            }

            List<string> queries = new List<string>();
            if (!dataAccess.GetDataList(1, queries))
            {
                return;
            }

            int rule = 0;
            dataAccess.GetData(2, ref rule);

            Result<TextMatchMultipleResult> result = TextMatchMultipleLogic.Evaluate(
                text,
                queries,
                (RegexQueryMode)rule);

            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetDataList(0, result.Value.Matches);
            dataAccess.SetData(1, result.Value.MatchCount);
            dataAccess.SetDataList(2, result.Value.MatchingIndexes);
            dataAccess.SetDataList(3, result.Value.NonMatchingIndexes);
        }
    }
}
