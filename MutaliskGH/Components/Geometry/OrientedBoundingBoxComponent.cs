using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using CoreGeometry = MutaliskGH.Core.Geometry;

namespace MutaliskGH.Components.Geometry
{
    public sealed class OrientedBoundingBoxComponent : BaseComponent
    {
        public OrientedBoundingBoxComponent()
            : base(
                "Oriented Bounding Box",
                "OBB",
                "Create bounding boxes oriented to the average direction of each brep's edge vectors.",
                CategoryNames.Geometry)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2da10d0b-527b-498a-9283-79174b897800"); }
        }

        protected override string IconResourceName
        {
            get { return "OrientedBoundingBox.png"; }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddBrepParameter(
                "Breps",
                "Brep",
                "Breps to bound.",
                GH_ParamAccess.list);

            parameterManager.AddIntegerParameter(
                "Method",
                "M",
                "0 uses clustered edge directions, 1 uses the mean of normalized edge directions, and 2 uses a length-weighted mean.",
                GH_ParamAccess.item,
                0);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddBoxParameter(
                "Union Box",
                "UBox",
                "Union bounding box using the combined edge direction of the input breps.",
                GH_ParamAccess.item);

            parameterManager.AddBoxParameter(
                "Bounding Boxes",
                "BBox",
                "Per-brep oriented bounding boxes.",
                GH_ParamAccess.list);

            parameterManager.AddVectorParameter(
                "Mean Vectors",
                "Mean Vec",
                "Per-brep mean edge direction vectors.",
                GH_ParamAccess.list);

            parameterManager.AddPlaneParameter(
                "Base Planes",
                "Pln",
                "Per-brep base planes used to construct the oriented bounding boxes.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            List<Brep> breps = new List<Brep>();
            if (!dataAccess.GetDataList(0, breps) || breps.Count == 0)
            {
                return;
            }

            int method = 0;
            dataAccess.GetData(1, ref method);

            List<Brep> validBreps = new List<Brep>();
            foreach (Brep brep in breps)
            {
                if (brep != null && brep.IsValid)
                {
                    validBreps.Add(brep);
                }
            }

            if (validBreps.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "At least one valid brep is required.");
                return;
            }

            List<Box> boxes = new List<Box>();
            List<Vector3d> meanVectors = new List<Vector3d>();
            List<Plane> planes = new List<Plane>();
            List<CoreGeometry.Vector3Value> unionDirections = new List<CoreGeometry.Vector3Value>();

            for (int index = 0; index < validBreps.Count; index++)
            {
                Brep brep = validBreps[index];
                List<CoreGeometry.Vector3Value> edgeDirections = GetEdgeDirections(brep);
                Result<CoreGeometry.OrientedBasisValue> basisResult = CoreGeometry.OrientedBoundingBoxLogic.CreateBasis(edgeDirections, method);
                if (basisResult.IsFailure)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, basisResult.ErrorMessage);
                    continue;
                }

                Result<CoreGeometry.Vector3Value> directionResult = CoreGeometry.OrientedBoundingBoxLogic.SelectPrimaryDirection(edgeDirections, method);
                if (directionResult.IsFailure)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, directionResult.ErrorMessage);
                    continue;
                }

                BoundingBox worldBounds = brep.GetBoundingBox(true);
                Plane plane = new Plane(worldBounds.Center, ToVector3d(basisResult.Value.XAxis), ToVector3d(basisResult.Value.YAxis));
                BoundingBox planeBounds = brep.GetBoundingBox(plane);
                Box box = new Box(plane, planeBounds);

                boxes.Add(box);
                meanVectors.Add(ToVector3d(directionResult.Value));
                planes.Add(plane);
                unionDirections.AddRange(edgeDirections);
            }

            if (boxes.Count == 0)
            {
                return;
            }

            Result<CoreGeometry.OrientedBasisValue> unionBasisResult = CoreGeometry.OrientedBoundingBoxLogic.CreateBasis(unionDirections, method);
            if (unionBasisResult.IsFailure)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, unionBasisResult.ErrorMessage);
                return;
            }

            BoundingBox unionWorldBounds = BoundingBox.Empty;
            for (int index = 0; index < validBreps.Count; index++)
            {
                unionWorldBounds = BoundingBox.Union(unionWorldBounds, validBreps[index].GetBoundingBox(true));
            }

            Plane unionPlane = new Plane(
                unionWorldBounds.Center,
                ToVector3d(unionBasisResult.Value.XAxis),
                ToVector3d(unionBasisResult.Value.YAxis));

            BoundingBox unionPlaneBounds = BoundingBox.Empty;
            for (int index = 0; index < validBreps.Count; index++)
            {
                unionPlaneBounds = BoundingBox.Union(unionPlaneBounds, validBreps[index].GetBoundingBox(unionPlane));
            }

            dataAccess.SetData(0, new Box(unionPlane, unionPlaneBounds));
            dataAccess.SetDataList(1, boxes);
            dataAccess.SetDataList(2, meanVectors);
            dataAccess.SetDataList(3, planes);
        }

        private static List<CoreGeometry.Vector3Value> GetEdgeDirections(Brep brep)
        {
            List<CoreGeometry.Vector3Value> directions = new List<CoreGeometry.Vector3Value>();
            for (int edgeIndex = 0; edgeIndex < brep.Edges.Count; edgeIndex++)
            {
                BrepEdge edge = brep.Edges[edgeIndex];
                Vector3d vector = edge.PointAtEnd - edge.PointAtStart;
                if (vector.Length <= global::Rhino.RhinoMath.ZeroTolerance)
                {
                    continue;
                }

                directions.Add(new CoreGeometry.Vector3Value(vector.X, vector.Y, vector.Z));
            }

            if (directions.Count == 0)
            {
                BoundingBox worldBounds = brep.GetBoundingBox(true);
                Vector3d diagonal = worldBounds.Max - worldBounds.Min;
                if (diagonal.Length > global::Rhino.RhinoMath.ZeroTolerance)
                {
                    directions.Add(new CoreGeometry.Vector3Value(diagonal.X, diagonal.Y, diagonal.Z));
                }
            }

            return directions;
        }

        private static Vector3d ToVector3d(CoreGeometry.Vector3Value vector)
        {
            return new Vector3d(vector.X, vector.Y, vector.Z);
        }
    }
}
