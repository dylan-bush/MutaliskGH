using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Display;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.Geometry;

namespace MutaliskGH.Components.Display
{
    public sealed class ColorByBranchComponent : BaseComponent
    {
        private readonly List<List<object>> previewBranches = new List<List<object>>();
        private readonly List<Color> previewColors = new List<Color>();
        private BoundingBox clippingBox = BoundingBox.Empty;

        public ColorByBranchComponent()
            : base(
                "Color by Branch",
                "CBR",
                "Return one deterministic color per input branch.",
                CategoryNames.Display)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("cedc8dc6-4c7f-4786-819a-5ef03e591ddf"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "List",
                "L",
                "Data tree used to derive one color per branch.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "Seed",
                "S",
                "Optional palette seed.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddColourParameter(
                "Color",
                "Col",
                "Colors aligned to input branches.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            ClearPreview();

            if (Params.Input[0].VolatileDataCount == 0)
            {
                return;
            }

            GH_Structure<IGH_Goo> values;
            if (!dataAccess.GetDataTree(0, out values) || values.PathCount == 0)
            {
                return;
            }

            IGH_Goo rawSeed = null;
            dataAccess.GetData(1, ref rawSeed);
            object seed = GrasshopperValueHelper.Unwrap(rawSeed);

            List<object> labels = new List<object>(values.PathCount);
            for (int index = 0; index < values.PathCount; index++)
            {
                labels.Add(values.Paths[index].ToString());
            }

            Result<PaletteEngineResult> result = PaletteEngineLogic.Generate(labels, 0.65, seed, false, 0.42, 0.18);
            if (ReportFailure(result))
            {
                return;
            }

            GH_Structure<GH_Colour> colors = new GH_Structure<GH_Colour>();
            for (int index = 0; index < result.Value.OutputColors.Count; index++)
            {
                Color color = ToColor(result.Value.OutputColors[index]);
                GH_Path path = values.Paths[index];
                colors.Append(new GH_Colour(color), path);

                previewColors.Add(color);
                List<object> branchPreview = new List<object>();
                for (int itemIndex = 0; itemIndex < values.Branches[index].Count; itemIndex++)
                {
                    object unwrapped = GrasshopperValueHelper.Unwrap(values.Branches[index][itemIndex]);
                    if (unwrapped != null)
                    {
                        branchPreview.Add(unwrapped);
                    }
                }

                previewBranches.Add(branchPreview);
            }

            clippingBox = BuildClippingBox();
            dataAccess.SetDataTree(0, colors);
        }

        public override BoundingBox ClippingBox
        {
            get { return clippingBox; }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            for (int index = 0; index < previewBranches.Count && index < previewColors.Count; index++)
            {
                DisplayPreviewHelper.DrawViewportMeshes(args.Display, previewBranches[index], previewColors[index]);
            }
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);

            for (int index = 0; index < previewBranches.Count && index < previewColors.Count; index++)
            {
                DisplayPreviewHelper.DrawViewportWires(args.Display, previewBranches[index], previewColors[index]);
            }
        }

        private BoundingBox BuildClippingBox()
        {
            List<object> flattened = new List<object>();
            for (int branchIndex = 0; branchIndex < previewBranches.Count; branchIndex++)
            {
                flattened.AddRange(previewBranches[branchIndex]);
            }

            return DisplayPreviewHelper.GetClippingBox(flattened);
        }

        private void ClearPreview()
        {
            previewBranches.Clear();
            previewColors.Clear();
            clippingBox = BoundingBox.Empty;
        }

        private static Color ToColor(PaletteColorValue color)
        {
            return Color.FromArgb(color.Red, color.Green, color.Blue);
        }
    }
}
