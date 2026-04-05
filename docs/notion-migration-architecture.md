# Mutalisk Notion Migration Architecture

## Purpose

This memo translates the Notion export at `C:\Users\bushd\Downloads\GH Library Database Notion Export` into a practical migration structure for a single mixed Grasshopper plugin with top-level category `Mutalisk`.

The export was validated against:

- the CSV inventory
- representative page exports for `Text Match Multiple`, `Branch by Member`, `RiR_ViewRangeBrep`, `Open ACAD File RO`, and `Batch Revit-to-NWC Export`
- the ShapeDiver Grasshopper/Rhino plugin template at `https://github.com/shapediver/GrasshopperPluginTemplate` for plugin-layout best-practice reference

This memo assumes the plugin will eventually follow the same broad separation of concerns used in the template:

- shared pure logic helpers
- Rhino/Grasshopper adapters
- Revit/Rhino.Inside adapters
- tests around the shared layers first

## Current Implementation Status

The repo has now moved beyond the original MVP baseline and currently includes these compiled families and slices:

- `Mutalisk / Text`: `RegEx Escape`, `Text Match Multiple`, `Multiple RegEx Index`, `RegEx Cull`, `Basic Strip`, `RegEx Text Replace`
- `Mutalisk / Data`: `Convert to Boolean`, `Return Duplicate Index`, `Return Duplicate Quantity`, `Integer Series`, `Branch by Member`, `Cull ENF`, `Partition Branches`
- `Mutalisk / Format`: `Serialize Plane`, `Deserialize Plane`, `Decimal In to Fractional Ft In`, `Find Next Available Code`
- `Mutalisk / Display`: `PaletteEngine`, `Color by Branch`, `Preview Color by Value`
- `Mutalisk / Geometry`: `Round Points`, `Rebuild Rectangle`, `Oriented Bounding Box`, `Offset Select`, `Extend and Trim Curves`
- `Mutalisk / Rhino`: `Reference Selected`, `SelValue`, `Get Group Membership`, `Get Layertable`

Recent geometry-specific progress:

- `Oriented Bounding Box` now supports 3 orientation strategies through `Method (M)`: clustered edge directions, mean direction, and length-weighted mean
- `Rebuild Rectangle` now evaluates source rectangle geometry and returns ordered vertices and edges rather than reconstructing from width and height alone
- `Offset Select` is the current user-facing name for the former `Offset Larger-Smaller`
- `Extend and Trim Curves` now keeps extension geometry even when no trim occurs and exposes `Trim Single (T)` for single-hit trim control

Recent display and Rhino-specific progress:

- `PaletteEngine` now has a compiled wrapper that exposes deterministic grouped palettes, aligned colors, RGB strings, and grouped branch palettes directly in Grasshopper
- `Preview Color by Value` now keeps all paired items and groups them by distinct value instead of dropping false-like values
- `Reference Selected` stores selected Rhino object IDs between triggers
- `SelValue` now accepts a list of strings and runs one Rhino selection command per value on a rising-edge trigger
- `Get Group Membership` now accepts referenced Rhino geometry directly and falls back to GUID parsing when needed
- `Get Layertable` now returns active Rhino layer `FullPath` values as a compiled component
- the plugin category icon is now registered through `GH_AssemblyPriority`, so the `Mutalisk` tab icon can differ from per-component icons

## Scope And Normalization

- CSV rows reviewed: `42`
- Unique conceptual tools after normalization: `41`
- Normalization rule applied: the duplicated `Text Match Multiple` CSV rows are treated as one conceptual component because both page exports describe the same `TMM` behavior and IO shape
- Plugin scope: one mixed plugin, but Revit-dependent work is grouped late and isolated by subcategory
- Workflow rule: existing `Target Implementation = Workflow` was kept, and additional tools were marked workflow/script when their value depends primarily on host-state or file-export side effects

## Family Groups

### Family 1

- family name: `Mutalisk / Data`
- included components: `Cull Empty Null or False Branches`, `Return Duplicate Index`, `Return Duplicate Quantity`, `Test Null or Text-Length-0`, `Branch by Member`, `Partition Branches`, `Integer Series`
- shared logic/core helpers: branch traversal, duplicate detection, boolean/null filtering, tree/list index mapping, series generation
- suggested plugin category/subcategory: `Mutalisk / Data`
- recommended migration order: `1`
- which tools remain workflows/scripts: `None`
- MVP membership: `Cull Empty Null or False Branches`, `Return Duplicate Index`, `Return Duplicate Quantity`, `Test Null or Text-Length-0`, `Branch by Member`
- rationale: dependency depth is low, side-effect risk is low, and helper reuse is high across later text, display, and geometry work

### Family 2

