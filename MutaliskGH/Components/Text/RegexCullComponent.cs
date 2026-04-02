using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using MutaliskGH.Core;
using MutaliskGH.Core.Text;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Text
{
    public sealed class RegexCullComponent : BaseComponent, IGH_VariableParameterComponent
    {
        private const int FixedInputCount = 4;
        private const int FixedOutputCount = 4;

        public RegexCullComponent()
            : base(
                "RegEx Cull",
                "RxCull",
                "Cull or keep parallel data by testing a list of strings against regex patterns. Add more data inputs with ZUI for parallel streams.",
                CategoryNames.Text)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("8d5666cf-ff30-49f9-b9c0-c881971309f4"); }
        }

        protected override string IconResourceName
        {
            get { return "RegexCull.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddBooleanParameter(
                "Return Matches",
                "B",
                "True = return matching items, False = cull matching items.",
                GH_ParamAccess.item,
                true);

            parameterManager.AddTextParameter(
                "Test",
                "L",
                "List of text values to test against the regex pattern.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "RegEx",
                "Re",
                "RegEx patterns. If multiple patterns are provided, any match counts as true.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Data",
                "||",
                "Primary parallel data list to filter.",
                GH_ParamAccess.list);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
            parameterManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "Indices",
                "I",
                "Indices of test values that matched at least one regex input.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "List",
                "L",
                "Filtered test list after applying the boolean mode.",
                GH_ParamAccess.list);

            parameterManager.AddBooleanParameter(
                "Pattern",
                "P",
                "Cull pattern mask. True means the test value matched at least one regex input.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Data",
                "||",
                "Filtered primary parallel stream.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            bool returnMatches = true;
            dataAccess.GetData(0, ref returnMatches);

            List<string> testValues = new List<string>();
            if (!dataAccess.GetDataList(1, testValues))
            {
                return;
            }

            List<string> patterns = new List<string>();
            if (!dataAccess.GetDataList(2, patterns))
            {
                return;
            }

            List<object> data = new List<object>();
            bool hasPrimaryData = dataAccess.GetDataList(3, data);
            if (!hasPrimaryData)
            {
                for (int index = 0; index < testValues.Count; index++)
                {
                    data.Add(testValues[index]);
                }
            }

            Result<RegexCullResult<object>> result = RegexCullLogic.Filter(returnMatches, testValues, patterns, data);
            if (ReportFailure(result))
            {
                return;
            }

            List<int> matchedIndexes = new List<int>();
            List<string> filteredTestValues = new List<string>();

            for (int index = 0; index < result.Value.MatchFlags.Count; index++)
            {
                bool isMatch = result.Value.MatchFlags[index];
                if (isMatch)
                {
                    matchedIndexes.Add(index);
                }

                bool keepItem = returnMatches ? isMatch : !isMatch;
                if (keepItem)
                {
                    filteredTestValues.Add(testValues[index]);
                }
            }

            dataAccess.SetDataList(0, matchedIndexes);
            dataAccess.SetDataList(1, filteredTestValues);
            dataAccess.SetDataList(2, result.Value.MatchFlags);
            dataAccess.SetDataList(3, result.Value.FilteredItems);

            for (int extraIndex = 0; extraIndex < Params.Input.Count - FixedInputCount; extraIndex++)
            {
                int inputIndex = FixedInputCount + extraIndex;
                int filteredOutputIndex = FixedOutputCount + extraIndex;

                List<object> extraData = new List<object>();
                if (!dataAccess.GetDataList(inputIndex, extraData))
                {
                    dataAccess.SetDataList(filteredOutputIndex, new object[0]);
                    continue;
                }

                Result<RegexCullResult<object>> extraResult = RegexCullLogic.Filter(
                    returnMatches,
                    testValues,
                    patterns,
                    extraData);

                if (ReportFailure(extraResult))
                {
                    return;
                }

                dataAccess.SetDataList(filteredOutputIndex, extraResult.Value.FilteredItems);
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index >= FixedInputCount;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index >= FixedInputCount;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            if (side != GH_ParameterSide.Input)
            {
                return null;
            }

            Param_GenericObject parameter = new Param_GenericObject();
            parameter.Access = GH_ParamAccess.list;
            parameter.Optional = true;
            return parameter;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input && index >= FixedInputCount;
        }

        public void VariableParameterMaintenance()
        {
            for (int extraIndex = 0; extraIndex < Params.Input.Count - FixedInputCount; extraIndex++)
            {
                IGH_Param input = Params.Input[FixedInputCount + extraIndex];
                input.Name = "||";
                input.NickName = "||";
                input.Description = "Additional parallel data list to filter with the shared regex mask.";
                input.Access = GH_ParamAccess.list;
                input.Optional = true;
            }

            int expectedOutputCount = FixedOutputCount + (Params.Input.Count - FixedInputCount);
            while (Params.Output.Count < expectedOutputCount)
            {
                Params.RegisterOutputParam(new Param_GenericObject());
            }

            while (Params.Output.Count > expectedOutputCount)
            {
                Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1], true);
            }

            for (int extraIndex = 0; extraIndex < Params.Input.Count - FixedInputCount; extraIndex++)
            {
                int filteredOutputIndex = FixedOutputCount + extraIndex;

                IGH_Param filteredOutput = Params.Output[filteredOutputIndex];
                filteredOutput.Name = "||";
                filteredOutput.NickName = "||";
                filteredOutput.Description = "Filtered data for an additional parallel stream.";
                filteredOutput.Access = GH_ParamAccess.list;
            }
        }
    }
}
