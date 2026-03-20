# MutaliskGH

MutaliskGH is a Grasshopper plugin project for migrating a curated library of custom utility components, Rhino interaction tools, visualization helpers, and Rhino.Inside/Revit workflows into a maintainable compiled plugin. The current repo contains the initial plugin scaffold, migration architecture documentation, and the shared framework pieces that will support a staged component-by-component migration.

## Features

- Single-plugin migration plan with `Mutalisk` category and family-based subcategories
- Documented component-family architecture for Text, Data, Format, Geometry, Rhino, Display, and Revit slices
- Shared framework scaffold for future components: `BaseComponent`, `CategoryNames`, `Result<T>`, and `IconLoader`
- Multi-target Grasshopper build setup for Rhino 8-compatible `.gha` output

## Getting Started

1. Open the repository in Visual Studio or VS Code with the .NET SDK installed.
2. Build the solution with `dotnet build MutaliskGH.slnx`.
3. Launch Rhino 8 with Grasshopper using the included debug profiles if you want to load the local plugin build.
4. Review the migration memo in `docs/notion-migration-architecture.md` before implementing new component families.

## Controls

- Grasshopper components in this plugin will appear under the `Mutalisk` tab, grouped by family subcategory.
- Interactive or host-side-effect tools such as file-open or Revit export workflows are planned to stay script or workflow driven unless explicitly migrated later.
- The first implementation family is `Mutalisk / Text`, with `RegEx Escape` selected as the initial golden component.
