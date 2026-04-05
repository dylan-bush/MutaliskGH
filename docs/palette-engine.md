# PaletteEngine Guide

`PaletteEngine` is MutaliskGH's deterministic grouped-color engine for Grasshopper. It takes an ordered list of values and returns related-but-distinct colors that stay stable for the same inputs and seed.

## What It Does

- Groups repeated values into the same color family
- Preserves first-occurrence group order
- Produces aligned colors for every input item
- Produces grouped palettes for downstream branch-based preview workflows
- Uses a seed so palettes can be re-shuffled deterministically without changing the underlying value grouping

## Typical Inputs

- `Values (V)`
  A flat list of labels, numbers, booleans, or mixed values
- `Strength (Str)`
  Controls variation inside each group
- `Seed (S)`
  Deterministic palette shuffle control
- `Overdrive (O)`
  Pushes stronger separation
- `Min Saturation (MinSat)`
  Saturation floor from `0..1`
- `Saturation Boost (SatB)`
  Additional saturation bias from `0..1`

## Value List Examples

```text
["A", "B", "A", "C", "B", "A"]
```

```text
["PNL-101", "PNL-102", "PNL-101", "PNL-103"]
```

```text
["Zone A", 101, false, 101, "Zone B", false]
```

## Output Structure

- `Colors (Col)`
  One color per input item, aligned to the original order
- `RGB`
  The same colors as `r,g,b` strings
- `Set`
  Distinct values in first-occurrence order
- `Group Colors (Grp)`
  One branch per distinct value, containing that group's palette

## Example

Input:

```text
Values = ["A", "B", "A", "C", "B", "A"]
Seed = 7
Strength = 0.65
Overdrive = false
MinSat = 0.42
SatB = 0.18
```

Output shape:

```text
Set = ["A", "B", "C"]
Colors = [col(A1), col(B1), col(A2), col(C1), col(B2), col(A3)]
Group Colors:
{0} -> A palette
{1} -> B palette
{2} -> C palette
```

## Behavior Notes

- The same value and seed combination will always produce the same grouped palette.
- Duplicate values stay in the same hue family, but they do not all collapse to one identical color.
- Changing only `Seed (S)` re-shuffles group hues deterministically.
- `PaletteEngine Harness` is the quickest way to generate large sample sets for evaluation.
- The harness emits geometry aligned `1:1` with the value list, so it can feed directly into `Preview Color by Value` for full display-chain testing.
