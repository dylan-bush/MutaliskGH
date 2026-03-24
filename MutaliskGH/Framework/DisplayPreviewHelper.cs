using Rhino.Display;
using Rhino.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace MutaliskGH.Framework
{
    internal static class DisplayPreviewHelper
    {
        public static BoundingBox GetClippingBox(IEnumerable<object> values)
        {
            BoundingBox clippingBox = BoundingBox.Empty;

            foreach (object value in values)
            {
                if (!TryGetBoundingBox(value, out BoundingBox bounds))
                {
                    continue;
                }

                clippingBox = clippingBox.IsValid
                    ? BoundingBox.Union(clippingBox, bounds)
                    : bounds;
            }

            return clippingBox;
        }

        public static void DrawViewportMeshes(DisplayPipeline display, IEnumerable<object> values, Color color)
        {
            DisplayMaterial material = new DisplayMaterial(color, 0.15);

            foreach (object value in values)
            {
                DrawViewportMeshes(display, value, material);
            }
        }

        public static void DrawViewportWires(DisplayPipeline display, IEnumerable<object> values, Color color)
        {
            foreach (object value in values)
            {
                DrawViewportWires(display, value, color);
            }
        }

        private static bool TryGetBoundingBox(object value, out BoundingBox bounds)
        {
            bounds = BoundingBox.Empty;

            if (value == null)
            {
                return false;
            }

            switch (value)
            {
                case GeometryBase geometry:
                    bounds = geometry.GetBoundingBox(true);
                    return bounds.IsValid;
                case Point3d point:
                    bounds = new BoundingBox(point, point);
                    return true;
                case Line line:
                    bounds = new BoundingBox(line.From, line.To);
                    return true;
                case Polyline polyline:
                    bounds = polyline.BoundingBox;
                    return bounds.IsValid;
                case Arc arc:
                    bounds = arc.ToNurbsCurve().GetBoundingBox(true);
                    return bounds.IsValid;
                case Circle circle:
                    bounds = circle.ToNurbsCurve().GetBoundingBox(true);
                    return bounds.IsValid;
                case Box box:
                    bounds = box.BoundingBox;
                    return bounds.IsValid;
                case Rectangle3d rectangle:
                    bounds = new PolylineCurve(rectangle.ToPolyline()).GetBoundingBox(true);
                    return bounds.IsValid;
                default:
                    return false;
            }
        }

        private static void DrawViewportMeshes(DisplayPipeline display, object value, DisplayMaterial material)
        {
            if (value == null)
            {
                return;
            }

            switch (value)
            {
                case Brep brep:
                    display.DrawBrepShaded(brep, material);
                    return;
                case Mesh mesh:
                    display.DrawMeshShaded(mesh, material);
                    return;
                case Extrusion extrusion:
                    display.DrawBrepShaded(extrusion.ToBrep(), material);
                    return;
                case Surface surface:
                    display.DrawBrepShaded(surface.ToBrep(), material);
                    return;
                case Box box:
                    display.DrawBrepShaded(box.ToBrep(), material);
                    return;
            }
        }

        private static void DrawViewportWires(DisplayPipeline display, object value, Color color)
        {
            if (value == null)
            {
                return;
            }

            switch (value)
            {
                case Curve curve:
                    display.DrawCurve(curve, color, 2);
                    return;
                case Brep brep:
                    display.DrawBrepWires(brep, color, 2);
                    return;
                case Mesh mesh:
                    display.DrawMeshWires(mesh, color, 2);
                    return;
                case Extrusion extrusion:
                    display.DrawBrepWires(extrusion.ToBrep(), color, 2);
                    return;
                case Surface surface:
                    display.DrawBrepWires(surface.ToBrep(), color, 2);
                    return;
                case Rhino.Geometry.Point point:
                    display.DrawPoint(point.Location, PointStyle.Simple, 3, color);
                    return;
                case Point3d point:
                    display.DrawPoint(point, PointStyle.Simple, 3, color);
                    return;
                case Line line:
                    display.DrawLine(line, color, 2);
                    return;
                case Polyline polyline:
                    display.DrawPolyline(polyline, color, 2);
                    return;
                case Arc arc:
                    display.DrawArc(arc, color, 2);
                    return;
                case Circle circle:
                    display.DrawCircle(circle, color, 2);
                    return;
                case Box box:
                    display.DrawBox(box, color, 2);
                    return;
                case Rectangle3d rectangle:
                    display.DrawPolyline(rectangle.ToPolyline(), color, 2);
                    return;
            }
        }
    }
}
