using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Display;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MutaliskGH.Components.Display
{
    public sealed class PaletteEngineComponent : BaseComponent
    {
        public PaletteEngineComponent()
            : base(
                "PaletteEngine",
                "Palette",
                "Generate deterministic grouped palette colors from a list of values.",
                CategoryNames.Display)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("74f4a6cc-920b-43ea-a124-3f367c2141fb"); }
        }

        protected override string IconResourceName
        {
            get { return "PaletteEngine.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Values",
                "V",
                "Values used to form color groups.",
                GH_ParamAccess.list);

            parameterManager.AddNumberParameter(
                "Strength",
                "Str",
                "Variation strength. 0..1 is normal, values above 1 push overdrive-style separation.",
                GH_ParamAccess.item,
                0.65);

            parameterManager.AddGenericParameter(
                "Seed",
                "S",
                "Optional palette seed. Changing the seed deterministically reshuffles group hues.",
                GH_ParamAccess.item);

            parameterManager.AddBooleanParameter(
                "Overdrive",
                "O",
                "Force stronger separation between related colors.",
                GH_ParamAccess.item,
                false);

            parameterManager.AddNumberParameter(
                "Min Saturation",
                "MinSat",
                "Minimum saturation floor in the range 0..1.",
                GH_ParamAccess.item,
                0.42);

            parameterManager.AddNumberParameter(
                "Saturation Boost",
                "SatB",
                "Additional saturation boost in the range 0..1.",
                GH_ParamAccess.item,
                0.18);

            for (int index = 0; index < Params.Input.Count; index++)
            {
                Params.Input[index].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddColourParameter(
                "Colors",
                "Col",
                "Colors aligned to the input order.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "RGB",
                "RGB",
                "RGB strings aligned to the input order, formatted as r,g,b.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Set",
                "Set",
                "Distinct value labels in first-occurrence order.",
                GH_ParamAccess.list);

            parameterManager.AddColourParameter(
                "Group Colors",
                "Grp",
                "Grouped palette colors, one branch per distinct value label.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<IGH_Goo> rawValues = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawValues) || rawValues.Count == 0)
            {
                return;
            }

            double strength = 0.65;
            dataAccess.GetData(1, ref strength);

            IGH_Goo rawSeed = null;
            dataAccess.GetData(2, ref rawSeed);
            object seed = GrasshopperValueHelper.Unwrap(rawSeed);

            bool overdrive = false;
            dataAccess.GetData(3, ref overdrive);

            double minimumSaturation = 0.42;
            dataAccess.GetData(4, ref minimumSaturation);

            double saturationBoost = 0.18;
            dataAccess.GetData(5, ref saturationBoost);

            List<object> values = GrasshopperValueHelper.UnwrapAll(rawValues);
            Result<PaletteEngineResult> result = PaletteEngineLogic.Generate(
                values,
                strength,
                seed,
                overdrive,
                minimumSaturation,
                saturationBoost);

            if (ReportFailure(result))
            {
                return;
            }

            var colors = new List<Color>(result.Value.OutputColors.Count);
            var rgbStrings = new List<string>(result.Value.OutputColors.Count);
            for (int index = 0; index < result.Value.OutputColors.Count; index++)
            {
                Color color = ToColor(result.Value.OutputColors[index]);
                colors.Add(color);
                rgbStrings.Add(color.R + "," + color.G + "," + color.B);
            }

            GH_Structure<GH_Colour> groupTree = new GH_Structure<GH_Colour>();
            for (int groupIndex = 0; groupIndex < result.Value.GroupLabels.Count; groupIndex++)
            {
                string label = result.Value.GroupLabels[groupIndex];
                IReadOnlyList<PaletteColorValue> groupColors;
                if (!result.Value.GroupPalettes.TryGetValue(label, out groupColors))
                {
                    continue;
                }

                GH_Path path = new GH_Path(groupIndex);
                for (int colorIndex = 0; colorIndex < groupColors.Count; colorIndex++)
                {
                    groupTree.Append(new GH_Colour(ToColor(groupColors[colorIndex])), path);
                }
            }

            dataAccess.SetDataList(0, colors);
            dataAccess.SetDataList(1, rgbStrings);
            dataAccess.SetDataList(2, result.Value.GroupLabels);
            dataAccess.SetDataTree(3, groupTree);
        }

        private static Color ToColor(PaletteColorValue color)
        {
            return Color.FromArgb(color.Red, color.Green, color.Blue);
        }
    }
}
