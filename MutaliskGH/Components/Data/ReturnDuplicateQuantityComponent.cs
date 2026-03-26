using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Data
{
    public sealed class ReturnDuplicateQuantityComponent : BaseComponent
    {
        public ReturnDuplicateQuantityComponent()
            : base(
                "Return Duplicate Quantity",
                "DupQty",
                "Return the distinct set and quantity of each value in a list.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("0dd0ff2f-7f70-44df-86da-4f0217e61d1d"); }
        }

        protected override string IconResourceName
        {
            get { return "ReturnDuplicateQuantity.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Input list of values to count.",
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
                "Quantity",
                "Q",
                "Quantity of each distinct value in the set output.",
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
            Result<DuplicateQuantityResult<object>> result = ReturnDuplicateQuantityLogic.Analyze(values);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetDataList(0, result.Value.DistinctValues);
            dataAccess.SetDataList(1, result.Value.Quantities);
        }
    }
}
