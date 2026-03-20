using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Data
{
    public sealed class IntegerSeriesComponent : BaseComponent
    {
        public IntegerSeriesComponent()
            : base(
                "Integer Series",
                "IntSeries",
                "Create an inclusive series of integers from start to end.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6d64c541-1ecc-427a-a890-c2049e75eefa"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "Start",
                "S",
                "Inclusive domain start.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "End",
                "E",
                "Inclusive domain end.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "Series",
                "S",
                "Inclusive integer series from start to end.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            int start = 0;
            if (!dataAccess.GetData(0, ref start))
            {
                return;
            }

            int end = 0;
            if (!dataAccess.GetData(1, ref end))
            {
                return;
            }

            Result<System.Collections.Generic.IReadOnlyList<int>> result = IntegerSeriesLogic.CreateInclusive(start, end);
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetDataList(0, result.Value);
        }
    }
}
