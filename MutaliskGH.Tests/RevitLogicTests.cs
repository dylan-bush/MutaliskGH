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

        [Fact]
        public void RevitMatchFilterElementsLogic_MatchesInRhinoOrder()
        {
            var revitElements = new object[] { "A", "B", "C" };
            var revitValues = new[] { "04140", "04139", "04141" };
            var rhinoValues = new[] { "04139", "04141" };

            var result = RevitMatchFilterElementsLogic.MatchByValue(revitElements, revitValues, rhinoValues);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "B", "C" }, result.Value.MatchedElements);
            Assert.Equal(new[] { "04139", "04141" }, result.Value.MatchedRevitValues);
            Assert.Equal(new[] { "04139", "04141" }, result.Value.MatchedRhinoValues);
            Assert.Equal(new[] { 1, 2 }, result.Value.MatchedRevitIndices);
            Assert.Equal(new[] { 0, 1 }, result.Value.MatchedRhinoIndices);
        }

        [Fact]
        public void RevitMatchFilterElementsLogic_ConsumesDuplicatesInSequence()
        {
            var revitElements = new object[] { "A1", "A2", "B1" };
            var revitValues = new[] { "04139", "04139", "04140" };
            var rhinoValues = new[] { "04139", "04139", "04140" };

            var result = RevitMatchFilterElementsLogic.MatchByValue(revitElements, revitValues, rhinoValues);

            Assert.True(result.IsSuccess);
            Assert.Equal(new[] { "A1", "A2", "B1" }, result.Value.MatchedElements);
            Assert.Equal(new[] { 0, 1, 2 }, result.Value.MatchedRevitIndices);
            Assert.Equal(new[] { 0, 1, 2 }, result.Value.MatchedRhinoIndices);
        }

        [Fact]
        public void RevitViewRangeBrepLogic_UsesCropBoxForGenericOrthographicViews()
        {
            var document = new FakeDocument();
            var view = new FakeView(document, "Section A")
            {
                CropBox = new FakeBoundingBoxXYZ(
                    new FakeXYZ(-10.0, -5.0, -2.0),
                    new FakeXYZ(10.0, 5.0, 2.0),
                    new FakeTransform(
                        new FakeXYZ(100.0, 200.0, 300.0),
                        new FakeXYZ(1.0, 0.0, 0.0),
                        new FakeXYZ(0.0, 1.0, 0.0),
                        new FakeXYZ(0.0, 0.0, 1.0)))
            };

            var result = RevitViewRangeBrepLogic.GetViewData(view);

            Assert.True(result.IsSuccess);
            Assert.Equal("Section A", result.Value.ViewName);
            Assert.Equal(-10.0, result.Value.Min.X);
            Assert.Equal(10.0, result.Value.Max.X);
            Assert.Equal(-2.0, result.Value.Min.Z);
            Assert.Equal(2.0, result.Value.Max.Z);
            Assert.Equal("CropBox", result.Value.SourceDescription);
        }

        [Fact]
        public void RevitViewRangeBrepLogic_OverridesPlanDepthWithViewRange()
        {
            var document = new FakeDocument();
            var plan = new FakePlanView(document, "Level 1 Plan")
            {
                CropBox = new FakeBoundingBoxXYZ(
                    new FakeXYZ(-20.0, -10.0, -1.0),
                    new FakeXYZ(20.0, 10.0, 1.0),
                    new FakeTransform(
                        new FakeXYZ(0.0, 0.0, 100.0),
                        new FakeXYZ(1.0, 0.0, 0.0),
                        new FakeXYZ(0.0, 1.0, 0.0),
                        new FakeXYZ(0.0, 0.0, 1.0))),
                GenLevel = new FakeLevel(96.0),
                ViewRange = new FakePlanViewRange(topOffset: 12.0, depthOffset: -4.0, bottomOffset: -2.0)
            };

            var result = RevitViewRangeBrepLogic.GetViewData(plan);

            Assert.True(result.IsSuccess);
            Assert.Equal(-8.0, result.Value.Min.Z);
            Assert.Equal(8.0, result.Value.Max.Z);
            Assert.Equal("CropBox + ViewRange", result.Value.SourceDescription);
        }

        [Fact]
        public void RevitViewRangeBrepLogic_FallsBackToSectionBoxWhenCropBoxIsMissing()
        {
            var document = new FakeDocument();
            var view = new FakeSectionBoxView(document, "3D")
            {
                SectionBox = new FakeBoundingBoxXYZ(
                    new FakeXYZ(0.0, 0.0, 0.0),
                    new FakeXYZ(5.0, 6.0, 7.0),
                    new FakeTransform(
                        new FakeXYZ(10.0, 20.0, 30.0),
                        new FakeXYZ(1.0, 0.0, 0.0),
                        new FakeXYZ(0.0, 1.0, 0.0),
                        new FakeXYZ(0.0, 0.0, 1.0)))
            };

            var result = RevitViewRangeBrepLogic.GetViewData(view);

            Assert.True(result.IsSuccess);
            Assert.Equal("SectionBox", result.Value.SourceDescription);
            Assert.Equal(7.0, result.Value.Max.Z);
        }

        [Fact]
        public void RevitElementMaterialMapLogic_CreateSettings_UsesExpectedDefaults()
        {
            RevitElementMaterialMapSettings settings = RevitElementMaterialMapLogic.CreateSettings(null, null, null, null);

            Assert.Equal(1e-9, settings.MinVolumeFt3, 12);
            Assert.Equal("Coarse", settings.DetailLevelName);
            Assert.True(settings.EmitGeometry);
            Assert.False(settings.Debug);
        }

        [Theory]
        [InlineData(0, "Coarse")]
        [InlineData(1, "Medium")]
        [InlineData(2, "Fine")]
        [InlineData("Coarse", "Coarse")]
        [InlineData("Medium", "Medium")]
        [InlineData("Fine", "Fine")]
        [InlineData("m", "Medium")]
        [InlineData("f", "Fine")]
        public void RevitElementMaterialMapLogic_NormalizesDetail(object input, string expected)
        {
            Assert.Equal(expected, RevitElementMaterialMapLogic.NormalizeDetail(input));
        }

        [Theory]
        [InlineData(null, 1e-9)]
        [InlineData(0.0, 1e-9)]
        [InlineData(-4.0, 1e-9)]
        [InlineData(0.25, 0.25)]
        public void RevitElementMaterialMapLogic_NormalizesMinimumVolume(object input, double expected)
        {
            Assert.Equal(expected, RevitElementMaterialMapLogic.NormalizeMinimumVolume(input), 12);
        }

        [Fact]
        public void RevitRenameViewsLogic_PairsSingleNameAcrossAllViews()
        {
            var views = new object[] { "A", "B", "C" };
            var names = new[] { "Renamed" };

            var result = RevitRenameViewsLogic.PairViewsAndNames(views, names);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.Count);
            Assert.Equal("Renamed", result.Value[0].Name);
            Assert.Equal("Renamed", result.Value[2].Name);
        }

        [Fact]
        public void RevitRenameViewsLogic_RejectsMismatchedNameCounts()
        {
            var views = new object[] { "A", "B" };
            var names = new[] { "One", "Two", "Three" };

            var result = RevitRenameViewsLogic.PairViewsAndNames(views, names);

            Assert.False(result.IsSuccess);
            Assert.Contains("one name", result.ErrorMessage);
        }

        [Fact]
        public void RevitExportLogic_PairsViewsAndPaths()
        {
            var views = new object[] { "ViewA", "ViewB" };
            var names = new[] { "Sheet-A", "Sheet-B" };

            var result = RevitExportLogic.PairViewsAndTargetPaths(views, names, @"C:\Exports", ".pdf");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Equal(@"C:\Exports\Sheet-A.pdf", result.Value[0].Path);
            Assert.Equal(@"C:\Exports\Sheet-B.pdf", result.Value[1].Path);
        }

        [Fact]
        public void RevitExportLogic_RejectsDuplicateTargetPaths()
        {
            var views = new object[] { "ViewA", "ViewB" };
            var names = new[] { "Same", "Same" };

            var result = RevitExportLogic.PairViewsAndTargetPaths(views, names, @"C:\Exports", ".dwg");

            Assert.False(result.IsSuccess);
            Assert.Contains("Duplicate path", result.ErrorMessage);
        }

        [Fact]
        public void RevitZoomElementLogic_ReturnsTargetMetadata()
        {
            var document = new FakeDocument();
            var element = new FakeElement(document, 4242)
            {
                Category = new FakeCategory("Views")
            };

            var result = RevitZoomElementLogic.GetTargetData(element);

            Assert.True(result.IsSuccess);
            Assert.Same(element, result.Value.Element);
            Assert.Equal(4242, result.Value.ElementIdInteger);
            Assert.Equal("Views", result.Value.CategoryName);
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

        private sealed class FakeXYZ
        {
            public FakeXYZ(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public double X { get; }

            public double Y { get; }

            public double Z { get; }
        }

        private sealed class FakeTransform
        {
            public FakeTransform(FakeXYZ origin, FakeXYZ basisX, FakeXYZ basisY, FakeXYZ basisZ)
            {
                Origin = origin;
                BasisX = basisX;
                BasisY = basisY;
                BasisZ = basisZ;
            }

            public FakeXYZ Origin { get; }

            public FakeXYZ BasisX { get; }

            public FakeXYZ BasisY { get; }

            public FakeXYZ BasisZ { get; }
        }

        private sealed class FakeBoundingBoxXYZ
        {
            public FakeBoundingBoxXYZ(FakeXYZ min, FakeXYZ max, FakeTransform transform)
            {
                Min = min;
                Max = max;
                Transform = transform;
            }

            public FakeXYZ Min { get; }

            public FakeXYZ Max { get; }

            public FakeTransform Transform { get; }
        }

        private class FakeView
        {
            public FakeView(FakeDocument document, string name)
            {
                Document = document;
                Name = name;
            }

            public FakeDocument Document { get; }

            public string Name { get; }

            public FakeBoundingBoxXYZ CropBox { get; set; }
        }

        private sealed class FakeSectionBoxView : FakeView
        {
            public FakeSectionBoxView(FakeDocument document, string name)
                : base(document, name)
            {
            }

            public FakeBoundingBoxXYZ SectionBox { get; set; }

            public FakeBoundingBoxXYZ GetSectionBox()
            {
                return SectionBox;
            }
        }

        private sealed class FakePlanView : FakeView
        {
            public FakePlanView(FakeDocument document, string name)
                : base(document, name)
            {
            }

            public FakeLevel GenLevel { get; set; }

            public FakePlanViewRange ViewRange { get; set; }

            public FakePlanViewRange GetViewRange()
            {
                return ViewRange;
            }
        }

        private sealed class FakeLevel
        {
            public FakeLevel(double elevation)
            {
                Elevation = elevation;
            }

            public double Elevation { get; }
        }

        private sealed class FakePlanViewRange
        {
            private readonly Dictionary<Autodesk.Revit.DB.PlanViewPlane, double> _offsets;

            public FakePlanViewRange(double topOffset, double depthOffset, double bottomOffset)
            {
                _offsets = new Dictionary<Autodesk.Revit.DB.PlanViewPlane, double>
                {
                    { Autodesk.Revit.DB.PlanViewPlane.TopClipPlane, topOffset },
                    { Autodesk.Revit.DB.PlanViewPlane.ViewDepthPlane, depthOffset },
                    { Autodesk.Revit.DB.PlanViewPlane.BottomClipPlane, bottomOffset }
                };
            }

            public double GetOffset(Autodesk.Revit.DB.PlanViewPlane plane)
            {
                return _offsets[plane];
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

namespace Autodesk.Revit.DB
{
    public enum PlanViewPlane
    {
        TopClipPlane,
        ViewDepthPlane,
        BottomClipPlane
    }
}
