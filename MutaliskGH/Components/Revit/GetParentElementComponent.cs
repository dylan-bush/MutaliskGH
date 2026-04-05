using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using System;

namespace MutaliskGH.Components.Revit
{
    public sealed class GetParentElementComponent : BaseComponent
    {
        public GetParentElementComponent()
            : base(
                "Get Parent Element",
                "GetParent",
                "Find the direct Revit parent or host for a nested element and report the parent hierarchy.",
                CategoryNames.RevitQuery)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("b5e4d377-a386-4f79-bf8f-66aec3cbc1b6"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Element",
                "E",
                "Revit element to inspect for parent or host relationships.",
                GH_ParamAccess.item);

            parameterManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter(
                "Parent",
                "P",
                "Direct parent or host element.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Parent Type",
                "T",
                "Element type of the parent when available.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Parent Family",
                "F",
                "Family of the parent when available.",
                GH_ParamAccess.item);

            parameterManager.AddGenericParameter(
                "Hierarchy",
                "H",
                "Parent hierarchy from immediate parent upward.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Relationships",
                "R",
                "Relationship labels for each hierarchy step.",
                GH_ParamAccess.list);

            parameterManager.AddTextParameter(
                "Error",
                "Err",
                "Error message when the query fails.",
                GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            IGH_Goo rawElement = null;
            if (!dataAccess.GetData(0, ref rawElement))
            {
                return;
            }

            object element = GrasshopperValueHelper.Unwrap(rawElement);
            Result<RevitParentElementResult> result = RevitParentElementLogic.GetParentData(element);
            if (result.IsFailure)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.ErrorMessage);
                dataAccess.SetData(5, result.ErrorMessage);
                return;
            }

            dataAccess.SetData(0, RevitGrasshopperWrapper.WrapElement(result.Value.Parent));
            dataAccess.SetData(1, RevitGrasshopperWrapper.WrapElement(result.Value.ParentType));
            dataAccess.SetData(2, RevitGrasshopperWrapper.WrapElement(result.Value.ParentFamily));
            dataAccess.SetDataList(3, RevitGrasshopperWrapper.WrapElements(result.Value.Hierarchy));
            dataAccess.SetDataList(4, result.Value.RelationshipTypes);
            dataAccess.SetData(5, string.Empty);
        }
    }
}
