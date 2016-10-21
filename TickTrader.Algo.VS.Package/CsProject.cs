using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    public class CsProject : FlavoredProjectBase, IVsProjectFlavorCfgProvider, IVsBuildStatusCallback
    {
        private VSPackage package;
        private IVsProjectFlavorCfgProvider innerVsProjectFlavorCfgProvider;
        private Project dteProject;
        private DTE dteObj;

        internal void SetPackage(VSPackage package)
        {
            this.package = package;
        }

        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            object objectForIUnknown = null;
            objectForIUnknown = Marshal.GetObjectForIUnknown(innerIUnknown);
            if (serviceProvider == null)
                serviceProvider = package;
            base.SetInnerProject(innerIUnknown);
            innerVsProjectFlavorCfgProvider = objectForIUnknown as IVsProjectFlavorCfgProvider;

            //var dte = serviceProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            //dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            //dte.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;
            
            //var project = dte.Solution.Projects.Item(0);
        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel)
        {
            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);

            object extObject;
            ErrorHandler.ThrowOnFailure(GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));
            dteProject = (EnvDTE.Project)extObject;
            dteObj = dteProject.DTE;

            //StringBuilder builder = new StringBuilder();
            //foreach (Property property in dteProject.Properties)
            //builder.Append(property.Name).Append("=").AppendLine();

            //dteObj.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            dteObj.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

            //IVsSolutionBuildManager buildManager = ((System.IServiceProvider)this).GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;

            //IVsProjectCfg[] ppIVsProjectCfg = new IVsProjectCfg[1];
            //buildManager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, this, ppIVsProjectCfg);

            //IVsBuildableProjectCfg ppIVsBuildableProjectCfg;
            //ppIVsProjectCfg[0].get_BuildableProjectCfg(out ppIVsBuildableProjectCfg);

            //uint pdwCookie;
            //ppIVsBuildableProjectCfg.AdviseBuildStatusCallback(this, out pdwCookie);
        }

        private void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            //var value = dteProject.Properties.OfType<Property>().FirstOrDefault(p => p.Name == 
            //var aCfgProps = PrintProperties(dteProject.ConfigurationManager.ActiveConfiguration.Properties);
            //var prjProps = PrintProperties(dteProject.Properties);
            //var prjName = dteProject.UniqueName;

            //var sManager = ((System.IServiceProvider)this).GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            //var builder = ((System.IServiceProvider)this).GetService(typeof(IVsBuildableProjectCfg)) as IVsBuildableProjectCfg;
            //IVsProject p;

            if (Project != dteProject.UniqueName) // filter out other projects
                return;

            try
            {
                var projectProps = dteProject.Properties;
                var solutionProps = dteObj.Solution.Properties;
                var buildProps = dteProject.ConfigurationManager.ActiveConfiguration.Properties;

                PackageWriter package = new PackageWriter();
                package.ProjectFile = projectProps.GetString("FileName");
                package.ProjectFolder = projectProps.GetString("FullPath");
                package.TargetFramework = projectProps.GetString("TargetFrameworkMoniker");
                package.AssemblyName = projectProps.GetString("OutputFileName");
                package.OutputPath = buildProps.GetString("OutputPath");
                package.SolutionPath = solutionProps.GetString("Path");
                package.VsVersion = dteObj.Version;
                package.SaveToCommonRepository();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Failed to build algo package! " + ex.ToString());
            }
        }

        private string PrintProperties(Properties props)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Property property in props)
            {
                builder.Append(property.Name).Append("=");

                try
                {
                    var value = property.Value;
                    builder.Append(value);
                }
                catch (Exception)
                {
                    builder.Append("{ERROR}");
                }

                builder.AppendLine();
            }
            return builder.ToString();
        }

        //private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        //{
        //    Task.Delay(10000).Wait();
        //}

        protected override void Close()
        {
            base.Close();
            if (innerVsProjectFlavorCfgProvider != null)
            {
                if (Marshal.IsComObject(innerVsProjectFlavorCfgProvider))
                    Marshal.ReleaseComObject(innerVsProjectFlavorCfgProvider);
                innerVsProjectFlavorCfgProvider = null;
            }
            if (dteObj != null)
            {
                dteObj.Events.BuildEvents.OnBuildProjConfigDone -= BuildEvents_OnBuildProjConfigDone;
                dteObj = null;
            }
        }

        protected override int GetProperty(uint itemId, int propId, out object property)
        {
            //if (propId == (int)__VSHPROPID2.VSHPROPID_CfgPropertyPagesCLSIDList)
            //{
            //    // Get a semicolon-delimited list of clsids of the configuration-dependent
            //    // property pages.
            //    ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));

            //    // Add the CustomPropertyPage property page.
            //    property += ';' + typeof(CustomPropertyPage).GUID.ToString("B");

            //    return VSConstants.S_OK;
            //}

            //if (propId == (int)__VSHPROPID2.VSHPROPID_PropertyPagesCLSIDList)
            //{

            //    // Get the list of priority page guids from the base project system.
            //    ErrorHandler.ThrowOnFailure(base.GetProperty(itemId, propId, out property));
            //    string pageList = (string)property;

            //    // Remove the Services page from the project designer.
            //    string servicesPageGuidString = "{43E38D2E-43B8-4204-8225-9357316137A4}";

            //    RemoveFromCLSIDList(ref pageList, servicesPageGuidString);
            //    property = pageList;
            //    return VSConstants.S_OK;
            //}

            return base.GetProperty(itemId, propId, out property);
        }

        private void RemoveFromCLSIDList(ref string pageList, string pageGuidString)
        {
            // Remove the specified page guid from the string of guids.
            int index = pageList.IndexOf(pageGuidString, StringComparison.OrdinalIgnoreCase);

            if (index != -1)
            {
                // Guids are separated by ';', so we need to ensure we remove the ';' 
                // when removing the last guid in the list.
                int index2 = index + pageGuidString.Length + 1;
                if (index2 >= pageList.Length)
                {
                    pageList = pageList.Substring(0, index).TrimEnd(';');
                }
                else
                {
                    pageList = pageList.Substring(0, index) + pageList.Substring(index2);
                }
            }
            else
            {
                throw new ArgumentException(
                    string.Format("Cannot find the Page {0} in the Page List {1}",
                    pageGuidString, pageList));
            }
        }

        #region IVsProjectFlavorCfgProvider Members

        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            IVsProjectFlavorCfg cfg = null;

            if (innerVsProjectFlavorCfgProvider != null)
            {
                innerVsProjectFlavorCfgProvider.
                    CreateProjectFlavorCfg(pBaseProjectCfg, out cfg);
            }

            var configuration = new CsProjectConfiguration();

            configuration.Initialize(this, pBaseProjectCfg, cfg);
            ppFlavorCfg = (IVsProjectFlavorCfg)configuration;

            return VSConstants.S_OK;
        }

        #endregion

        public int BuildBegin(ref int pfContinue)
        {
            return VSConstants.S_OK;
        }

        public int BuildEnd(int fSuccess)
        {
            return VSConstants.S_OK;
        }

        public int Tick(ref int pfContinue)
        {
            return VSConstants.S_OK;
        }
    }
}
