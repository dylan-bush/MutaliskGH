# MutaliskGH

MutaliskGH is a compiled Grasshopper plugin for migrating a curated Notion-documented library of custom utility components, tree tools, text tools, and workflow helpers into a maintainable C# codebase. The repo now contains the migration architecture memo, a reusable plugin framework, a shared pure-logic core library, an automated test project, and the first implemented `Mutalisk / Text` and `Mutalisk / Data` component slices.

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
- ZUI-expandable parallel stream support on components that need it, including `RegEx Cull`, `Branch by Member`, and `Cull ENF`
- Multi-target Grasshopper build setup for Rhino 8-compatible `.gha` output

## Getting Started

1. Open the repository in Visual Studio or VS Code with the .NET SDK installed.
2. Build the plugin with `dotnet build MutaliskGH\MutaliskGH.csproj -f net48`.
3. Run the pure-logic tests with `dotnet test MutaliskGH.Tests\MutaliskGH.Tests.csproj`.
4. Launch Rhino 8 with Grasshopper and load the local `.gha` if you want to test the current components interactively.
5. Review `docs/notion-migration-architecture.md` before starting a new migration slice.

## Controls

- Grasshopper components appear under the `Mutalisk` tab and currently populate the `Text` and `Data` subcategories.
- Components that use ZUI can be expanded with Grasshopper zoom controls to add additional `||` lanes.
- `Partition Branches` uses a branch-selection pattern in `P`; flat and grafted pattern inputs are both supported when they provide one decision per branch.
- Host-side-effect tools such as file-open and Revit export workflows are still expected to remain script or workflow driven unless explicitly migrated later.
