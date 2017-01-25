using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.VS.Package
{
    public class CsProject : FlavoredProjectBase, IVsProjectFlavorCfgProvider, IVsBuildStatusCallback
    {
        private VSPackage package;
        private IVsProjectFlavorCfgProvider innerVsProjectFlavorCfgProvider;
        private Project dteProject;
        private DTE dteObj;
        private string projectFolder;
        private string projectName;
        private BuildEvents buildEvents;

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
        }

        protected override void InitializeForOuter(string fileName, string location, string name, uint flags, ref Guid guidProject, out bool cancel)
        {
            base.InitializeForOuter(fileName, location, name, flags, ref guidProject, out cancel);

            object extObject;
            ErrorHandler.ThrowOnFailure(GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out extObject));
            dteProject = (EnvDTE.Project)extObject;
            dteObj = dteProject.DTE;
            projectFolder = Path.GetDirectoryName(dteProject.FullName);
            projectName = dteProject.Name;
            this.buildEvents = dteObj.Events.BuildEvents;

            buildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

            AdjustApiReference();
        }

        private void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            if (Project != dteProject.UniqueName) // filter out other projects
                return;

            if (!Success)
                return;

            try
            {
                var projectProps = dteProject.Properties;
                var solutionProps = dteObj.Solution.Properties;
                var buildProps = dteProject.ConfigurationManager.ActiveConfiguration.Properties;

                var projectFolderPath = projectProps.GetString("FullPath");
                var projectFileName = projectProps.GetString("FileName");
                var projectFilePath = Path.Combine(projectFolderPath, projectFileName);
                var outputFolder = buildProps.GetString("OutputPath");
                var outputFolderPath = Path.Combine(projectFolderPath, outputFolder);

                PackageWriter package = new PackageWriter(WriteLineToBuild);
                package.ProjectFile = projectFilePath;
                package.Runtime = projectProps.GetString("TargetFrameworkMoniker");
                package.MainFileName = projectProps.GetString("OutputFileName");
                package.SrcFolder = outputFolderPath;
                package.Workspace = solutionProps.GetString("Path");
                package.Ide = "VS" + dteObj.Version;
                package.Save(EnvService.AlgoCommonRepositoryFolder);
            }
            catch (System.IO.IOException ioex)
            {
                WriteLineToBuild("error: Failed to build algo package! " + ioex.Message);
            }
            catch (Exception ex)
            {
                WriteLineToBuild("error:Failed to build algo package! " + ex.ToString());
            }
        }

        //private string PrintProperties(Properties props)
        //{
        //    StringBuilder builder = new StringBuilder();
        //    foreach (Property property in props)
        //    {
        //        builder.Append(property.Name).Append("=");

        //        try
        //        {
        //            var value = property.Value;
        //            builder.Append(value);
        //        }
        //        catch (Exception)
        //        {
        //            builder.Append("{ERROR}");
        //        }

        //        builder.AppendLine();
        //    }
        //    return builder.ToString();
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

        //private void RemoveFromCLSIDList(ref string pageList, string pageGuidString)
        //{
        //    // Remove the specified page guid from the string of guids.
        //    int index = pageList.IndexOf(pageGuidString, StringComparison.OrdinalIgnoreCase);

        //    if (index != -1)
        //    {
        //        // Guids are separated by ';', so we need to ensure we remove the ';' 
        //        // when removing the last guid in the list.
        //        int index2 = index + pageGuidString.Length + 1;
        //        if (index2 >= pageList.Length)
        //        {
        //            pageList = pageList.Substring(0, index).TrimEnd(';');
        //        }
        //        else
        //        {
        //            pageList = pageList.Substring(0, index) + pageList.Substring(index2);
        //        }
        //    }
        //    else
        //    {
        //        throw new ArgumentException(
        //            string.Format("Cannot find the Page {0} in the Page List {1}",
        //            pageGuidString, pageList));
        //    }
        //}

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

        private void AdjustApiReference()
        {
            WriteToGeneral("Loading project " + projectName + " ...\r\n");

            VSLangProj.VSProject vsProject = (VSLangProj.VSProject)dteProject.Object;
            var references = vsProject.References.Cast<VSLangProj.Reference>();
            var apiRef = references.FirstOrDefault(r => r.Name == EnvService.ApiAssemblyName);
            var apiSrc = GetLatestApiCopy();
            var apiProjectDefPath = Path.Combine(projectFolder, EnvService.ApiAssemblyFileName);

            if (apiSrc != null)
                WriteToGeneral("Available Api version: " + apiSrc.Version + "\r\n");
            else
            {
                WriteToGeneral("Cannot find/load API asembly! Reference update is not possible.\r\n");
                return;
            }

            if (apiRef == null)
            {
                WriteToGeneral("Project has no API refenrece.\r\n");

                // add reference
                try
                {
                    apiSrc.Save(apiProjectDefPath);
                    var newRef = vsProject.References.Add(apiProjectDefPath);
                    newRef.CopyLocal = false;
                    WriteToGeneral("Added reference to API v." + apiSrc.Version + "\r\n");
                }
                catch (Exception ex)
                {
                    WriteToGeneral("Failed to add API reference! " + ex.Message + "\r\n");
                }
            }
            else
            {
                // update reference if necessary
                try
                {
                    // check location
                    var existingApiRefPath = apiRef.Path;
                    if (!ComparePaths(existingApiRefPath, apiProjectDefPath))
                        return; // User has relocated API dll. Let's keep it as it is.

                    // check version
                    var refVersion = GetVersionOrNull(existingApiRefPath);
                    if (refVersion != null)
                        WriteToGeneral("Project Api version: " + refVersion + "\r\n");
                    if (refVersion == null || refVersion < apiSrc.Version)
                    {
                        apiSrc.Save(existingApiRefPath);
                        WriteToGeneral("Updated reference to API v." + apiSrc.Version + "\r\n");
                    }
                }
                catch (Exception ex)
                {
                    WriteToGeneral("Failed to update API reference! " + ex.Message + "\r\n");
                }
            }
        }

        private Version GetVersionOrNull(string path)
        {
            try
            {
                var versionStr = FileVersionInfo.GetVersionInfo(path).FileVersion;
                return new Version(versionStr);
            }
            catch (Exception ex)
            {
                WriteToGeneral("Error: " + ex.Message);
                return null;
            }
        }

        private AssemblyCopy GetLatestApiCopy()
        {
            var commonApiPath = Path.Combine(EnvService.AlgoCommonApiFolder, EnvService.ApiAssemblyFileName);
            var apiFromCommon = new AssemblyCopy(commonApiPath);

            if (apiFromCommon.IsValid)
                return apiFromCommon;

            if (apiFromCommon.LoadErrorMessage != null)
                WriteToGeneral("Error: " + apiFromCommon.LoadErrorMessage + "\r\n");

            string packageApiPath = Path.Combine(EnvService.PackageFolder, EnvService.ApiAssemblyFileName);
            var apiFromPackage = new AssemblyCopy(packageApiPath);

            if (apiFromPackage.IsValid)
                return apiFromPackage;

            if (apiFromPackage.LoadErrorMessage != null)
                WriteToGeneral("Error: " + apiFromPackage.LoadErrorMessage + "\r\n");

            return null;
        }

        private void WriteToGeneral(string message)
        {
            package.OutputPane_General?.OutputString(message);
        }

        private void WriteLineToBuild(string message)
        {
            WriteToBuild(message + "\r\n");
        }

        private void WriteToBuild(string message)
        {
            package.OutputPane_Build?.OutputString(message);
        }

        public static bool ComparePaths(string path1, string path2)
        {
            return NormalizePath(path1) == NormalizePath(path2);
        }

        public static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                       .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                       .ToUpperInvariant();
        }
    }
}
