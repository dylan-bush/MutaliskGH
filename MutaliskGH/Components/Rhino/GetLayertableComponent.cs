using Grasshopper.Kernel;
using MutaliskGH.Core.Rhino;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Rhino
{
    public sealed class GetLayertableComponent : BaseComponent
    {
        public GetLayertableComponent()
            : base(
                "Get Layertable",
                "Layers",
                "Return the active Rhino document layer full paths.",
                CategoryNames.Rhino)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f9228a0e-9728-4d75-a6ba-d4f232e4650d"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Layers",
                "L",
                "Active Rhino document layer full paths.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                return;
            }

            var layerPaths = new List<string>();
            for (int index = 0; index < doc.Layers.Count; index++)
            {
                global::Rhino.DocObjects.Layer layer = doc.Layers[index];
                if (layer != null)
                {
                    layerPaths.Add(layer.FullPath);
                }
            }

            dataAccess.SetDataList(0, RhinoMetadataLogic.NormalizeLayerPaths(layerPaths));
        }
    }
}
