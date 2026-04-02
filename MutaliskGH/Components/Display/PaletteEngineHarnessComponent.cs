using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Display;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Display
{
    public sealed class PaletteEngineHarnessComponent : BaseComponent
    {
        public PaletteEngineHarnessComponent()
            : base(
                "PaletteEngine Harness",
                "PaletteTest",
                "Emit canned sample inputs for quick PaletteEngine testing.",
                CategoryNames.Display)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("42f13839-5325-4fc8-9fe0-3216f75a5006"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddIntegerParameter(
                "Preset",
                "P",
                "Harness preset index. 0 = Simple Repeats, 1 = Panel Codes, 2 = Mixed Types, 3 = Overdrive Sweep.",
                GH_ParamAccess.item,
                0);

            parameterManager.AddIntegerParameter(
                "Count",
                "C",
                "Requested sample item count. Values and geometry are repeated to reach this size.",
                GH_ParamAccess.item,
                48);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGeometryParameter(
                "Geometry",
                "G",
                "Sample geometry aligned 1:1 with the output values.",
                GH_ParamAccess.list);

            parameterManager.AddGenericParameter(
                "Values",
                "V",
                "Sample values for PaletteEngine.",
                GH_ParamAccess.list);

            parameterManager.AddNumberParameter(
                "Strength",
                "Str",
                "Sample strength input.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Seed",
                "S",
                "Sample seed input.",
                GH_ParamAccess.item);

            parameterManager.AddBooleanParameter(
                "Overdrive",
                "O",
                "Sample overdrive input.",
                GH_ParamAccess.item);

            parameterManager.AddNumberParameter(
                "Min Saturation",
                "MinSat",
                "Sample minimum saturation input.",
                GH_ParamAccess.item);

            parameterManager.AddNumberParameter(
                "Saturation Boost",
                "SatB",
                "Sample saturation boost input.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Preset Name",
                "Name",
                "Selected harness preset name.",
                GH_ParamAccess.item);

            parameterManager.AddTextParameter(
                "Notes",
                "N",
                "Short note describing what the preset is useful for.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            int presetIndex = 0;
            dataAccess.GetData(0, ref presetIndex);
            int count = 48;
            dataAccess.GetData(1, ref count);

            Result<PaletteEngineHarnessPreset> result = PaletteEngineHarnessLogic.GetPreset(presetIndex);
            if (ReportFailure(result))
            {
                return;
            }

            PaletteEngineHarnessPreset preset = result.Value;
            Result<IReadOnlyList<object>> expandedValuesResult = PaletteEngineHarnessLogic.ExpandValues(preset, count);
            if (ReportFailure(expandedValuesResult))
            {
                return;
            }

            IReadOnlyList<object> values = expandedValuesResult.Value;
            dataAccess.SetDataList(0, BuildSampleGeometry(presetIndex, values.Count));
            dataAccess.SetDataList(1, values);
            dataAccess.SetData(2, preset.Strength);
            dataAccess.SetData(3, preset.Seed);
            dataAccess.SetData(4, preset.Overdrive);
            dataAccess.SetData(5, preset.MinimumSaturation);
            dataAccess.SetData(6, preset.SaturationBoost);
            dataAccess.SetData(7, preset.Name);
            dataAccess.SetData(8, preset.Notes);
        }

        private static IEnumerable<GeometryBase> BuildSampleGeometry(int presetIndex, int count)
        {
            var geometry = new List<GeometryBase>(count);
            for (int index = 0; index < count; index++)
            {
                int column = index % 4;
                int row = index / 4;
                Plane plane = new Plane(new Point3d(column * 8.0, -row * 8.0, 0.0), Vector3d.ZAxis);

                switch (presetIndex)
                {
                    case 1:
                        geometry.Add(new Rectangle3d(plane, 5.0, 3.5).ToNurbsCurve());
                        break;

                    case 2:
                        if (index % 3 == 0)
                        {
                            geometry.Add(new Circle(plane, 2.5).ToNurbsCurve());
                        }
                        else if (index % 3 == 1)
                        {
                            geometry.Add(new LineCurve(
                                plane.Origin + (plane.XAxis * -2.5),
                                plane.Origin + (plane.XAxis * 2.5) + (plane.YAxis * 1.5)));
                        }
                        else
                        {
                            geometry.Add(new PolylineCurve(new[]
                            {
                                plane.Origin + (plane.XAxis * -2.5) + (plane.YAxis * -2.0),
                                plane.Origin + (plane.XAxis * 2.5) + (plane.YAxis * -2.0),
                                plane.Origin + (plane.YAxis * 2.0),
                                plane.Origin + (plane.XAxis * -2.5) + (plane.YAxis * -2.0)
                            }));
                        }
                        break;

                    case 3:
                        geometry.Add(new Circle(plane, 3.0 - ((index % 3) * 0.45)).ToNurbsCurve());
                        break;

                    default:
                        geometry.Add(new Circle(plane, 2.4).ToNurbsCurve());
                        break;
                }
            }

            return geometry;
        }
    }
}
