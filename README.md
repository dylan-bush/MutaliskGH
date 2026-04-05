# MutaliskGH

<p align="center">
  <img src="MutaliskGH/Resources/Icons/MutaliskGH.png" alt="MutaliskGH header art" width="320">
</p>

MutaliskGH is a compiled Grasshopper plugin for migrating a curated Notion-documented library of custom utility components, tree tools, text tools, preview tools, geometry tools, Rhino-interaction helpers, and Rhino.Inside.Revit tools into a maintainable C# codebase. The repo now contains the migration architecture memo, a reusable plugin framework, a shared pure-logic core library, an automated test project, and the implemented `Mutalisk / Text`, `Mutalisk / Data`, `Mutalisk / Format`, `Mutalisk / Display`, `Mutalisk / Geometry`, `Mutalisk / Rhino`, `Mutalisk / Revit Query`, and first `Mutalisk / Revit Automation` slices.

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
  - `PaletteEngine`
  - `Color by Branch`
  - `Preview Color by Value`
  - `PaletteEngine Harness`
- Implemented `Mutalisk / Geometry` components:
  - `Round Points`
  - `Rebuild Rectangle`
  - `Oriented Bounding Box`
  - `Offset Select`
  - `Extend and Trim Curves`
- Implemented `Mutalisk / Rhino` components:
  - `Reference Selected`
  - `SelValue`
  - `Get Group Membership`
  - `Get Layertable`
  - `Open ACAD File RO`
- Implemented `Mutalisk / Revit Query` components:
  - `Get Parent Element`
  - `Spot Elevation Reference`
  - `Match Filter Elements`
  - `View Range Brep`
  - `Element Material Map`
- Implemented `Mutalisk / Revit Automation` components:
  - `Rename Views`
  - `Zoom Element`
  - `Revit Views to PDF`
  - `Revit Views to DWG`
- ZUI-expandable parallel stream support on components that need it, including `RegEx Cull`, `Branch by Member`, and `Cull ENF`
- Flexible code-format search support in `Find Next Available Code`, including original-style fixed slots via patterns like `{000###}`
- Multiple orientation strategies in `Oriented Bounding Box`, including clustered edge directions, mean direction, and length-weighted mean
- Built-in colored preview on `Color by Branch` and `Preview Color by Value`, with branch-grouped color output
- `PaletteEngine Harness` for high-count sample values, aligned geometry, and canned palette presets when testing the display slice
- Rhino-side interaction helpers for prompting object selection, querying layer/group metadata, and running repeated `SelValue` selections from Grasshopper triggers
- Plugin-level category icon registration so the `Mutalisk` tab icon can be controlled independently from per-component icons
- Multi-target Grasshopper build setup for Rhino 8-compatible `.gha` output

## Getting Started

1. Open the repository in Visual Studio or VS Code with the .NET SDK installed.
2. Build the plugin with `dotnet build MutaliskGH\MutaliskGH.csproj -f net48`.
3. Run the pure-logic tests with `dotnet test MutaliskGH.Tests\MutaliskGH.Tests.csproj`.
4. Launch Rhino 8 with Grasshopper and load the local `.gha` if you want to test the current components interactively.
5. Review `docs/notion-migration-architecture.md` before starting a new migration slice.

## Controls

- Grasshopper components appear under the `Mutalisk` tab and currently populate the `Text`, `Data`, `Format`, `Display`, `Geometry`, `Rhino`, `Revit Query`, and `Revit Automation` subcategories.
- Components that use ZUI can be expanded with Grasshopper zoom controls to add additional `||` lanes.
- `RegEx Cull` now still computes when only `L` and `Re` are connected; the main `||` output falls back to the test list when no explicit primary parallel stream is supplied.
- `Partition Branches` uses a branch-selection pattern in `P`; flat and grafted pattern inputs are both supported when they provide one decision per branch.
- `Find Next Available Code` accepts a `Format` input. Use `{000###}` for the original behavior, `{000000}` for a fully searchable 6-digit code, or literal wrappers such as `LVL-{000###}`.
- `Oriented Bounding Box` accepts `Method (M)`: `0` for clustered edge directions, `1` for mean direction, and `2` for length-weighted mean.
- `Color by Branch` outputs a grafted `Col` tree with one color per source branch and now previews compatible geometry in those branch colors.
- `PaletteEngine` exposes the shared deterministic palette generator directly and returns aligned colors, aligned RGB strings, the distinct label set, and grouped branch palettes.
- `PaletteEngine Harness` emits matching sample geometry and values, plus default palette settings and a scalable `Count (C)` input for denser preview testing.
- `Preview Color by Value` keeps all paired items, groups geometry and values by distinct value, outputs branch-colored trees, and previews the geometry using the generated colors. Both display components use `S` for the seed input.
- `Rebuild Rectangle` preserves the source rectangle geometry and returns ordered vertices starting at bottom-left and continuing clockwise.
- `Round Points` uses `Factor (F)` as the coordinate-rounding control.
- `Offset Select` returns the selected offset curve or ordered pair of offset curves through `Selected Offset (O)`.
- `Extend and Trim Curves` outputs extended curves even when no trim occurs and supports `Trim Single (T)` to control single-hit trimming behavior.
- `Reference Selected` stores the last chosen Rhino object IDs until triggered again, `SelValue` accepts a list of strings and runs one Rhino selection command per value on a rising-edge toggle, `Get Group Membership` accepts either referenced Rhino geometry or GUIDs, and `Get Layertable` returns active Rhino layer full paths.
- `Open ACAD File RO` validates a `.dwg` path, reuses a running AutoCAD instance when possible, and opens the file read-only on a rising-edge trigger.
- `Get Parent Element`, `Spot Elevation Reference`, `Match Filter Elements`, `View Range Brep`, and `Element Material Map` are reflection-based Revit wrappers intended for Rhino.Inside.Revit runtime objects without adding hard compile-time Revit API references to the plugin project.
- `Get Parent Element` and `Spot Elevation Reference` now emit RiR-native Grasshopper goo when Rhino.Inside.Revit GH types are available, so their outputs can feed downstream RiR components directly.
- `Rename Views`, `Zoom Element`, `Revit Views to PDF`, and `Revit Views to DWG` are compiled but intentionally gated behind rising-edge `Run` inputs so they behave as deliberate automation actions instead of firing on every solve.
- `Batch Revit-to-NWC Export` is still being treated as a workflow candidate rather than a compiled component.

## Documentation Site

- A GitHub Pages-ready static docs site lives in `docs/`.
- The landing page is `docs/index.html`.
- The current site includes the project overview, implemented component families, framework structure, current interaction notes, and links back to the migration memo and README.
- To publish it with GitHub Pages, set the repository Pages source to the `main` branch and the `/docs` folder.
