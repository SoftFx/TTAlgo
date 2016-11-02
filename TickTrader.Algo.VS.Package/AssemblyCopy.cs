using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    internal class AssemblyCopy
    {
        private byte[] bytes;

        public AssemblyCopy(string path)
        {
            try
            {
                var versionStr = FileVersionInfo.GetVersionInfo(path).FileVersion;
                Version = new Version(versionStr);
                bytes = File.ReadAllBytes(path);
                IsValid = true;
            }
            catch (Exception ex)
            {
                LoadErrorMessage = ex.Message;
                IsValid = false;
            }
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, bytes);
        }

        public Version Version { get; private set; }
        public string LoadErrorMessage { get; private set; }
        public bool IsValid { get; private set; }
    }
}