- family name: `Mutalisk / Text`
- included components: `RegEx Escape`, `Basic Strip`, `RegEx Text Replace`, `Text Match Multiple`, `Multiple RegEx Index`, `RegEx Cull`
- shared logic/core helpers: text normalization, regex pattern compilation and caching, exact-vs-raw match mode, index/result-set helpers
- suggested plugin category/subcategory: `Mutalisk / Text`
- recommended migration order: `2`
- which tools remain workflows/scripts: `None`
- MVP membership: `RegEx Escape`, `Text Match Multiple`, `RegEx Cull`
- rationale: dependency depth is low, side-effect risk is low, and helper reuse is high for data filtering and formatting components

### Family 3

- family name: `Mutalisk / Format`
- included components: `Decimal In to Fractional Ft In`, `Find Next Available Code`, `Serialize Plane`, `Deserialize Plane`
- shared logic/core helpers: string formatting, code incrementation, plane serialization/parsing, validation and fallback parsing
- suggested plugin category/subcategory: `Mutalisk / Format`
- recommended migration order: `2`
- which tools remain workflows/scripts: `None`
- MVP membership: `Decimal In to Fractional Ft In`, `Serialize Plane`, `Deserialize Plane`
- rationale: dependency depth is low, side-effect risk is low, and helper reuse is high for user-facing formatting, serialization, and code-generation flows

### Family 4

- family name: `Mutalisk / Geometry`
- included components: `Oriented Bounding Box`, `Offset Select`, `Rebuild Rectangle`, `Round Points`, `Extend and Trim Curves`
- shared logic/core helpers: tolerance handling, plane/box utilities, curve extension and trim wrappers, point rounding helpers
- suggested plugin category/subcategory: `Mutalisk / Geometry`
- recommended migration order: `3`
- which tools remain workflows/scripts: `None`
- MVP membership: `Round Points`, `Rebuild Rectangle`
- rationale: dependency depth is moderate, side-effect risk is low, and helper reuse is good, but a few components need stronger geometry abstractions before migration

### Family 5

- family name: `Mutalisk / Rhino`
- included components: `Reference Selected`, `SelValue`, `Get Group Membership`, `Get Layertable`, `Open ACAD File RO`
- shared logic/core helpers: Rhino document access, selection lookup, object/group/layer query wrappers, file and application guards
- suggested plugin category/subcategory: `Mutalisk / Rhino`
- recommended migration order: `4`
- which tools remain workflows/scripts: `None`
- MVP membership: `None`
- rationale: dependency depth is moderate because these depend on Rhino context, side-effect risk is mixed, and helper reuse is good once document adapters exist

### Family 6

- family name: `Mutalisk / Display`
- included components: `PaletteEngine`, `Color by Branch`, `Preview Color by Value`
- shared logic/core helpers: palette interpolation, value-to-color mapping, preview material and attribute adapters
- suggested plugin category/subcategory: `Mutalisk / Display`
- recommended migration order: `4`
- which tools remain workflows/scripts: `None`
- MVP membership: `Color by Branch`, `Preview Color by Value`
- rationale: dependency depth is moderate, side-effect risk is low, and helper reuse is strong once core data and color-mapping utilities are in place

### Family 7

- family name: `Mutalisk / Revit Query`
- included components: `RiR_ViewRangeBrep`, `RiR_GetParentElement`, `RiR_SpotElevationReference`, `RiR_MatchFilterElements`, `RiR_ElementMaterialMap-2023`, `RiR_ElementMaterialMap-2025`
- shared logic/core helpers: Rhino.Inside session guard, Revit element and view adapters, geometry extraction helpers, version-specific API shims
- suggested plugin category/subcategory: `Mutalisk / Revit Query`, `Mutalisk / Revit Geometry`
- recommended migration order: `5`
- which tools remain workflows/scripts: `None by default`
- MVP membership: `None`
- rationale: dependency depth is high because of Rhino.Inside and Revit API requirements, side-effect risk is lower than automation work, and helper reuse is high inside the Revit slice once the session and adapter layers exist

Compatibility note for material maps:

- `RiR_ElementMaterialMap-2023` and `RiR_ElementMaterialMap-2025` should share one abstraction and one family owner
- keep separate compatibility notes and API shims, not separate architectural families

Current repo note:

- the repo now implements this as one consolidated `Element Material Map` component rather than separate 2023 and 2025 user-facing components

### Family 8

- family name: `Mutalisk / Revit Automation`
- included components: `RiR_RevitViewsToDWG`, `RiR_RevitViewsToPDF`, `RiR_Rename Views`, `RiR_ZoomElement`, `Batch Revit-to-NWC Export`
- shared logic/core helpers: transaction runner, export path and file naming policy, batch reporting, host-state validation, UI-side-effect wrappers
- suggested plugin category/subcategory: `Mutalisk / Revit Automation`
- recommended migration order: `6`
- which tools remain workflows/scripts: `RiR_RevitViewsToDWG`, `RiR_RevitViewsToPDF`, `RiR_Rename Views`, `RiR_ZoomElement`, `Batch Revit-to-NWC Export`
- MVP membership: `None`
- rationale: dependency depth is highest, side-effect risk is highest, and helper reuse is real but mainly valuable after the Revit query and host-control layers are already stable

