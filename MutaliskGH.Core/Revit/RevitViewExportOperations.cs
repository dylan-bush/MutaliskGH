using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MutaliskGH.Core.Revit
{
    public static class RevitViewExportOperations
    {
        public static Result<string> ExportPdf(object document, object view, string targetPath)
        {
            if (document == null)
            {
                return Result<string>.Failure("No active Revit document is available.");
            }

            if (view == null)
            {
                return Result<string>.Failure("A Revit view is required.");
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return Result<string>.Failure("A target PDF path is required.");
            }

            string exportDirectory = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrWhiteSpace(exportDirectory))
            {
                return Result<string>.Failure("A target PDF folder is required.");
            }

            Directory.CreateDirectory(exportDirectory);

            Assembly assembly = document.GetType().Assembly;
            Type optionsType = assembly.GetType("Autodesk.Revit.DB.PDFExportOptions", false, false);
            if (optionsType == null)
            {
                return Result<string>.Failure("Revit PDFExportOptions is not available in the current host.");
            }

            object options = Activator.CreateInstance(optionsType);
            SetProperty(options, "Combine", false);

            object viewId = RevitReflectionHelper.GetPropertyValue(view, "Id");
            object viewIdCollection = CreateCollectionFromSingleItem(viewId);
            if (viewIdCollection == null)
            {
                return Result<string>.Failure("The supplied Revit view does not expose a valid ElementId.");
            }

            Dictionary<string, DateTime> beforeSnapshot = SnapshotFiles(exportDirectory, "*.pdf");
            MethodInfo exportMethod = FindPdfExportMethod(document.GetType(), optionsType, viewIdCollection.GetType());
            if (exportMethod == null)
            {
                return Result<string>.Failure("A compatible Revit PDF export method could not be found.");
            }

            object exported;
            try
            {
                exported = exportMethod.Invoke(document, new[] { exportDirectory, viewIdCollection, options });
            }
            catch (TargetInvocationException exception)
            {
                string message = exception.InnerException?.Message ?? exception.Message;
                return Result<string>.Failure(message);
            }

            if (exported is bool boolean && !boolean)
            {
                return Result<string>.Failure("Revit reported that the PDF export failed.");
            }

            string exportedPath = ResolveLatestExportedFile(exportDirectory, "*.pdf", beforeSnapshot, targetPath);
            if (string.IsNullOrWhiteSpace(exportedPath))
            {
                return Result<string>.Failure("No PDF file was produced by the export.");
            }

            try
            {
                if (!PathsEqual(exportedPath, targetPath))
                {
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }

                    File.Move(exportedPath, targetPath);
                }
            }
            catch (Exception exception)
            {
                return Result<string>.Failure("PDF export succeeded but the output file could not be renamed: " + exception.Message);
            }

            return Result<string>.Success(targetPath);
        }

        public static Result<string> ExportDwg(object document, object view, string targetPath)
        {
            if (document == null)
            {
                return Result<string>.Failure("No active Revit document is available.");
            }

            if (view == null)
            {
                return Result<string>.Failure("A Revit view is required.");
            }

            if (string.IsNullOrWhiteSpace(targetPath))
            {
                return Result<string>.Failure("A target DWG path is required.");
            }

            string exportDirectory = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrWhiteSpace(exportDirectory))
            {
                return Result<string>.Failure("A target DWG folder is required.");
            }

            Directory.CreateDirectory(exportDirectory);

            Assembly assembly = document.GetType().Assembly;
            Type optionsType = assembly.GetType("Autodesk.Revit.DB.DWGExportOptions", false, false);
            if (optionsType == null)
            {
                return Result<string>.Failure("Revit DWGExportOptions is not available in the current host.");
            }

            object options = Activator.CreateInstance(optionsType);
            SetProperty(options, "MergedViews", true);

            object viewId = RevitReflectionHelper.GetPropertyValue(view, "Id");
            object viewIdCollection = CreateCollectionFromSingleItem(viewId);
            if (viewIdCollection == null)
            {
                return Result<string>.Failure("The supplied Revit view does not expose a valid ElementId.");
            }

            string baseName = Path.GetFileNameWithoutExtension(targetPath);
            MethodInfo exportMethod = FindDwgExportMethod(document.GetType(), optionsType, viewIdCollection.GetType());
            if (exportMethod == null)
            {
                return Result<string>.Failure("A compatible Revit DWG export method could not be found.");
            }

            object exported;
            try
            {
                exported = exportMethod.Invoke(document, new[] { exportDirectory, baseName, viewIdCollection, options });
            }
            catch (TargetInvocationException exception)
            {
                string message = exception.InnerException?.Message ?? exception.Message;
                return Result<string>.Failure(message);
            }

            if (exported is bool boolean && !boolean)
            {
                return Result<string>.Failure("Revit reported that the DWG export failed.");
            }

            string resolvedPath = ResolveDwgPath(exportDirectory, baseName, targetPath);
            return Result<string>.Success(resolvedPath);
        }

        private static void SetProperty(object target, string propertyName, object value)
        {
            PropertyInfo property = target?.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
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

        private static MethodInfo FindPdfExportMethod(Type documentType, Type optionsType, Type collectionType)
        {
            return documentType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "Export", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 3
                        && parameters[0].ParameterType == typeof(string)
                        && parameters[1].ParameterType.IsAssignableFrom(collectionType)
                        && parameters[2].ParameterType.IsAssignableFrom(optionsType);
                });
        }

        private static MethodInfo FindDwgExportMethod(Type documentType, Type optionsType, Type collectionType)
        {
            return documentType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(method =>
                {
                    if (!string.Equals(method.Name, "Export", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    return parameters.Length == 4
                        && parameters[0].ParameterType == typeof(string)
                        && parameters[1].ParameterType == typeof(string)
                        && parameters[2].ParameterType.IsAssignableFrom(collectionType)
                        && parameters[3].ParameterType.IsAssignableFrom(optionsType);
                });
        }

        private static object CreateCollectionFromSingleItem(object item)
        {
            if (item == null)
            {
                return null;
            }

            Type itemType = item.GetType();
            Type listType = typeof(List<>).MakeGenericType(itemType);
            object list = Activator.CreateInstance(listType);
            MethodInfo addMethod = listType.GetMethod("Add", new[] { itemType });
            addMethod?.Invoke(list, new[] { item });
            return list;
        }

        private static Dictionary<string, DateTime> SnapshotFiles(string directory, string searchPattern)
        {
            var snapshot = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(directory))
            {
                return snapshot;
            }

            foreach (string path in Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly))
            {
                snapshot[path] = File.GetLastWriteTimeUtc(path);
            }

            return snapshot;
        }

        private static string ResolveLatestExportedFile(
            string directory,
            string searchPattern,
            IReadOnlyDictionary<string, DateTime> beforeSnapshot,
            string preferredTargetPath)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            if (File.Exists(preferredTargetPath))
            {
                DateTime lastWrite = File.GetLastWriteTimeUtc(preferredTargetPath);
                if (!beforeSnapshot.TryGetValue(preferredTargetPath, out DateTime beforeWrite) || lastWrite > beforeWrite)
                {
                    return preferredTargetPath;
                }
            }

            string bestPath = null;
            DateTime bestWrite = DateTime.MinValue;

            foreach (string path in Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly))
            {
                DateTime lastWrite = File.GetLastWriteTimeUtc(path);
                if (beforeSnapshot.TryGetValue(path, out DateTime beforeWrite) && lastWrite <= beforeWrite)
                {
                    continue;
                }

                if (lastWrite > bestWrite)
                {
                    bestWrite = lastWrite;
                    bestPath = path;
                }
            }

            if (!string.IsNullOrWhiteSpace(bestPath))
            {
                return bestPath;
            }

            return Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault();
        }

        private static string ResolveDwgPath(string directory, string baseName, string preferredTargetPath)
        {
            if (File.Exists(preferredTargetPath))
            {
                return preferredTargetPath;
            }

            return Directory.GetFiles(directory, baseName + "*.dwg", SearchOption.TopDirectoryOnly)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault() ?? preferredTargetPath;
        }

        private static bool PathsEqual(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
