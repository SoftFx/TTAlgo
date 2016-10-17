using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TickTrader.Algo.VS.Package
{
    [ComVisible(false)]
    public class CsProjectConfiguration : IVsProjectFlavorCfg, IPersistXMLFragment, IVsDeployableProjectCfg
    {
        private static Dictionary<IVsCfg, CsProjectConfiguration> mapIVsCfgToProjectCfg = new Dictionary<IVsCfg, CsProjectConfiguration>();

        private bool isDirty = false;
        private Dictionary<string, string> propertiesList = new Dictionary<string, string>();
        private IVsHierarchy project;
        private IVsCfg baseConfiguration;
        private IVsProjectFlavorCfg innerConfiguration;

        public void Initialize(CsProject project,
          IVsCfg baseConfiguration, IVsProjectFlavorCfg innerConfiguration)
        {
            this.project = project;
            this.baseConfiguration = baseConfiguration;
            this.innerConfiguration = innerConfiguration;
            mapIVsCfgToProjectCfg.Add(baseConfiguration, this);
        }

        public string this[string propertyName]
        {
            get
            {
                if (propertiesList.ContainsKey(propertyName))
                {
                    return propertiesList[propertyName];
                }
                return String.Empty;
            }
            set
            {
                // Don't do anything if there isn't any real change
                if (this[propertyName] == value)
                {
                    return;
                }

                isDirty = true;
                if (propertiesList.ContainsKey(propertyName))
                {
                    propertiesList.Remove(propertyName);
                }
                propertiesList.Add(propertyName, value);
            }
        }

        #region IVsProjectFlavorCfg

        public int get_CfgType(ref Guid iidCfg, out IntPtr ppCfg)
        {
            ppCfg = IntPtr.Zero;

            if (iidCfg == typeof(IVsDeployableProjectCfg).GUID)
            {
                ppCfg = Marshal.GetComInterfaceForObject(this, typeof(IVsDeployableProjectCfg));
                return VSConstants.S_OK;
            }

            if (innerConfiguration != null)
                return innerConfiguration.get_CfgType(ref iidCfg, out ppCfg);
            else
                return VSConstants.E_NOINTERFACE;
        }

        public int Close()
        {
            mapIVsCfgToProjectCfg.Remove(this.baseConfiguration);
            int hr = this.innerConfiguration.Close();

            if (this.project != null)
            {
                this.project = null;
            }

            if (this.baseConfiguration != null)
            {
                if (Marshal.IsComObject(this.baseConfiguration))
                {
                    Marshal.ReleaseComObject(this.baseConfiguration);
                }
                this.baseConfiguration = null;
            }

            if (this.innerConfiguration != null)
            {
                if (Marshal.IsComObject(this.innerConfiguration))
                {
                    Marshal.ReleaseComObject(this.innerConfiguration);
                }
                this.innerConfiguration = null;
            }
            return hr;
        }

        #endregion IVsProjectFlavorCfg

        #region IPersistXMLFragment

        public int InitNew(ref Guid guidFlavor, uint storage)
        {
            //Return,if it is our guid.
            if (IsAlgoCsProj(ref guidFlavor))
            {
                return VSConstants.S_OK;
            }

            //Forward the call to inner flavor(s).
            if (this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .InitNew(ref guidFlavor, storage);
            }

            return VSConstants.S_OK;
        }

        public int IsFragmentDirty(uint storage, out int pfDirty)
        {
            pfDirty = 0;
            switch (storage)
            {
                // Specifies storage file type to project file.
                case (uint)_PersistStorageType.PST_PROJECT_FILE:
                    if (isDirty)
                    {
                        pfDirty |= 1;
                    }
                    break;

                // Specifies storage file type to user file.
                case (uint)_PersistStorageType.PST_USER_FILE:
                    // Do not store anything in the user file.
                    break;
            }

            // Forward the call to inner flavor(s) 
            if (pfDirty == 0 && this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .IsFragmentDirty(storage, out pfDirty);
            }
            return VSConstants.S_OK;
        }

        public int Load(ref Guid guidFlavor, uint storage, string pszXMLFragment)
        {
            if (IsAlgoCsProj(ref guidFlavor))
            {
                switch (storage)
                {
                    case (uint)_PersistStorageType.PST_PROJECT_FILE:
                        // Load our data from the XML fragment.
                        XmlDocument doc = new XmlDocument();
                        XmlNode node = doc.CreateElement(this.GetType().Name);
                        node.InnerXml = pszXMLFragment;
                        if (node == null || node.FirstChild == null)
                            break;

                        // Load all the properties
                        foreach (XmlNode child in node.FirstChild.ChildNodes)
                        {
                            propertiesList.Add(child.Name, child.InnerText);
                        }
                        break;
                    case (uint)_PersistStorageType.PST_USER_FILE:
                        // Do not store anything in the user file.
                        break;
                }
            }

            // Forward the call to inner flavor(s)
            if (this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .Load(ref guidFlavor, storage, pszXMLFragment);
            }

            return VSConstants.S_OK;
        }

        public int Save(ref Guid guidFlavor, uint storage, out string pbstrXMLFragment, int fClearDirty)
        {
            pbstrXMLFragment = null;

            if (IsAlgoCsProj(ref guidFlavor))
            {
                switch (storage)
                {
                    case (uint)_PersistStorageType.PST_PROJECT_FILE:
                        // Create XML for our data (a string and a bool).
                        XmlDocument doc = new XmlDocument();
                        XmlNode root = doc.CreateElement(this.GetType().Name);

                        foreach (KeyValuePair<string, string> property in propertiesList)
                        {
                            XmlNode node = doc.CreateElement(property.Key);
                            node.AppendChild(doc.CreateTextNode(property.Value));
                            root.AppendChild(node);
                        }

                        doc.AppendChild(root);
                        // Get XML fragment representing our data
                        pbstrXMLFragment = doc.InnerXml;

                        if (fClearDirty != 0)
                            isDirty = false;
                        break;
                    case (uint)_PersistStorageType.PST_USER_FILE:
                        // Do not store anything in the user file.
                        break;
                }
            }

            // Forward the call to inner flavor(s)
            if (this.innerConfiguration != null
                && this.innerConfiguration is IPersistXMLFragment)
            {
                return ((IPersistXMLFragment)this.innerConfiguration)
                    .Save(ref guidFlavor, storage, out pbstrXMLFragment, fClearDirty);
            }

            return VSConstants.S_OK;
        }

        #endregion IPersistXMLFragment

        #region IVsDeployableProjectCfg

        private Microsoft.VisualStudio.Shell.EventSinkCollection adviseSink = new Microsoft.VisualStudio.Shell.EventSinkCollection();
        private Task deployTask;

        public int AdviseDeployStatusCallback(IVsDeployStatusCallback pIVsDeployStatusCallback, out uint pdwCookie)
        {
            if (pIVsDeployStatusCallback == null)
                throw new ArgumentNullException("pIVsDeployStatusCallback");

            pdwCookie = adviseSink.Add(pIVsDeployStatusCallback);
            return VSConstants.S_OK;
        }

        public int UnadviseDeployStatusCallback(uint dwCookie)
        {
            adviseSink.RemoveAt(dwCookie);
            return VSConstants.S_OK;
        }

        public int StartDeploy(IVsOutputWindowPane pIVsOutputWindowPane, uint dwOptions)
        {
            deployTask = Task.Factory.StartNew(() => Task.Delay(10000).Wait());

            return VSConstants.S_OK;
        }

        public int QueryStatusDeploy(out int pfDeployDone)
        {
            if (deployTask != null && deployTask.IsFaulted)
                pfDeployDone = 1;
            else
                pfDeployDone = 0;

            return VSConstants.S_OK;
        }

        public int StopDeploy(int fSync)
        {
            if (fSync == 0)
            {
                // Async Stop
            }
            else
            {
                // Sync Stop
                if (deployTask != null)
                    deployTask.Wait();
            }

            return VSConstants.S_OK;
        }

        public int WaitDeploy(uint dwMilliseconds, int fTickWhenMessageQNotEmpty)
        {
            // Obsolete method
            return VSConstants.S_OK;
        }

        public int QueryStartDeploy(uint dwOptions, int[] pfSupported, int[] pfReady)
        {
            if (pfSupported != null && pfSupported.Length > 0)
                pfSupported[0] = 1;
            if (pfReady != null && pfReady.Length > 0)
            {
                //pfReady[0] = 0;
                //if (deploymentThread != null && !deploymentThread.IsAlive)
                pfReady[0] = 1;
            }
            return VSConstants.S_OK;
        }

        public int Commit(uint dwReserved)
        {
            return VSConstants.S_OK;
        }

        public int Rollback(uint dwReserved)
        {
            return VSConstants.S_OK;
        }

        #endregion IVsDeployableProjectCfg

        private bool IsAlgoCsProj(ref Guid guidFlavor)
        {
            return guidFlavor.Equals(CsProjectFactory.ProjectGuid);
        }
    }
}
