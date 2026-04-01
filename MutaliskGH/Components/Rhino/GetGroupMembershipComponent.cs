using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Rhino;
using MutaliskGH.Framework;
using System;
using System.Collections.Generic;

namespace MutaliskGH.Components.Rhino
{
    public sealed class GetGroupMembershipComponent : BaseComponent
    {
        public GetGroupMembershipComponent()
            : base(
                "Get Group Membership",
                "GGM",
                "Return the Rhino group names for each referenced object ID.",
                CategoryNames.Rhino)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b1e97f35-590d-4dad-b2e7-77f9e3029852"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Geometry / GUID",
                "Ref",
                "Referenced Rhino geometry or Rhino object GUIDs.",
                GH_ParamAccess.tree);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter(
                "Groups",
                "G",
                "Group names for each input object. Outputs Ungrouped when no Rhino groups are assigned.",
                GH_ParamAccess.tree);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            GH_Structure<IGH_Goo> idTree;
            if (!dataAccess.GetDataTree(0, out idTree))
            {
                return;
            }

            var outputTree = new GH_Structure<GH_String>();
            var doc = global::Rhino.RhinoDoc.ActiveDoc;
            if (doc == null)
            {
                dataAccess.SetDataTree(0, outputTree);
                return;
            }

            for (int branchIndex = 0; branchIndex < idTree.PathCount; branchIndex++)
            {
                GH_Path branchPath = idTree.get_Path(branchIndex);
                IList<IGH_Goo> branch = idTree.Branches[branchIndex];

                for (int itemIndex = 0; itemIndex < branch.Count; itemIndex++)
                {
                    GH_Path itemPath = branchPath.AppendElement(itemIndex);
                    IGH_Goo goo = branch[itemIndex];
                    Result<Guid> guidResult = TryResolveObjectId(goo);

                    IReadOnlyList<string> groupNames;
                    if (!guidResult.IsSuccess)
                    {
                        groupNames = RhinoMetadataLogic.NormalizeGroupNames(null);
                    }
                    else
                    {
                        global::Rhino.DocObjects.RhinoObject rhinoObject = doc.Objects.FindId(guidResult.Value);
                        if (rhinoObject == null)
                        {
                            groupNames = RhinoMetadataLogic.NormalizeGroupNames(null);
                        }
                        else
                        {
                            int[] groupIndices = rhinoObject.Attributes.GetGroupList();
                            var names = new List<string>();
                            if (groupIndices != null)
                            {
                                for (int groupIndex = 0; groupIndex < groupIndices.Length; groupIndex++)
                                {
                                    global::Rhino.DocObjects.Group group = doc.Groups.FindIndex(groupIndices[groupIndex]);
                                    if (group != null)
                                    {
                                        names.Add(group.Name);
                                    }
                                }
                            }

                            groupNames = RhinoMetadataLogic.NormalizeGroupNames(names);
                        }
                    }

                    for (int nameIndex = 0; nameIndex < groupNames.Count; nameIndex++)
                    {
                        outputTree.Append(new GH_String(groupNames[nameIndex]), itemPath);
                    }
                }
            }

            dataAccess.SetDataTree(0, outputTree);
        }

        private static Result<Guid> TryResolveObjectId(IGH_Goo goo)
        {
            IGH_GeometricGoo geometricGoo = goo as IGH_GeometricGoo;
            if (geometricGoo != null && geometricGoo.IsReferencedGeometry && geometricGoo.ReferenceID != Guid.Empty)
            {
                return Result<Guid>.Success(geometricGoo.ReferenceID);
            }

            object unwrappedValue = GrasshopperValueHelper.Unwrap(goo);
            return RhinoMetadataLogic.TryParseObjectId(unwrappedValue);
        }
    }
}
