using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Format;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;

namespace MutaliskGH.Components.Format
{
    public sealed class SerializePlaneComponent : BaseComponent
    {
        public SerializePlaneComponent()
            : base(
                "Serialize Plane",
                "SerPln",
                "Convert a plane into serialized text using the O...X...Y...Z... format.",
                CategoryNames.Format)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6e0d56b2-5bad-438b-ac46-c73668badff6"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddPlaneParameter(
                "Plane",
                "Pln",
                "Plane to serialize.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Text",
                "Txt",
                "Serialized plane text.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            Plane plane = Plane.Unset;
            if (!dataAccess.GetData(0, ref plane))
            {
                return;
            }

            Result<string> result = PlaneSerializationLogic.Serialize(ToPlaneValue(plane));
            if (ReportFailure(result))
            {
                return;
            }

            dataAccess.SetData(0, result.Value);
        }

        private static PlaneValue ToPlaneValue(Plane plane)
        {
            return new PlaneValue(
                new Vector3Value(plane.OriginX, plane.OriginY, plane.OriginZ),
                new Vector3Value(plane.XAxis.X, plane.XAxis.Y, plane.XAxis.Z),
                new Vector3Value(plane.YAxis.X, plane.YAxis.Y, plane.YAxis.Z),
                new Vector3Value(plane.ZAxis.X, plane.ZAxis.Y, plane.ZAxis.Z));
        }
    }
}
