using Grasshopper.Kernel;
using MutaliskGH.Core;
using MutaliskGH.Core.Rhino;
using MutaliskGH.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MutaliskGH.Components.Rhino
{
    public sealed class OpenAcadFileReadOnlyComponent : BaseComponent
    {
        private bool wasTriggered;

        public OpenAcadFileReadOnlyComponent()
            : base(
                "Open ACAD File RO",
                "OpenACADRO",
                "Open a DWG in AutoCAD as read-only on a rising-edge Activate trigger.",
                CategoryNames.Rhino)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("20ce4671-43da-4956-8b70-96f9297ad470"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("File Path", "Path", "Full path to the DWG file.", GH_ParamAccess.item);
            parameterManager.AddBooleanParameter("Activate", "Go", "Run the AutoCAD open action on a rising-edge trigger.", GH_ParamAccess.item, false);

            parameterManager[0].Optional = true;
            parameterManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager parameterManager)
        {
            parameterManager.AddTextParameter("Details", "out", "Execution details and backend information.", GH_ParamAccess.item);
            parameterManager.AddTextParameter("Status", "Status", "Component status code.", GH_ParamAccess.item);
            parameterManager.AddBooleanParameter("Opened", "Opened", "True when the DWG is open in AutoCAD.", GH_ParamAccess.item);
            parameterManager.AddBooleanParameter("Used Existing Instance", "Existing", "True when a running AutoCAD instance was reused.", GH_ParamAccess.item);
        }

        protected override void SolveInstanceCore(IGH_DataAccess dataAccess)
        {
            string filePath = null;
            dataAccess.GetData(0, ref filePath);

            bool activate = false;
            dataAccess.GetData(1, ref activate);

            if (!activate)
            {
                SetOutputs(dataAccess, OpenAcadFileLogic.NoOp());
                wasTriggered = false;
                return;
            }

            if (wasTriggered)
            {
                dataAccess.SetData(0, "Waiting for Activate to reset.");
                dataAccess.SetData(1, OpenAcadFileLogic.StatusNoOp);
                dataAccess.SetData(2, false);
                dataAccess.SetData(3, false);
                return;
            }

            Result<string> validation = OpenAcadFileLogic.ValidateDwgPath(filePath);
            if (validation.IsFailure)
            {
                OpenAcadFileResult error = OpenAcadFileLogic.Error(validation.ErrorMessage, validation.ErrorMessage, OpenAcadFileLogic.NormalizePath(filePath));
                if (validation.ErrorMessage != OpenAcadFileLogic.StatusFileNotFound)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, validation.ErrorMessage);
                }

                SetOutputs(dataAccess, error);
                wasTriggered = activate;
                return;
            }

            OpenAcadFileResult result = OpenReadOnly(validation.Value);
            if (result.Status == OpenAcadFileLogic.StatusAutoCadUnavailable || result.Status == OpenAcadFileLogic.StatusOpenFailed)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, result.Details);
            }

            SetOutputs(dataAccess, result);
            wasTriggered = activate;
        }

        private static void SetOutputs(IGH_DataAccess dataAccess, OpenAcadFileResult result)
        {
            dataAccess.SetData(0, result.Details);
            dataAccess.SetData(1, result.Status);
            dataAccess.SetData(2, result.Opened);
            dataAccess.SetData(3, result.UsedExistingInstance);
        }

        private static OpenAcadFileResult OpenReadOnly(string normalizedPath)
        {
            object application = null;
            bool usedExistingInstance = false;
            string detailsPrefix = "backend=Marshal; path=" + normalizedPath;

            try
            {
                try
                {
                    application = Marshal.GetActiveObject("AutoCAD.Application");
                    usedExistingInstance = true;
                }
                catch
                {
                    application = null;
                }

                if (application == null)
                {
                    Type progType = Type.GetTypeFromProgID("AutoCAD.Application");
                    if (progType == null)
                    {
                        return OpenAcadFileLogic.Error(OpenAcadFileLogic.StatusAutoCadUnavailable, detailsPrefix + "; create=ProgID not found", normalizedPath);
                    }

                    try
                    {
                        application = Activator.CreateInstance(progType);
                        usedExistingInstance = false;
                    }
                    catch (Exception exception)
                    {
                        return OpenAcadFileLogic.Error(OpenAcadFileLogic.StatusAutoCadUnavailable, detailsPrefix + "; create=" + exception.Message, normalizedPath);
                    }
                }

                TrySetVisible(application);

                object documents = application.GetType().InvokeMember("Documents", BindingFlags.GetProperty, null, application, null);
                int count = Convert.ToInt32(documents.GetType().InvokeMember("Count", BindingFlags.GetProperty, null, documents, null));
                for (int index = 0; index < count; index++)
                {
                    object document = documents.GetType().InvokeMember("Item", BindingFlags.InvokeMethod, null, documents, new object[] { index });
                    string fullName = null;
                    try
                    {
                        object rawFullName = document.GetType().InvokeMember("FullName", BindingFlags.GetProperty, null, document, null);
                        fullName = OpenAcadFileLogic.NormalizePath(rawFullName?.ToString());
                    }
                    catch
                    {
                    }

                    if (!string.IsNullOrWhiteSpace(fullName) &&
                        string.Equals(fullName, normalizedPath, StringComparison.OrdinalIgnoreCase))
                    {
                        return OpenAcadFileLogic.Success(OpenAcadFileLogic.StatusAlreadyOpen, usedExistingInstance, detailsPrefix, normalizedPath);
                    }
                }

                object openedDocument = null;
                try
                {
                    openedDocument = documents.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, documents, new object[] { normalizedPath, true });
                }
                catch
                {
                    try
                    {
                        openedDocument = documents.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, documents, new object[] { normalizedPath });
                    }
                    catch (Exception exception)
                    {
                        return OpenAcadFileLogic.Error(OpenAcadFileLogic.StatusOpenFailed, detailsPrefix + "; open=" + exception.Message, normalizedPath);
                    }
                }

                return new OpenAcadFileResult(
                    OpenAcadFileLogic.StatusOpenedReadOnly,
                    openedDocument != null,
                    usedExistingInstance,
                    detailsPrefix,
                    normalizedPath);
            }
            catch (Exception exception)
            {
                return OpenAcadFileLogic.Error(OpenAcadFileLogic.StatusOpenFailed, detailsPrefix + "; error=" + exception.Message, normalizedPath);
            }
        }

        private static void TrySetVisible(object application)
        {
            try
            {
                application?.GetType().InvokeMember("Visible", BindingFlags.SetProperty, null, application, new object[] { true });
            }
            catch
            {
            }
        }
    }
}
