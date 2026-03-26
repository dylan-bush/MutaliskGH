# MutaliskGH

MutaliskGH is a compiled Grasshopper plugin for migrating a curated Notion-documented library of custom utility components, tree tools, text tools, preview tools, geometry tools, and Rhino-interaction helpers into a maintainable C# codebase. The repo now contains the migration architecture memo, a reusable plugin framework, a shared pure-logic core library, an automated test project, and the implemented `Mutalisk / Text`, `Mutalisk / Data`, `Mutalisk / Format`, `Mutalisk / Display`, `Mutalisk / Geometry`, and first `Mutalisk / Rhino` slices.

## Features

- Single-plugin migration plan with `Mutalisk` category and family-based subcategories documented in `docs/notion-migration-architecture.md`
- Shared plugin framework including `BaseComponent`, `CategoryNames`, `IconLoader`, and Grasshopper value-unwrapping helpers
- Separate `MutaliskGH.Core` project for reusable logic and `MutaliskGH.Tests` for fast non-Grasshopper unit coverage
- Implemented `Mutalisk / Text` components:
  - `RegEx Escape`
  - `Text Match Multiple`
  - `Multiple RegEx Index`
  - `RegEx Cull`
  - `Basic Strip`
  - `RegEx Text Replace`
- Implemented `Mutalisk / Data` components:
  - `Convert to Boolean`
  - `Return Duplicate Index`
  - `Return Duplicate Quantity`
  - `Integer Series`
  - `Branch by Member`
  - `Cull ENF`
  - `Partition Branches`
- Implemented `Mutalisk / Format` components:
  - `Serialize Plane`
  - `Deserialize Plane`
  - `Decimal In to Fractional Ft In`
  - `Find Next Available Code`
- Implemented `Mutalisk / Display` components:
  - `Color by Branch`
  - `Preview Color by Value`
- Implemented `Mutalisk / Geometry` components:
  - `Round Points`
  - `Rebuild Rectangle`
  - `Oriented Bounding Box`
  - `Offset Select`
  - `Extend and Trim Curves`
- Implemented `Mutalisk / Rhino` components:
  - `Reference Selected`
  - `SelValue`
- ZUI-expandable parallel stream support on components that need it, including `RegEx Cull`, `Branch by Member`, and `Cull ENF`
- Flexible code-format search support in `Find Next Available Code`, including original-style fixed slots via patterns like `{000###}`
- Multiple orientation strategies in `Oriented Bounding Box`, including clustered edge directions, mean direction, and length-weighted mean
- Built-in colored preview on `Color by Branch` and `Preview Color by Value`, with branch-grouped color output
- Rhino-side interaction helpers for prompting object selection and running repeated `SelValue` selections from Grasshopper triggers
- Multi-target Grasshopper build setup for Rhino 8-compatible `.gha` output

## Getting Started

1. Open the repository in Visual Studio or VS Code with the .NET SDK installed.
2. Build the plugin with `dotnet build MutaliskGH\MutaliskGH.csproj -f net48`.
3. Run the pure-logic tests with `dotnet test MutaliskGH.Tests\MutaliskGH.Tests.csproj`.
4. Launch Rhino 8 with Grasshopper and load the local `.gha` if you want to test the current components interactively.
5. Review `docs/notion-migration-architecture.md` before starting a new migration slice.

## Controls

- Grasshopper components appear under the `Mutalisk` tab and currently populate the `Text`, `Data`, `Format`, `Display`, `Geometry`, and `Rhino` subcategories.
- Components that use ZUI can be expanded with Grasshopper zoom controls to add additional `||` lanes.
- `Partition Branches` uses a branch-selection pattern in `P`; flat and grafted pattern inputs are both supported when they provide one decision per branch.
- `Find Next Available Code` accepts a `Format` input. Use `{000###}` for the original behavior, `{000000}` for a fully searchable 6-digit code, or literal wrappers such as `LVL-{000###}`.
- `Oriented Bounding Box` accepts `Method (M)`: `0` for clustered edge directions, `1` for mean direction, and `2` for length-weighted mean.
- `Color by Branch` outputs a grafted `Col` tree with one color per source branch and now previews compatible geometry in those branch colors.
- `Preview Color by Value` keeps all paired items, groups geometry and values by distinct value, outputs branch-colored trees, and previews the geometry using the generated colors. Both display components use `S` for the seed input.
- `Rebuild Rectangle` preserves the source rectangle geometry and returns ordered vertices starting at bottom-left and continuing clockwise.
- `Round Points` uses `Factor (F)` as the coordinate-rounding control.
- `Offset Select` returns the selected offset curve or ordered pair of offset curves through `Selected Offset (O)`.
- `Extend and Trim Curves` outputs extended curves even when no trim occurs and supports `Trim Single (T)` to control single-hit trimming behavior.
- `Reference Selected` stores the last chosen Rhino object IDs until triggered again, and `SelValue` now accepts a list of strings and runs one Rhino selection command per value on a rising-edge toggle.
- Host-side-effect tools such as file-open and Revit export workflows are still expected to remain script or workflow driven unless explicitly migrated later.

## Documentation Site

- A GitHub Pages-ready static docs site lives in `docs/`.
- The landing page is `docs/index.html`.
- The current site includes the project overview, implemented component families, framework structure, current interaction notes, and links back to the migration memo and README.
- To publish it with GitHub Pages, set the repository Pages source to the `main` branch and the `/docs` folder.
