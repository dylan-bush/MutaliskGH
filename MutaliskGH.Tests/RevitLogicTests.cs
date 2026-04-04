using MutaliskGH.Core.Revit;
using System.Collections.Generic;
using Xunit;

namespace MutaliskGH.Tests
{
    public class RevitLogicTests
    {
        [Fact]
        public void RevitParentElementLogic_ReturnsSuperComponentParentTypeAndFamily()
        {
            var document = new FakeDocument();
            var family = new FakeFamily("Panel Family");
            var parentType = new FakeElementType(document, 2001);
            var parent = new FamilyInstance(document, 1001)
            {
                Symbol = new FakeSymbol(family),
                TypeId = new FakeElementId(2001),
                Category = new FakeCategory("Generic Models")
            };
            var child = new FakeElement(document, 1002)
            {
                SuperComponent = parent,
                Category = new FakeCategory("Adaptive Points")
            };

            document.Register(parent);
            document.Register(parentType);
            document.Register(child);

            var result = RevitParentElementLogic.GetParentData(child);

            Assert.True(result.IsSuccess);
            Assert.Same(parent, result.Value.Parent);
            Assert.Same(parentType, result.Value.ParentType);
            Assert.Same(family, result.Value.ParentFamily);
            Assert.Single(result.Value.Hierarchy);
            Assert.Equal("SuperComponent", result.Value.RelationshipTypes[0]);
        }

        [Fact]
        public void RevitParentElementLogic_UsesDependentSearchForNestedFamilyInstance()
        {
            var document = new FakeDocument();
            var host = new FakeElement(document, 1001);
            var child = new FamilyInstance(document, 1002)
            {
                Category = new FakeCategory("Generic Models")
            };

            host.DependentIds.Add(child.Id);
            document.Register(host);
            document.Register(child);

            var result = RevitParentElementLogic.GetParentData(child);

            Assert.True(result.IsSuccess);
            Assert.Same(host, result.Value.Parent);
            Assert.Equal("Nested Family", result.Value.RelationshipTypes[0]);
        }

        [Fact]
        public void RevitSpotElevationReferenceLogic_ResolvesAdaptivePointParent()
        {
            var document = new FakeDocument();
            var parent = new FamilyInstance(document, 1001)
            {
                Category = new FakeCategory("Generic Models")
            };
            var adaptivePoint = new FakeElement(document, 1002)
            {
                SuperComponent = parent,
                Category = new FakeCategory("Adaptive Points")
            };
            var spot = new SpotDimension(document, 1003)
            {
                References = new List<FakeReference>
                {
                    new FakeReference(adaptivePoint.Id)
                }
            };

            document.Register(parent);
            document.Register(adaptivePoint);
            document.Register(spot);

            var result = RevitSpotElevationReferenceLogic.GetReferenceData(spot);

            Assert.True(result.IsSuccess);
            Assert.Same(adaptivePoint, result.Value.Reference);
            Assert.Same(parent, result.Value.ReferenceParent);
            Assert.Equal(1001, result.Value.ParentElementId);
            Assert.Equal("Adaptive Points", result.Value.CategoryName);
        }

        [Fact]
        public void RevitSpotElevationReferenceLogic_FailsForNonSpotInput()
        {
            var document = new FakeDocument();
            var element = new FakeElement(document, 1001);
            document.Register(element);

            var result = RevitSpotElevationReferenceLogic.GetReferenceData(element);

            Assert.False(result.IsSuccess);
            Assert.Contains("SpotDimension", result.ErrorMessage);
        }

        private sealed class FakeDocument
        {
            private readonly Dictionary<int, object> _elements = new Dictionary<int, object>();

            public void Register(FakeElement element)
            {
                _elements[element.Id.IntegerValue] = element;
            }

            public object GetElement(FakeElementId id)
            {
                object element;
                return id != null && _elements.TryGetValue(id.IntegerValue, out element) ? element : null;
            }

            public IEnumerable<object> GetElements()
            {
                return _elements.Values;
            }
        }

        private class FakeElementId
        {
            public static readonly FakeElementId InvalidElementId = new FakeElementId(-1);

            public FakeElementId(int integerValue)
            {
                IntegerValue = integerValue;
            }

            public int IntegerValue { get; }

            public override string ToString()
            {
                return IntegerValue.ToString();
            }
        }

        private class FakeCategory
        {
            public FakeCategory(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private class FakeElement
        {
            public FakeElement(FakeDocument document, int id)
            {
                Document = document;
                Id = new FakeElementId(id);
                GroupId = FakeElementId.InvalidElementId;
                AssemblyInstanceId = FakeElementId.InvalidElementId;
                Parameters = new List<FakeParameter>();
                DependentIds = new List<FakeElementId>();
                TypeId = FakeElementId.InvalidElementId;
            }

            public FakeDocument Document { get; }

            public FakeElementId Id { get; }

            public FakeElementId GroupId { get; set; }

            public FakeElementId AssemblyInstanceId { get; set; }

            public FakeCategory Category { get; set; }

            public object Host { get; set; }

            public object SuperComponent { get; set; }

            public List<FakeParameter> Parameters { get; }

            public List<FakeElementId> DependentIds { get; }

            public FakeElementId TypeId { get; set; }

            public IEnumerable<FakeElementId> GetDependentElements(object filter)
            {
                return DependentIds;
            }

            public FakeElementId GetTypeId()
            {
                return TypeId;
            }
        }

        private sealed class FakeElementType : FakeElement
        {
            public FakeElementType(FakeDocument document, int id)
                : base(document, id)
            {
            }
        }

        private sealed class FamilyInstance : FakeElement
        {
            public FamilyInstance(FakeDocument document, int id)
                : base(document, id)
            {
            }

            public FakeSymbol Symbol { get; set; }
        }

        private sealed class FakeSymbol
        {
            public FakeSymbol(FakeFamily family)
            {
                Family = family;
            }

            public FakeFamily Family { get; }
        }

        private sealed class FakeFamily
        {
            public FakeFamily(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class FakeReference
        {
            public FakeReference(FakeElementId elementId)
            {
                ElementId = elementId;
            }

            public FakeElementId ElementId { get; }
        }

        private sealed class SpotDimension : FakeElement
        {
            public SpotDimension(FakeDocument document, int id)
                : base(document, id)
            {
                References = new List<FakeReference>();
            }

            public List<FakeReference> References { get; set; }
        }

        private sealed class FakeParameter
        {
            public FakeParameter(FakeElementId elementId)
            {
                StorageType = new FakeStorageType("ElementId");
                ElementIdValue = elementId;
            }

            public FakeStorageType StorageType { get; }

            public FakeElementId ElementIdValue { get; }

            public FakeElementId AsElementId()
            {
                return ElementIdValue;
            }
        }

        private sealed class FakeStorageType
        {
            private readonly string _name;

            public FakeStorageType(string name)
            {
                _name = name;
            }

            public override string ToString()
            {
                return _name;
            }
        }
    }
}
