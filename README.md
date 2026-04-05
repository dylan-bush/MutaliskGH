# MutaliskGH

<p align="center">
  <img src="MutaliskGH/Resources/Icons/MutaliskGH.png" alt="MutaliskGH header art" width="320">
</p>

MutaliskGH is a compiled Grasshopper plugin focused on high-utility workflow components for text processing, data trees, formatting, display, geometry, Rhino interaction, and Rhino.Inside.Revit. The project packages a broad set of everyday production tools into a consistent C# plugin with reusable core logic, testing, and a shared visual language.

## What It Includes

- Text utilities for regex matching, filtering, replacement, and cleanup
- Data-tree tools for grouping, partitioning, duplicate analysis, and boolean conversion
- Formatting tools for plane serialization, code generation, and unit presentation
- Display tools for deterministic palette generation and colored viewport preview
- Geometry tools for point rounding, rectangle evaluation, oriented boxes, offsets, and curve trimming
- Rhino helpers for selection, metadata lookup, and AutoCAD bridging
- Rhino.Inside.Revit query and automation tools for element inspection, view analysis, and view export

## Component Families

### Mutalisk / Text

- `RegEx Escape`
- `Text Match Multiple`
- `Multiple RegEx Index`
- `RegEx Cull`
- `Basic Strip`
- `RegEx Text Replace`

### Mutalisk / Data

- `Convert to Boolean`
- `Return Duplicate Index`
- `Return Duplicate Quantity`
- `Integer Series`
- `Branch by Member`
- `Cull ENF`
- `Partition Branches`

### Mutalisk / Format

- `Serialize Plane`
- `Deserialize Plane`
- `Decimal In to Fractional Ft In`
- `Find Next Available Code`

### Mutalisk / Display

- `PaletteEngine`
- `Color by Branch`
- `Preview Color by Value`
- `PaletteEngine Harness`

### Mutalisk / Geometry

- `Round Points`
- `Rebuild Rectangle`
- `Oriented Bounding Box`
- `Offset Select`
- `Extend and Trim Curves`

### Mutalisk / Rhino

- `Reference Selected`
- `SelValue`
- `Get Group Membership`
- `Get Layertable`
- `Open ACAD File RO`

### Mutalisk / Revit Query

- `Get Parent Element`
- `Spot Elevation Reference`
- `Match Filter Elements`
- `View Range Brep`
- `Element Material Map`

### Mutalisk / Revit Automation

- `Rename Views`
- `Zoom Element`
- `Revit Views to PDF`
- `Revit Views to DWG`

## Product Notes

- Components appear under the `Mutalisk` Grasshopper tab with family-based subcategories.
- ZUI-expandable `||` lanes are supported where parallel stream workflows need them.
- `PaletteEngine`, `Color by Branch`, and `Preview Color by Value` share the same deterministic seed-driven display system.
- `Find Next Available Code` supports mixed fixed/searchable formats such as `{000###}` and literal wrappers such as `LVL-{000###}`.
- `Oriented Bounding Box` exposes clustered, mean, and length-weighted orientation strategies through `Method (M)`.
- Revit query outputs now attempt to emit Rhino.Inside.Revit-native Grasshopper goo when those runtime types are available.
- Revit automation tools are gated behind trigger inputs so they behave as deliberate actions rather than solve-time side effects.

## Project Structure

- `MutaliskGH`
  Grasshopper wrappers, component UI, icons, and viewport-preview behavior
- `MutaliskGH.Core`
  Reusable pure logic for text, data, display, geometry, Rhino, and Revit helpers
- `MutaliskGH.Tests`
  Fast automated coverage for core behavior outside Grasshopper
- `docs/`
  Product docs, usage notes, and planning references

## Getting Started

1. Build the plugin:
   `dotnet build MutaliskGH\MutaliskGH.csproj -f net48`
2. Run the tests:
   `dotnet test MutaliskGH.Tests\MutaliskGH.Tests.csproj`
3. Load the built `.gha` in Grasshopper, or deploy it to a Rhino.Inside.Revit libraries folder for Revit-hosted testing.

## Documentation

- [Docs Site](docs/index.html)
- [Component Catalog and Roadmap](docs/notion-migration-architecture.md)
- [PaletteEngine Guide](docs/palette-engine.md)
- [Revit Integration Notes](docs/revit-planning.md)
