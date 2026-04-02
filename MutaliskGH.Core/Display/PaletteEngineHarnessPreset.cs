using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public sealed class PaletteEngineHarnessPreset
    {
        public PaletteEngineHarnessPreset(
            string name,
            string notes,
            IReadOnlyList<object> values,
            double strength,
            object seed,
            bool overdrive,
            double minimumSaturation,
            double saturationBoost)
        {
            Name = name;
            Notes = notes;
            Values = values;
            Strength = strength;
            Seed = seed;
            Overdrive = overdrive;
            MinimumSaturation = minimumSaturation;
            SaturationBoost = saturationBoost;
        }

        public string Name { get; }

        public string Notes { get; }

        public IReadOnlyList<object> Values { get; }

        public double Strength { get; }

        public object Seed { get; }

        public bool Overdrive { get; }

        public double MinimumSaturation { get; }

        public double SaturationBoost { get; }
    }
}
