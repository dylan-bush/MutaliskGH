using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Format;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Format
{
    public sealed class FindNextAvailableCodeComponent : BaseComponent
    {
        public FindNextAvailableCodeComponent()
            : base(
                "Find Next Available Code",
                "NextCode",
                "Find the nearest available code using a format with fixed 0 slots and searchable # slots.",
                CategoryNames.Format)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("672e4797-eadc-4109-9b7d-3ed266eb7e18"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Target",
                "T",
                "Target code, such as 042069.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Taken",
                "L",
                "List of already-used codes.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Format",
                "F",
                "Code format, such as {000###} for the original behavior or {000000} for a fully searchable 6-digit code.",
                GH_ParamAccess.item,
                "{000###}");

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Code",
                "C",
                "Nearest available code formatted to the requested pattern.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawTarget = null;
            if (!dataAccess.GetData(0, ref rawTarget))
            {
                return;
            }

            List<IGH_Goo> rawTaken = new List<IGH_Goo>();
            dataAccess.GetDataList(1, rawTaken);

            string format = "{000###}";
            dataAccess.GetData(2, ref format);

            Result<string> result = NextAvailableCodeLogic.FindNext(
                GrasshopperValueHelper.Unwrap(rawTarget),
                GrasshopperValueHelper.UnwrapAll(rawTaken),
                format);

            if (ReportFailure(result))
            {
                return;
            }

            if (result.Value != null)
            {
                dataAccess.SetData(0, result.Value);
            }
        }
    }
}
