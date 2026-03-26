using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Rhino;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Rhino
{
    public sealed class SelValueComponent : BaseComponent
    {
        private bool wasTriggered;

        public SelValueComponent()
            : base(
                "SelValue",
                "SelValue",
                "Run Rhino SelValue for the provided string on a rising-edge toggle.",
                CategoryNames.Rhino)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("1d25adce-6c95-4718-b7e5-05bd43a0f1d8"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Strings",
                "Str",
                "Value strings passed to Rhino SelValue.",
                GH_ParamAccess.list);

            parameterManager.AddBooleanParameter(
                "Toggle",
                "Toggle",
                "Run SelValue on a rising-edge toggle.",
                GH_ParamAccess.item,
                false);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            var values = new List<string>();
            dataAccess.GetDataList(0, values);

            bool toggle = false;
            dataAccess.GetData(1, ref toggle);

            if (toggle && !wasTriggered)
            {
                Result<IReadOnlyList<string>> commandResult = RhinoCommandLogic.BuildSelValueCommands(values);
                if (ReportFailure(commandResult))
                {
                    wasTriggered = toggle;
                    return;
                }

                foreach (string command in commandResult.Value)
                {
                    global::Rhino.RhinoApp.RunScript(command, false);
                }
            }

            wasTriggered = toggle;
        }
    }
}
