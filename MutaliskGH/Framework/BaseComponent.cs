using Grasshopper.Kernel;
using MutaliskGH.Core;
using System;
using System.Drawing;

namespace MutaliskGH.Framework
{
    public abstract class BaseComponent : GH_Component
    {
        protected BaseComponent(
            string name,
            string nickname,
            string description,
            string subCategory)
            : base(name, nickname, description, CategoryNames.Plugin, subCategory)
        {
        }

        protected virtual string IconResourceName
        {
            get { return null; }
        }

        protected override Bitmap Icon
        {
            get { return IconLoader.Load(IconResourceName); }
        }

        protected sealed override void SolveInstance(IGH_DataAccess da)
        {
            try
            {
                SolveInstanceCore(da);
            }
            catch (Exception exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, exception.Message);
            }
        }

        protected abstract void SolveInstanceCore(IGH_DataAccess da);

        protected bool ReportFailure<T>(Result<T> result)
        {
            if (result == null || result.IsSuccess)
            {
                return false;
            }

            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, result.ErrorMessage);
            return true;
        }
    }
}
