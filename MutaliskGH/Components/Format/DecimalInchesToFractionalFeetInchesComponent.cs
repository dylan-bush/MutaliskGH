using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Format;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Format
{
    public sealed class DecimalInchesToFractionalFeetInchesComponent : BaseComponent
    {
        public DecimalInchesToFractionalFeetInchesComponent()
            : base(
                "Decimal In to Fractional Ft In",
                "D2F",
                "Convert decimal inches to rounded architectural feet-and-inches text.",
                CategoryNames.Format)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("25b75177-bb6f-4871-bd6e-fd30907d6ae5"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddNumberParameter(
                "Decimal Inches",
                "In",
                "Decimal inch value to convert.",
                GH_ParamAccess.item);

            parameterManager.AddIntegerParameter(
                "Denominator",
                "D",
                "Fractional inch denominator, such as 16 for sixteenths.",
                GH_ParamAccess.item,
                16);

            parameterManager.AddIntegerParameter(
                "Mode",
                "M",
                "Rounding mode: 0 = nearest, 1 = up, 2 = down.",
                GH_ParamAccess.item,
                0);

            parameterManager.AddBooleanParameter(
                "Show Zero Inches",
                "Z",
                "Show zero inches when the value lands on an exact foot.",
                GH_ParamAccess.item,
                true);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
            parameterManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "Txt",
                "Architectural feet-and-inches text.",
                GH_ParamAccess.item);

            parameterManager.AddNumberParameter(
                "Rounded Inches",
                "In",
                "Rounded total inches after denominator and mode are applied.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            double decimalInches = 0.0;
            if (!dataAccess.GetData(0, ref decimalInches))
            {
                return;
            }

            int denominator = 16;
            dataAccess.GetData(1, ref denominator);

            int mode = 0;
            dataAccess.GetData(2, ref mode);

            bool showZeroInches = true;
            dataAccess.GetData(3, ref showZeroInches);

            Result<DecimalFeetInchesResult> result = DecimalFeetInchesLogic.Convert(
                decimalInches,
                denominator,
                mode,
                showZeroInches);

            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetData(0, result.Value.FormattedText);
            dataAccess.SetData(1, result.Value.RoundedInches);
        }
    }
}
