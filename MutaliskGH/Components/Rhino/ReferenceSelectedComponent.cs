using Grasshopper.Kernel;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Rhino
{
    public sealed class ReferenceSelectedComponent : BaseComponent
    {
        private bool wasPressed;
        private readonly List<string> selectedIds = new List<string>();

        public ReferenceSelectedComponent()
            : base(
                "Reference Selected",
                "RefSelected",
                "Open a Rhino selection prompt and store the selected object IDs.",
                CategoryNames.Rhino)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("17bc42a4-34ef-4e44-8fda-4a6943b6dcdd"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddBooleanParameter(
                "Button",
                "Button",
                "Trigger the Rhino selection prompt.",
                GH_ParamAccess.item,
                false);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "IDs",
                "a",
                "Selected Rhino object IDs.",
                GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            bool pressed = false;
            dataAccess.GetData(0, ref pressed);

            if (pressed && !wasPressed)
            {
                global::Rhino.DocObjects.ObjRef[] objects = null;
                global::Rhino.Commands.Result result = global::Rhino.Input.RhinoGet.GetMultipleObjects(
                    "Select objects to reference",
                    false,
                    global::Rhino.DocObjects.ObjectType.AnyObject,
                    out objects);

                if (result == global::Rhino.Commands.Result.Success && objects != null)
                {
                    selectedIds.Clear();
                    for (int index = 0; index < objects.Length; index++)
                    {
                        selectedIds.Add(objects[index].ObjectId.ToString());
                    }
                }
            }

            wasPressed = pressed;
            dataAccess.SetDataList(0, selectedIds);
        }
    }
}
