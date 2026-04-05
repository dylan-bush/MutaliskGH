# Revit Planning Notes

This note captures the current Revit-specific implementation direction for `MutaliskGH` and the constraints we should keep in mind as the `Mutalisk / Revit Query` family grows.

## Current Decision

- Keep moving forward with the current repo structure for now.
- Continue implementing the first Revit components inside the existing plugin/codebase.
- Use the current reflection-based runtime access approach as the short-term path of least resistance.

## Why We Are Doing That

- It lets us keep momentum without immediately restructuring the plugin into separate Rhino-only and Revit-only assemblies.
- It avoids adding hard compile-time Revit references before the Revit slice is large enough to justify the packaging work.
- It keeps the first migration passes testable with fake-object logic tests in `MutaliskGH.Tests`.

## Rhino.Inside.Revit Findings

Source:
- https://www.rhino3d.com/inside/revit/1.0/reference/rir-plugins

Relevant guidance from the Rhino.Inside.Revit docs:
- For official plugin development, Revit API libraries should be referenced in the plugin project.
- `RhinoInside.Revit.dll` can be referenced directly from the Rhino.Inside.Revit install location because there is no NuGet package for it.
- Revit context is expected to come from the `RhinoInside.Revit.Revit` static type.
- Revit-dependent Grasshopper plugins should be installed in Rhino.Inside.Revit-specific library folders so they only load inside that environment.

## Practical Implications For MutaliskGH

- The current reflection-based Revit components are acceptable as an early implementation strategy, but they are not the long-term official integration model.
- If the Revit slice continues to grow, we should expect to move toward typed access through:
  - Revit API assemblies
  - `RhinoInside.Revit.dll`
  - `RhinoInside.Revit.Revit.ActiveUIApplication`
  - `RhinoInside.Revit.Revit.ActiveUIDocument`
  - `RhinoInside.Revit.Revit.ActiveDBDocument`
- Revit-dependent `.gha` deployment should eventually be separated from the normal Grasshopper library deployment path.

## Deployment Note

The Rhino.Inside.Revit docs recommend Revit-only plugin deployment under:

- `%PROGRAMDATA%\\Grasshopper\\Libraries-Inside-Revit-20XX`
- `%APPDATA%\\Grasshopper\\Libraries-Inside-Revit-20XX`

This matters because Revit-dependent plugins may fail to load in standalone Rhino if they are deployed like a normal Grasshopper plugin.

## Current Risk

With the current single-plugin structure:

- Revit components may remain harder to package cleanly than the Rhino-only families.
- Reflection keeps compile-time friction low, but it also reduces type safety and may be more brittle than a typed Rhino.Inside.Revit integration.
- The packaging boundary between normal Grasshopper use and Rhino.Inside.Revit use is still unresolved.

## Recommended Future Upgrade Path

When the Revit family becomes large enough, revisit the architecture and likely do one or more of these:

1. Split Revit-specific components into a separate Revit-only `.gha`.
2. Add typed references to Revit API and `RhinoInside.Revit.dll`.
3. Replace reflection-heavy access paths with typed Rhino.Inside.Revit context access.
4. Document Revit-specific install/deployment instructions separately from the normal plugin instructions.

## Current Revit Components

Implemented so far:

- `Get Parent Element`
- `Spot Elevation Reference`
- `Match Filter Elements`
- `View Range Brep`
- `Element Material Map`
- `Rename Views`
- `Zoom Element`
- `Revit Views to PDF`
- `Revit Views to DWG`

These currently use reflection-based runtime inspection and fake-object unit tests rather than direct Revit API compile-time bindings.

## Current Wrapper Direction

- Revit query outputs now attempt to wrap results as Rhino.Inside.Revit-native Grasshopper goo when those GH types are available at runtime.
- This is the current compromise between:
  - keeping the plugin build free of hard Revit/Rhino.Inside compile-time references
  - making the component outputs usable directly in downstream RiR components

## Current Automation Direction

- Some Revit automation tools are now compiled despite the original migration memo preferring workflows first.
- The current mitigation is behavioral, not architectural:
  - all compiled automation tools are gated behind rising-edge `Run` triggers
  - exports and rename actions do not fire on every solve
- `Batch Revit-to-NWC Export` still looks better suited to a workflow tool than a normal always-loaded Grasshopper utility component.
