using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core.Data;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Data
{
    public sealed class TestNullOrTextLengthZeroComponent : BaseComponent
    {
        public TestNullOrTextLengthZeroComponent()
            : base(
                "Convert to Boolean",
                "ToBool",
                "Convert generic input to a boolean value using broad truthiness rules.",
                CategoryNames.Data)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("69a9ed49-819d-40bc-9ce4-e080c1e22038"); }
        }

        protected override string IconResourceName
        {
            get { return "ConvertToBoolean.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Value",
                "V",
                "Value to convert to a boolean.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddBooleanParameter(
                "Boolean",
                "B",
                "Boolean result of the conversion.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawValue = null;
            if (!dataAccess.GetData(0, ref rawValue))
            {
                if (Params.Input[0].VolatileDataCount > 0)
                {
                    dataAccess.SetData(0, false);
                }

                return;
            }

            object value = GrasshopperValueHelper.Unwrap(rawValue);
            dataAccess.SetData(0, TestNullOrTextLengthZeroLogic.Evaluate(value).Value);
        }
    }
}
