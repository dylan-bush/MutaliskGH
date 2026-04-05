using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;
using MutaliskGH.Framework;

namespace MutaliskGH
{
    public class MutaliskGHInfo : GH_AssemblyInfo
    {
        public override string Name => "MutaliskGH";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => IconLoader.Load("MutaliskGH-1COL.png");

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("2f8080e2-04ed-447a-837e-1d1b7ed168b4");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}
