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
    public class CsProject : FlavoredProjectBase, IVsProjectFlavorCfgProvider
    {
        private VSPackage package;
        private IVsProjectFlavorCfgProvider innerVsProjectFlavorCfgProvider;

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

        protected override void Close()
        {
            base.Close();
            if (innerVsProjectFlavorCfgProvider != null)
            {
                if (Marshal.IsComObject(innerVsProjectFlavorCfgProvider))
                    Marshal.ReleaseComObject(innerVsProjectFlavorCfgProvider);
                innerVsProjectFlavorCfgProvider = null;
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

        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg,
            out IVsProjectFlavorCfg ppFlavorCfg)
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
    }
}
