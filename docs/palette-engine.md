# PaletteEngine

`PaletteEngine` generates deterministic grouped palette colors from a list of values.

## What It Expects

- `Values (V)` is a flat list.
- Values do not need to be text only.
- Repeated values form one color family.
- Distinct values keep first-occurrence order.

Examples of valid `Values` lists:

```text
["A", "B", "A", "C", "B", "A"]
```

```text
["PNL-101", "PNL-102", "PNL-101", "PNL-103"]
```

```text
["Zone A", 101, false, 101, "Zone B", false]
```

## Inputs

- `Values (V)`: list of labels or values to group by
- `Strength (Str)`: color variation within each group
  - `0..1` = normal
  - `>1` = stronger variation
- `Seed (S)`: deterministic shuffle control for group hues
- `Overdrive (O)`: pushes stronger separation
- `Min Saturation (MinSat)`: minimum saturation floor in `0..1`
- `Saturation Boost (SatB)`: extra saturation boost in `0..1`

## Outputs

- `Colors (Col)`: colors aligned to the input order
- `RGB`: aligned RGB strings in `r,g,b` format
- `Set`: distinct values in first-occurrence order
- `Group Colors (Grp)`: one branch per distinct value, containing that group palette

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

## Notes

- Same values plus same seed will always return the same palette.
- Changing only `Seed (S)` reshuffles group hues deterministically.
- Duplicates do not reuse exactly the same color; they vary within the same group family.
- Use `PaletteEngine Harness` for quick on-canvas sample data, aligned sample geometry, and default settings.
- The harness now includes `Count (C)`, so you can request larger sample sets such as `48`, `96`, or more when evaluating palette separation.
- The harness geometry stays aligned 1:1 with the expanded value list, so it can be wired directly into `Preview Color by Value` for downstream preview checks.
