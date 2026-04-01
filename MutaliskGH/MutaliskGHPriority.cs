using Grasshopper;
using Grasshopper.Kernel;
using MutaliskGH.Framework;

namespace MutaliskGH
{
    public sealed class MutaliskGHPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            var icon = IconLoader.Load("MutaliskGH.png");
            if (icon != null)
            {
                Instances.ComponentServer.AddCategoryIcon(CategoryNames.Plugin, icon);
            }

            Instances.ComponentServer.AddCategorySymbolName(CategoryNames.Plugin, 'M');
            return GH_LoadingInstruction.Proceed;
        }
    }
}
