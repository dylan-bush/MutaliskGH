using System.Collections.Generic;

namespace MutaliskGH.Core.Revit
{
    public sealed class RevitParentElementResult
    {
        public RevitParentElementResult(
            object parent,
            object parentType,
            object parentFamily,
            IReadOnlyList<object> hierarchy,
            IReadOnlyList<string> relationshipTypes)
        {
            Parent = parent;
            ParentType = parentType;
            ParentFamily = parentFamily;
            Hierarchy = hierarchy;
            RelationshipTypes = relationshipTypes;
        }

        public object Parent { get; }

        public object ParentType { get; }

        public object ParentFamily { get; }

        public IReadOnlyList<object> Hierarchy { get; }

        public IReadOnlyList<string> RelationshipTypes { get; }
    }
}
