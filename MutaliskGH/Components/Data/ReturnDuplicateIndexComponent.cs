using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Data
{
    public sealed class ReturnDuplicateIndexComponent : BaseComponent
    {
        public ReturnDuplicateIndexComponent()
            : base(
                "Return Duplicate Index",
                "DupIdx",
                "Return the distinct set, first-occurrence indexes, and duplicate indexes from a list.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("90d898be-2176-413b-8971-ec964570a76b"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Input list that may contain duplicate values.",
                GH_ParamAccess.list);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Set",
                "S",
                "Distinct values in first-occurrence order.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Index",
                "i",
                "Indexes of the first occurrence of each distinct value.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Cull Index",
                "C",
                "Indexes of values that were culled as duplicates.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<IGH_Goo> rawValues = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawValues))
            {
                return;
            }

            List<object> values = GrasshopperValueHelper.UnwrapAll(rawValues);
            Result<DuplicateIndexResult<object>> result = ReturnDuplicateIndexLogic.Analyze(values);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetDataList(0, result.Value.DistinctValues);
            dataAccess.SetDataList(1, result.Value.RetainedIndexes);
            dataAccess.SetDataList(2, result.Value.CulledIndexes);
        }
    }
}
