using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Display;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MutaliskGH.Components.Display
{
    public sealed class PreviewColorByValueComponent : BaseComponent
    {
        private readonly List<List<object>> previewBranches = new List<List<object>>();
        private readonly List<Color> previewColors = new List<Color>();
        private BoundingBox clippingBox = BoundingBox.Empty;

        public PreviewColorByValueComponent()
            : base(
                "Preview Color by Value",
                "PCV",
                "Group geometry by value, generate branch colors, and preview each branch with its generated color.",
                CategoryNames.Display)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c6d048d0-8fe4-463c-a238-f39d26103d4c"); }
        }

        protected override string IconResourceName
        {
            get { return "PreviewColorByValue.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Geometry",
                "Geometry",
                "Geometry or data to preview.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Values",
                "Values",
                "Values used to derive colors.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Seed",
                "S",
                "Optional palette seed.",
                GH_ParamAccess.item);

            parameterManager.AddBooleanParameter(
                "Gradient",
                "Bool",
                "False uses grouped palette colors. True uses a gradient by distinct-value order.",
                GH_ParamAccess.item,
                false);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
            parameterManager[2].Optional = true;
            parameterManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Filtered Input",
                "Filtered Input",
                "Geometry branched by distinct value.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "Filtered Values",
                "Filtered Values",
                "Values branched by distinct value.",
                GH_ParamAccess.tree);

            parameterManager.AddGenericParameter(
                "Set",
                "Set",
                "Distinct values in first-occurrence order.",
                GH_ParamAccess.list);

            parameterManager.AddColourParameter(
                "Color",
                "Col",
                "One color per output branch, grafted to match the filtered input grouping.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            ClearPreview();

            List<IGH_Goo> rawGeometry = new List<IGH_Goo>();
            List<IGH_Goo> rawValues = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawGeometry) || !dataAccess.GetDataList(1, rawValues))
            {
                return;
            }

            if (rawGeometry.Count == 0 || rawValues.Count == 0)
            {
                return;
            }

            IGH_Goo rawSeed = null;
            dataAccess.GetData(2, ref rawSeed);
            object seed = GrasshopperValueHelper.Unwrap(rawSeed);

            bool useGradient = false;
            dataAccess.GetData(3, ref useGradient);

            List<object> geometry = GrasshopperValueHelper.UnwrapAll(rawGeometry);
            List<object> values = GrasshopperValueHelper.UnwrapAll(rawValues);

            Result<PreviewColorByValueResult> result = PreviewColorByValueLogic.Evaluate(geometry, values, seed, useGradient);
            if (ReportFailure(result))
            {
                return;
            }

            GH_Structure<IGH_Goo> geometryTree = new GH_Structure<IGH_Goo>();
            GH_Structure<IGH_Goo> valueTree = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_Colour> colorTree = new GH_Structure<GH_Colour>();

            List<IGH_Goo> filteredGeometry = new List<IGH_Goo>();
            List<IGH_Goo> filteredValues = new List<IGH_Goo>();
            for (int index = 0; index < System.Math.Min(rawGeometry.Count, rawValues.Count); index++)
            {
                filteredGeometry.Add(rawGeometry[index]);
                filteredValues.Add(rawValues[index]);
            }

            for (int branchIndex = 0; branchIndex < result.Value.MatchPatterns.Count; branchIndex++)
            {
                GH_Path path = new GH_Path(branchIndex);
                IReadOnlyList<bool> pattern = result.Value.MatchPatterns[branchIndex];
                Color branchColor = ToColor(result.Value.BranchColors[branchIndex]);
                colorTree.Append(new GH_Colour(branchColor), path);
                previewColors.Add(branchColor);

                List<object> previewBranch = new List<object>();
                for (int itemIndex = 0; itemIndex < pattern.Count; itemIndex++)
                {
                    if (!pattern[itemIndex])
                    {
                        continue;
                    }

                    if (itemIndex < filteredGeometry.Count)
                    {
                        geometryTree.Append(filteredGeometry[itemIndex], path);
                        object previewValue = GrasshopperValueHelper.Unwrap(filteredGeometry[itemIndex]);
                        if (previewValue != null)
                        {
                            previewBranch.Add(previewValue);
                        }
                    }

                    if (itemIndex < filteredValues.Count)
                    {
                        valueTree.Append(filteredValues[itemIndex], path);
                    }
                }

                previewBranches.Add(previewBranch);
            }

            clippingBox = BuildClippingBox();

            dataAccess.SetDataTree(0, geometryTree);
            dataAccess.SetDataTree(1, valueTree);
            dataAccess.SetDataList(2, result.Value.DistinctValues);
            dataAccess.SetDataTree(3, colorTree);
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
