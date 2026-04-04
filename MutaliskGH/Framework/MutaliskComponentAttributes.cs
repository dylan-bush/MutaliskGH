using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;

namespace MutaliskGH.Framework
{
    internal sealed class MutaliskComponentAttributes : GH_ComponentAttributes
    {
        private static readonly Color ComponentFill = ColorTranslator.FromHtml("#87607b");
        private static readonly Color ComponentEdge = Darken(ComponentFill, 0.28f);
        private static readonly Color ComponentText = Color.FromArgb(248, 242, 247);
        private static readonly GH_PaletteStyle StandardStyle = new GH_PaletteStyle(ComponentFill, ComponentEdge, ComponentText);

        public MutaliskComponentAttributes(IGH_Component owner)
            : base(owner)
        {
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            if (channel != GH_CanvasChannel.Objects ||
                Owner == null ||
                Selected ||
                Owner.Hidden ||
                Owner.Locked ||
                Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Warning ||
                Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error)
            {
                base.Render(canvas, graphics, channel);
                return;
            }

            using (GH_Capsule capsule = GH_Capsule.CreateCapsule(Bounds, GH_Palette.Normal))
            {
                capsule.Render(graphics, StandardStyle);
            }

            RenderComponentCapsule(
                canvas,
                graphics,
                false,
                true,
                true,
                true,
                true,
                true);
        }

        private static Color Darken(Color color, float amount)
        {
            amount = Math.Max(0f, Math.Min(1f, amount));
            return Color.FromArgb(
                color.A,
                (int)Math.Round(color.R * (1f - amount)),
                (int)Math.Round(color.G * (1f - amount)),
                (int)Math.Round(color.B * (1f - amount)));
        }
    }
}
