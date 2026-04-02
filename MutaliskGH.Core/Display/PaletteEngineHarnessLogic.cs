using System;
using System.Collections.Generic;

namespace MutaliskGH.Core.Display
{
    public static class PaletteEngineHarnessLogic
    {
        private static readonly PaletteEngineHarnessPreset[] Presets =
        {
            new PaletteEngineHarnessPreset(
                "Simple Repeats",
                "Basic repeated labels. Use this to confirm duplicates share a hue family while still varying within the group.",
                new object[] { "A", "B", "A", "C", "B", "A" },
                0.65,
                7,
                false,
                0.42,
                0.18),
            new PaletteEngineHarnessPreset(
                "Panel Codes",
                "Architectural-style code labels. Useful for checking readability across repeated but longer string values.",
                new object[] { "PNL-101", "PNL-102", "PNL-101", "PNL-103", "PNL-104", "PNL-103" },
                0.65,
                42,
                false,
                0.42,
                0.18),
            new PaletteEngineHarnessPreset(
                "Mixed Types",
                "Mixed text, numbers, and booleans. PaletteEngine groups by normalized label order, not by a text-only assumption.",
                new object[] { "Zone A", 101, false, 101, "Zone B", false, 202, "Zone A" },
                0.8,
                "mixed",
                false,
                0.45,
                0.2),
            new PaletteEngineHarnessPreset(
                "Overdrive Sweep",
                "More aggressive separation. Good for checking how grouped palettes behave when you push distinction harder.",
                new object[] { "Core", "Shell", "Core", "Edge", "Frame", "Shell", "Frame", "Edge", "Core" },
                1.35,
                99,
                true,
                0.5,
                0.24)
        };

        public static Result<PaletteEngineHarnessPreset> GetPreset(int presetIndex)
        {
            if (Presets.Length == 0)
            {
                return Result<PaletteEngineHarnessPreset>.Failure("No PaletteEngine harness presets are available.");
            }

            int clampedIndex = Math.Max(0, Math.Min(Presets.Length - 1, presetIndex));
            return Result<PaletteEngineHarnessPreset>.Success(Presets[clampedIndex]);
        }

        public static Result<IReadOnlyList<object>> ExpandValues(PaletteEngineHarnessPreset preset, int count)
        {
            if (preset == null)
            {
                return Result<IReadOnlyList<object>>.Failure("A harness preset is required.");
            }

            int safeCount = Math.Max(1, count);
            var values = new object[safeCount];
            for (int index = 0; index < safeCount; index++)
            {
                values[index] = preset.Values[index % preset.Values.Count];
            }

            return Result<IReadOnlyList<object>>.Success(values);
        }

        public static int PresetCount
        {
            get { return Presets.Length; }
        }
    }
}
