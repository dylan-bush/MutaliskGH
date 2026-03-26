using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Format;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;

namespace MutaliskGH.Components.Format
{
    public sealed class DeserializePlaneComponent : BaseComponent
    {
        public DeserializePlaneComponent()
            : base(
                "Deserialize Plane",
                "DesPln",
                "Convert serialized O...X...Y...Z... text back into a plane.",
                CategoryNames.Format)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("9a8d1a6e-f9da-40ae-b7e7-66ffbb2b438f"); }
        }

        protected override string IconResourceName
        {
            get { return "DeserializePlane.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "Txt",
                "Serialized plane text.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddPlaneParameter(
                "Plane",
                "Pln",
                "Plane reconstructed from serialized text.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string text = null;
            if (!dataAccess.GetData(0, ref text))
            {
                return;
            }

            Result<PlaneValue> result = PlaneSerializationLogic.Deserialize(text);
            if (ReportFailure(result))
            {
                return;
            }

            Result<Plane> planeResult = ToRhinoPlane(result.Value);
            if (ReportFailure(planeResult))
            {
                return;
            }

            dataAccess.SetData(0, planeResult.Value);
        }

        private static Result<Plane> ToRhinoPlane(PlaneValue value)
        {
            Point3d origin = new Point3d(value.Origin.X, value.Origin.Y, value.Origin.Z);
            Vector3d xAxis = new Vector3d(value.XAxis.X, value.XAxis.Y, value.XAxis.Z);
            Vector3d yAxis = new Vector3d(value.YAxis.X, value.YAxis.Y, value.YAxis.Z);
            Vector3d zAxis = new Vector3d(value.ZAxis.X, value.ZAxis.Y, value.ZAxis.Z);

            if (!xAxis.Unitize())
            {
                return Result<Plane>.Failure("Serialized plane X axis must be non-zero.");
            }

            if (!yAxis.Unitize())
            {
                return Result<Plane>.Failure("Serialized plane Y axis must be non-zero.");
            }

            if (!zAxis.Unitize())
            {
                return Result<Plane>.Failure("Serialized plane Z axis must be non-zero.");
            }

            Plane plane = new Plane(origin, xAxis, yAxis);
            if (!plane.IsValid)
            {
                return Result<Plane>.Failure("Serialized plane axes do not define a valid plane.");
            }

            Vector3d computedZ = plane.ZAxis;
            computedZ.Unitize();

            if (computedZ * zAxis < 0.999999)
            {
                return Result<Plane>.Failure("Serialized plane Z axis does not match the X and Y axes.");
            }

            return Result<Plane>.Success(plane);
        }
    }
}