## Proposed MVP Batch

The MVP optimizes for broad utility coverage, shared helper creation, and minimal external dependency risk.

- `RegEx Escape`
- `Text Match Multiple`
- `RegEx Cull`
- `Cull Empty Null or False Branches`
- `Return Duplicate Index`
- `Return Duplicate Quantity`
- `Test Null or Text-Length-0`
- `Branch by Member`
- `Decimal In to Fractional Ft In`
- `Serialize Plane`
- `Deserialize Plane`
- `Round Points`
- `Rebuild Rectangle`
- `Color by Branch`
- `Preview Color by Value`

Explicit MVP exclusions:

- all Revit automation work
- `Open ACAD File RO`
- `Oriented Bounding Box`
- `RiR_ElementMaterialMap-2023`
- `RiR_ElementMaterialMap-2025`

## Execution Slice 1

The first implementation family is locked to `Mutalisk / Text`.

- first family: `Mutalisk / Text`
- first batch: `RegEx Escape`, `Text Match Multiple`, `Multiple RegEx Index`, `RegEx Cull`
- reason: related text/regex behavior, low-to-medium complexity, no Revit write operations, and strong shared-helper reuse
- preferred golden component for step 4: `RegEx Escape`
- step 3 scaffold only: `BaseComponent`, `CategoryNames`, `Result<T>`, `IconLoader`

## Migration Sequence

1. Build shared data-tree, index, and duplicate helpers under `Mutalisk / Data`
2. Build shared text and serialization helpers under `Mutalisk / Text` and `Mutalisk / Format`
3. Add low-risk geometry wrappers under `Mutalisk / Geometry`
4. Add Rhino document adapters and display helpers under `Mutalisk / Rhino` and `Mutalisk / Display`
5. Add Revit session guards, adapters, and query components under `Mutalisk / Revit Query`
6. Revisit automation-heavy Revit exports and host-control tasks as workflows first, not compiled components

## Validation

### Coverage

All normalized tools are assigned exactly once to a primary family:

- `Mutalisk / Data`: `Cull Empty Null or False Branches`, `Return Duplicate Index`, `Return Duplicate Quantity`, `Test Null or Text-Length-0`, `Branch by Member`, `Partition Branches`, `Integer Series`
- `Mutalisk / Text`: `RegEx Escape`, `Basic Strip`, `RegEx Text Replace`, `Text Match Multiple`, `Multiple RegEx Index`, `RegEx Cull`
- `Mutalisk / Format`: `Decimal In to Fractional Ft In`, `Find Next Available Code`, `Serialize Plane`, `Deserialize Plane`
- `Mutalisk / Geometry`: `Oriented Bounding Box`, `Offset Select`, `Rebuild Rectangle`, `Round Points`, `Extend and Trim Curves`
- `Mutalisk / Rhino`: `Reference Selected`, `SelValue`, `Get Group Membership`, `Get Layertable`, `Open ACAD File RO`
- `Mutalisk / Display`: `PaletteEngine`, `Color by Branch`, `Preview Color by Value`
- `Mutalisk / Revit Query`: `RiR_ViewRangeBrep`, `RiR_GetParentElement`, `RiR_SpotElevationReference`, `RiR_MatchFilterElements`, `RiR_ElementMaterialMap-2023`, `RiR_ElementMaterialMap-2025`
- `Mutalisk / Revit Automation`: `RiR_RevitViewsToDWG`, `RiR_RevitViewsToPDF`, `RiR_Rename Views`, `RiR_ZoomElement`, `Batch Revit-to-NWC Export`

### Workflow Decisions

Workflow or script recommendations are justified by side effects:

- `Batch Revit-to-NWC Export` is already marked `Workflow` in the CSV and is export-pipeline oriented
- `RiR_RevitViewsToDWG` and `RiR_RevitViewsToPDF` are file-export tasks with external side effects
- `RiR_Rename Views` and `RiR_ZoomElement` directly modify or control the Revit host environment
- `Open ACAD File RO` opens and coordinates an external AutoCAD instance and therefore behaves more like a workflow bridge than a pure Grasshopper utility component

Current repo note:

- despite the original recommendation, the repo now includes compiled wrappers for `RiR_RevitViewsToDWG`, `RiR_RevitViewsToPDF`, `RiR_Rename Views`, `RiR_ZoomElement`, and `Open ACAD File RO`
- the current mitigation is to keep all of them explicitly trigger-driven so they act as deliberate workflow helpers rather than always-live utility nodes

### Assumptions

- top-level category remains `Mutalisk`
- one mixed plugin is acceptable, but Revit work stays late in the migration order
- the eventual codebase should split shared logic from Grasshopper, Rhino, and Revit integration layers in line with the ShapeDiver template style
- if future page-export review reveals the two `Text Match Multiple` exports are materially different, split them at implementation time, not in this grouping memo
