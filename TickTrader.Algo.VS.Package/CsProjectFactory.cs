using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    [Guid(ProjectGuidString)]
    public class CsProjectFactory : FlavoredProjectFactoryBase
    {
        public const string ProjectGuidString = "74F46FEE-74DF-49D2-A1A3-F634C4CE65AE";

        private VSPackage package;

        public CsProjectFactory(VSPackage package)
            : base()
        {
            this.package = package;
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            CsProject newProject = new CsProject();
            newProject.SetPackage(package);
            return newProject;
        }
    }
}
