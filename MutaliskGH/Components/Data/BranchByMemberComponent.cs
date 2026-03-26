using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Data
{
    public sealed class BranchByMemberComponent : BaseComponent, IGH_VariableParameterComponent
    {
        private const int FixedInputCount = 1;
        private const int FixedOutputCount = 1;

        public BranchByMemberComponent()
            : base(
                "Branch by Member",
                "BrMember",
                "Sort and branch parallel lists by matching key members. Add more parallel lanes with ZUI.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7c0e2ef6-f258-4f22-ad35-7f0e2fce7f65"); }
        }

        protected override string IconResourceName
        {
            get { return "BranchByMember.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Key",
                "K",
                "List to evaluate for member grouping.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel list to branch by the key members.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel list to branch by the key members.",
                GH_ParamAccess.list);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddBooleanParameter(
                "Pattern",
                "p",
                "Boolean member-match pattern for each distinct key branch.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel list branched by key members.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel list branched by key members.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<IGH_Goo> rawKeys = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawKeys))
            {
                return;
            }

            List<object> keys = GrasshopperValueHelper.UnwrapAll(rawKeys);
            Result<BranchByMemberResult<object>> result = BranchByMemberLogic.Analyze(keys);
            if (ReportFailure(result))
            {
                return;
            }

            GH_Structure<GH_Boolean> patternTree = new GH_Structure<GH_Boolean>();
            for (int branchIndex = 0; branchIndex < result.Value.MatchPatterns.Count; branchIndex++)
            {
                GH_Path path = new GH_Path(branchIndex);
                IReadOnlyList<bool> pattern = result.Value.MatchPatterns[branchIndex];

                for (int itemIndex = 0; itemIndex < pattern.Count; itemIndex++)
                {
                    patternTree.Append(new GH_Boolean(pattern[itemIndex]), path);
                }
            }

            dataAccess.SetDataTree(0, patternTree);

            for (int extraIndex = 0; extraIndex < Params.Input.Count - FixedInputCount; extraIndex++)
            {
                int inputIndex = FixedInputCount + extraIndex;
                int outputIndex = FixedOutputCount + extraIndex;
                List<IGH_Goo> rawValues = new List<IGH_Goo>();
                GH_Structure<IGH_Goo> branchedTree = new GH_Structure<IGH_Goo>();

                bool hasParallelInput = dataAccess.GetDataList(inputIndex, rawValues);
                if (!hasParallelInput && extraIndex == 0)
                {
                    rawValues.AddRange(rawKeys);
                    hasParallelInput = true;
                }

                if (hasParallelInput)
                {
                    for (int branchIndex = 0; branchIndex < result.Value.MatchPatterns.Count; branchIndex++)
                    {
                        GH_Path path = new GH_Path(branchIndex);
                        IReadOnlyList<bool> pattern = result.Value.MatchPatterns[branchIndex];

                        for (int itemIndex = 0; itemIndex < pattern.Count; itemIndex++)
                        {
                            if (!pattern[itemIndex] || itemIndex >= rawValues.Count)
                            {
                                continue;
                            }

                            branchedTree.Append(rawValues[itemIndex], path);
                        }
                    }
                }

                dataAccess.SetDataTree(outputIndex, branchedTree);
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
                input.Description = "Parallel list to branch by the key members.";
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
                IGH_Param output = Params.Output[FixedOutputCount + extraIndex];
                output.Name = "||";
                output.NickName = "||";
                output.Description = "Parallel list branched by key members.";
                output.Access = GH_ParamAccess.tree;
            }
        }
    }
}
