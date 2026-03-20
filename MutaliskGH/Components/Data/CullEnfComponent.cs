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
    public sealed class CullEnfComponent : BaseComponent, IGH_VariableParameterComponent
    {
        private const int FixedInputCount = 1;
        private const int FixedOutputCount = 1;

        public CullEnfComponent()
            : base(
                "Cull ENF",
                "CullENF",
                "Remove branches that are empty, null, false-only, or text-length-zero, with matching parallel branch removal.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a7cd20b6-0b58-454a-b67d-56f8f7d0c810"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Basis tree for branch culling.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel tree to cull with the same branch-removal mask.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel tree to cull with the same branch-removal mask.",
                GH_ParamAccess.tree);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Basis tree with empty, null, and false-only branches removed.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel tree with the same branches removed as the basis tree.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "||",
                "||",
                "Parallel tree with the same branches removed as the basis tree.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            GH_Structure<IGH_Goo> basisTree;
            if (!dataAccess.GetDataTree(0, out basisTree))
            {
                return;
            }

            List<IReadOnlyList<object>> basisBranches = new List<IReadOnlyList<object>>(basisTree.PathCount);
            for (int branchIndex = 0; branchIndex < basisTree.PathCount; branchIndex++)
            {
                IList<IGH_Goo> branch = basisTree.Branches[branchIndex];
                List<IGH_Goo> branchValues = new List<IGH_Goo>(branch.Count);
                for (int itemIndex = 0; itemIndex < branch.Count; itemIndex++)
                {
                    branchValues.Add(branch[itemIndex]);
                }

                basisBranches.Add(GrasshopperValueHelper.UnwrapAll(branchValues));
            }

            Result<IReadOnlyList<bool>> keepFlagsResult = CullEnfLogic.EvaluateKeepFlags(basisBranches);
            if (ReportFailure(keepFlagsResult))
            {
                return;
            }

            GH_Structure<IGH_Goo> culledBasisTree = FilterTree(basisTree, keepFlagsResult.Value, basisTree);
            dataAccess.SetDataTree(0, culledBasisTree);

            for (int extraIndex = 0; extraIndex < Params.Input.Count - FixedInputCount; extraIndex++)
            {
                int inputIndex = FixedInputCount + extraIndex;
                int outputIndex = FixedOutputCount + extraIndex;
                GH_Structure<IGH_Goo> parallelTree;

                if (!dataAccess.GetDataTree(inputIndex, out parallelTree))
                {
                    dataAccess.SetDataTree(outputIndex, new GH_Structure<IGH_Goo>());
                    continue;
                }

                GH_Structure<IGH_Goo> culledParallelTree = FilterTree(parallelTree, keepFlagsResult.Value, basisTree);
                dataAccess.SetDataTree(outputIndex, culledParallelTree);
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
            parameter.Access = GH_ParamAccess.tree;
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
                input.Description = "Parallel tree to cull with the same branch-removal mask.";
                input.Access = GH_ParamAccess.tree;
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
                output.Description = "Parallel tree with the same branches removed as the basis tree.";
                output.Access = GH_ParamAccess.tree;
            }
        }

        private static GH_Structure<IGH_Goo> FilterTree(
            GH_Structure<IGH_Goo> sourceTree,
            IReadOnlyList<bool> keepFlags,
            GH_Structure<IGH_Goo> basisTree)
        {
            GH_Structure<IGH_Goo> filteredTree = new GH_Structure<IGH_Goo>();

            for (int branchIndex = 0; branchIndex < keepFlags.Count && branchIndex < basisTree.PathCount; branchIndex++)
            {
                if (!keepFlags[branchIndex])
                {
                    continue;
                }

                GH_Path keptPath = basisTree.Paths[branchIndex];
                filteredTree.EnsurePath(keptPath);

                int sourceBranchIndex = sourceTree.Paths.IndexOf(keptPath);
                if (sourceBranchIndex < 0)
                {
                    continue;
                }

                IList<IGH_Goo> sourceBranch = sourceTree.Branches[sourceBranchIndex];
                for (int itemIndex = 0; itemIndex < sourceBranch.Count; itemIndex++)
                {
                    filteredTree.Append(sourceBranch[itemIndex], keptPath);
                }
            }

            return filteredTree;
        }
    }
}
