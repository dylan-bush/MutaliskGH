using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using MutaliskGH.Core.Revit;
using MutaliskGH.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MutaliskGH.Components.Revit
{
    public sealed class ElementMaterialMapComponent : BaseComponent
    {
        public ElementMaterialMapComponent()
            : base(
                "Element Material Map",
                "MatMap",
                "Extract Revit solid geometry as Rhino meshes while preserving per-solid material associations, including painted overrides.",
                CategoryNames.RevitQuery)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("69f6fb86-f220-4612-a0c4-360f941e5f7f"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddGenericParameter("Elements", "E", "Revit elements to extract.", GH_ParamAccess.list);
            parameterManager.AddNumberParameter("Minimum Volume", "minVol", "Minimum solid volume in ft^3. Solids below this threshold are skipped.", GH_ParamAccess.item, 1e-9);
            parameterManager.AddIntegerParameter("Detail", "detail", "Revit detail level: 0 = Coarse, 1 = Medium, 2 = Fine.", GH_ParamAccess.item, 0);
            parameterManager.AddBooleanParameter("Emit Geometry", "emitGeo", "If true, output Rhino meshes. If false, skip mesh conversion but still report material sets and volumes.", GH_ParamAccess.item, true);
            parameterManager.AddBooleanParameter("Debug", "dbg", "If true, output debug log messages.", GH_ParamAccess.item, false);

            for (int index = 0; index < Params.Input.Count; index++)
            {
                Params.Input[index].Optional = true;
            }
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("Output", "out", "Summary output and any collected error information.", GH_ParamAccess.item);
            parameterManager.AddMeshParameter("Mesh", "mesh", "One Rhino mesh per Revit solid.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("Material Names", "matNames", "Comma-separated material names per solid.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("Material Ids", "matIds", "Comma-separated material id values per solid.", GH_ParamAccess.list);
            parameterManager.AddNumberParameter("Volume Ft3", "volFt3", "Solid volume per solid in internal Revit ft^3 units.", GH_ParamAccess.list);
            parameterManager.AddTextParameter("Log", "log", "Debug log.", GH_ParamAccess.list);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            var rawElements = new List<IGH_Goo>();
            if (!dataAccess.GetDataList(0, rawElements) || rawElements.Count == 0)
            {
                return;
            }

            object rawMinVolume = null;
            object rawDetail = null;
            object rawEmitGeometry = null;
            object rawDebug = null;
            dataAccess.GetData(1, ref rawMinVolume);
            dataAccess.GetData(2, ref rawDetail);
            dataAccess.GetData(3, ref rawEmitGeometry);
            dataAccess.GetData(4, ref rawDebug);

            RevitElementMaterialMapSettings settings = RevitElementMaterialMapLogic.CreateSettings(rawMinVolume, rawDetail, rawEmitGeometry, rawDebug);
            var log = new List<string>();
            Action<string> debug = message =>
            {
                if (settings.Debug)
                {
                    log.Add(message);
                }
            };

            debug("Started.");
            debug("Detail=" + settings.DetailLevelName + " minVol(ft^3)=" + settings.MinVolumeFt3 + " emitGeo=" + settings.EmitGeometry + " dbg=" + settings.Debug);

            List<object> elements = UnwrapElements(rawElements, log, settings.Debug);
            debug("Unwrapped elements: " + elements.Count);

            var meshes = new List<Mesh>();
            var materialNames = new List<string>();
            var materialIds = new List<string>();
            var volumes = new List<double>();

            int elementCount = 0;
            int solidCount = 0;
            int skippedSmall = 0;
            int meshFail = 0;
            int outCount = 0;

            for (int elementIndex = 0; elementIndex < elements.Count; elementIndex++)
            {
                object element = elements[elementIndex];
                if (element == null)
                {
                    continue;
                }

                elementCount++;
                object document = RevitReflectionHelper.GetPropertyValue(element, "Document");
                if (document == null)
                {
                    continue;
                }

                object options = CreateGeometryOptions(element.GetType().Assembly, settings.DetailLevelName);
                object geometry = RevitReflectionHelper.InvokeMethod(element, "get_Geometry", options);
                if (geometry == null)
                {
                    continue;
                }

                Dictionary<long, string> materialNameCache = new Dictionary<long, string>(256);
                List<object> solids = GetSolids(geometry);

                for (int solidIndex = 0; solidIndex < solids.Count; solidIndex++)
                {
                    object solid = solids[solidIndex];
                    if (solid == null)
                    {
                        continue;
                    }

                    solidCount++;
                    double? volume = TryReadDouble(RevitReflectionHelper.GetPropertyValue(solid, "Volume"));
                    if (!volume.HasValue)
                    {
                        continue;
                    }

                    if (volume.Value < settings.MinVolumeFt3)
                    {
                        skippedSmall++;
                        continue;
                    }

                    List<string> names;
                    List<long> ids;
                    ReadSolidMaterials(document, element, solid, materialNameCache, out names, out ids);

                    if (settings.EmitGeometry)
                    {
                        Mesh mesh = ConvertSolidToRhinoMesh(solid);
                        if (mesh == null)
                        {
                            meshFail++;
                            continue;
                        }

                        meshes.Add(mesh);
                    }

                    materialNames.Add(names.Count > 0 ? string.Join(", ", names) : string.Empty);
                    materialIds.Add(ids.Count > 0 ? string.Join(", ", ids.Select(value => value.ToString())) : string.Empty);
                    volumes.Add(volume.Value);
                    outCount++;
                }
            }

            debug("Elements: " + elementCount);
            debug("Solids: " + solidCount);
            debug("SkippedSmall: " + skippedSmall);
            debug("MeshFail: " + meshFail);
            debug("Out: " + outCount);

            dataAccess.SetData(0, "Elements: " + elementCount + ", Solids: " + solidCount + ", Out: " + outCount + ", SkippedSmall: " + skippedSmall + ", MeshFail: " + meshFail);
            dataAccess.SetDataList(1, meshes);
            dataAccess.SetDataList(2, materialNames);
            dataAccess.SetDataList(3, materialIds);
            dataAccess.SetDataList(4, volumes);
            dataAccess.SetDataList(5, log);
        }

        private static List<object> UnwrapElements(IReadOnlyList<IGH_Goo> values, List<string> log, bool debug)
        {
            var elements = new List<object>();
            for (int index = 0; index < values.Count; index++)
            {
                AddUnwrapped(values[index], elements, log, debug);
            }

            return elements;
        }

        private static void AddUnwrapped(object value, List<object> elements, List<string> log, bool debug)
        {
            if (value == null)
            {
                return;
            }

            if (value is IGH_Goo goo)
            {
                AddUnwrapped(GrasshopperValueHelper.Unwrap(goo), elements, log, debug);
                return;
            }

            if (!(value is string) && value is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    AddUnwrapped(item, elements, log, debug);
                }

                return;
            }

            PropertyInfo valueProperty = value.GetType().GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
            if (valueProperty != null)
            {
                try
                {
                    object unwrapped = valueProperty.GetValue(value, null);
                    if (unwrapped != null)
                    {
                        elements.Add(unwrapped);
                        return;
                    }
                }
                catch
                {
                }
            }

            if (value.GetType().Name.IndexOf("Element", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                elements.Add(value);
                return;
            }

            if (debug)
            {
                log.Add("Could not unwrap element. Type: " + value.GetType().FullName);
            }
        }

        private static object CreateGeometryOptions(Assembly assembly, string detailLevelName)
        {
            Type optionsType = assembly.GetType("Autodesk.Revit.DB.Options", false, false);
            object options = optionsType == null ? null : Activator.CreateInstance(optionsType);
            if (options == null)
            {
                return null;
            }

            SetProperty(options, "ComputeReferences", true);
            SetProperty(options, "IncludeNonVisibleObjects", true);

            object detailLevel = RevitReflectionHelper.ParseEnumArgument(assembly, "Autodesk.Revit.DB.ViewDetailLevel", detailLevelName);
            if (detailLevel != null)
            {
                SetProperty(options, "DetailLevel", detailLevel);
            }

            return options;
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            PropertyInfo property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null || !property.CanWrite)
            {
                return;
            }

            try
            {
                property.SetValue(target, value, null);
            }
            catch
            {
            }
        }

        private static List<object> GetSolids(object geometryElement)
        {
            var solids = new List<object>();
            if (geometryElement == null)
            {
                return solids;
            }

            IEnumerable enumerable = geometryElement as IEnumerable;
            if (enumerable == null)
            {
                return solids;
            }

            foreach (object geometryObject in enumerable)
            {
                if (geometryObject == null)
                {
                    continue;
                }

                if (string.Equals(geometryObject.GetType().Name, "Solid", StringComparison.Ordinal))
                {
                    solids.Add(geometryObject);
                    continue;
                }

                if (string.Equals(geometryObject.GetType().Name, "GeometryInstance", StringComparison.Ordinal))
                {
                    object instanceGeometry = RevitReflectionHelper.InvokeMethod(geometryObject, "GetInstanceGeometry");
                    solids.AddRange(GetSolids(instanceGeometry));
                }
            }

            return solids;
        }

        private static void ReadSolidMaterials(
            object document,
            object element,
            object solid,
            Dictionary<long, string> materialNameCache,
            out List<string> materialNames,
            out List<long> materialIds)
        {
            materialNames = new List<string>();
            materialIds = new List<long>();

            object faces = RevitReflectionHelper.GetPropertyValue(solid, "Faces");
            IEnumerable enumerable = faces as IEnumerable;
            if (enumerable == null)
            {
                return;
            }

            var seen = new HashSet<long>();
            object elementId = RevitReflectionHelper.GetPropertyValue(element, "Id");

            foreach (object face in enumerable)
            {
                if (face == null)
                {
                    continue;
                }

                object materialId = RevitReflectionHelper.InvokeMethod(document, "GetPaintedMaterial", elementId, face);
                if (!RevitReflectionHelper.IsElementIdValid(materialId))
                {
                    materialId = RevitReflectionHelper.GetPropertyValue(face, "MaterialElementId");
                }

                int? id = RevitReflectionHelper.GetElementIdInteger(materialId);
                if (!id.HasValue)
                {
                    continue;
                }

                long key = id.Value;
                if (!seen.Add(key))
                {
                    continue;
                }

                materialIds.Add(key);
                materialNames.Add(GetMaterialName(document, materialId, materialNameCache, key));
            }
        }

        private static string GetMaterialName(object document, object materialId, Dictionary<long, string> cache, long key)
        {
            if (cache.TryGetValue(key, out string name))
            {
                return name;
            }

            object material = RevitReflectionHelper.GetDocumentElement(document, materialId);
            string materialName = RevitReflectionHelper.GetPropertyValue(material, "Name") as string;
            cache[key] = !string.IsNullOrWhiteSpace(materialName) ? materialName : key.ToString();
            return cache[key];
        }

        private static Mesh ConvertSolidToRhinoMesh(object solid)
        {
            object faces = RevitReflectionHelper.GetPropertyValue(solid, "Faces");
            IEnumerable enumerable = faces as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            var output = new Mesh();
            foreach (object face in enumerable)
            {
                if (face == null)
                {
                    continue;
                }

                object revitMesh = RevitReflectionHelper.InvokeMethod(face, "Triangulate");
                if (revitMesh == null)
                {
                    continue;
                }

                Mesh rhinoMesh = DecodeRevitMesh(revitMesh);
                if (rhinoMesh == null)
                {
                    continue;
                }

                output.Append(rhinoMesh);
            }

            if (output.Faces.Count == 0)
            {
                return null;
            }

            try
            {
                output.Normals.ComputeNormals();
                output.Compact();
            }
            catch
            {
            }

            return output;
        }

        private static Mesh DecodeRevitMesh(object revitMesh)
        {
            Assembly convertAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, "RhinoInside.Revit", StringComparison.Ordinal));

            if (convertAssembly == null)
            {
                try
                {
                    convertAssembly = Assembly.Load("RhinoInside.Revit");
                }
                catch
                {
                    return null;
                }
            }

            Type decoderType = convertAssembly.GetType("RhinoInside.Revit.Convert.Geometry.GeometryDecoder", false, false);
            if (decoderType == null)
            {
                return null;
            }

            foreach (MethodInfo method in decoderType.GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                if (!string.Equals(method.Name, "ToMesh", StringComparison.Ordinal))
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    continue;
                }

                if (!parameters[0].ParameterType.IsInstanceOfType(revitMesh))
                {
                    continue;
                }

                try
                {
                    return method.Invoke(null, new[] { revitMesh }) as Mesh;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static double? TryReadDouble(object value)
        {
            if (value is double doubleValue)
            {
                return doubleValue;
            }

            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return longValue;
            }

            if (value == null)
            {
                return null;
            }

            if (double.TryParse(value.ToString(), out double parsed))
            {
                return parsed;
            }

            return null;
        }
    }
}
