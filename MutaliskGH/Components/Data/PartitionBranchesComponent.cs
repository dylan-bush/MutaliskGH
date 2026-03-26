using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Data
{
    public sealed class PartitionBranchesComponent : BaseComponent
    {
        public PartitionBranchesComponent()
            : base(
                "Partition Branches",
                "PartBr",
                "Partition selected branches of a tree while leaving unselected branches unaltered.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("a70c0c11-2d98-4f86-a6a4-4eb74aa1b478"); }
        }

        protected override string IconResourceName
        {
            get { return "PartitionBranches.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Tree to partition branch-by-branch.",
                GH_ParamAccess.tree);

            parameterManager.AddIntegerParameter(
                "Pattern",
                "P",
                "Yes-or-no branch pattern. Accepts either a flat list or a grafted tree with one value per branch.",
                GH_ParamAccess.tree);

            parameterManager.AddIntegerParameter(
                "Size",
                "S",
                "Size of partitions applied to branches selected by the pattern.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Tree",
                "T",
                "Guide tree with unaltered branches and partitioned sub-branches.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            if (Params.Input[0].VolatileDataCount == 0 || Params.Input[1].VolatileDataCount == 0)
            {
                return;
            }

            GH_Structure<IGH_Goo> inputTree;
            if (!dataAccess.GetDataTree(0, out inputTree))
            {
                return;
            }

            GH_Structure<GH_Integer> patternTree;
            if (!dataAccess.GetDataTree(1, out patternTree))
            {
                return;
            }

            int size = 1;
            dataAccess.GetData(2, ref size);

            List<IReadOnlyList<int>> patternBranches = new List<IReadOnlyList<int>>(patternTree.PathCount);
            for (int branchIndex = 0; branchIndex < patternTree.PathCount; branchIndex++)
            {
                List<int> branchValues = new List<int>(patternTree.Branches[branchIndex].Count);
                for (int itemIndex = 0; itemIndex < patternTree.Branches[branchIndex].Count; itemIndex++)
                {
                    branchValues.Add(patternTree.Branches[branchIndex][itemIndex].Value);
                }

                patternBranches.Add(branchValues);
            }

            Result<IReadOnlyList<int>> normalizedPatternResult = PartitionBranchesLogic.NormalizePattern(patternBranches);
            if (ReportFailure(normalizedPatternResult))
            {
                return;
            }

            List<IReadOnlyList<IGH_Goo>> branches = new List<IReadOnlyList<IGH_Goo>>(inputTree.PathCount);
            for (int branchIndex = 0; branchIndex < inputTree.PathCount; branchIndex++)
            {
                branches.Add(inputTree.Branches[branchIndex]);
            }

            Result<PartitionBranchesResult<IGH_Goo>> result = PartitionBranchesLogic.Partition(
                branches,
                normalizedPatternResult.Value,
                size);
            if (ReportFailure(result))
            {
                return;
            }

            GH_Structure<IGH_Goo> tree = new GH_Structure<IGH_Goo>();
            for (int groupIndex = 0; groupIndex < result.Value.Groups.Count; groupIndex++)
            {
                PartitionBranchesGroup<IGH_Goo> group = result.Value.Groups[groupIndex];
                GH_Path sourcePath = inputTree.Paths[group.BranchIndex];

                for (int segmentIndex = 0; segmentIndex < group.Segments.Count; segmentIndex++)
                {
                    GH_Path outputPath = sourcePath.AppendElement(segmentIndex);
                    tree.EnsurePath(outputPath);

                    IReadOnlyList<IGH_Goo> segment = group.Segments[segmentIndex];
                    for (int itemIndex = 0; itemIndex < segment.Count; itemIndex++)
                    {
                        tree.Append(segment[itemIndex], outputPath);
                    }
                }
            }

            dataAccess.SetDataTree(0, tree);
        }
    }
}
