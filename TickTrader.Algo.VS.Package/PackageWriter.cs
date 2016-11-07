using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.VS.Package
{
    public class PackageWriter
    {
        public const string MetadataFileName = "metadata.xml";
        public const string DefaultExtension = ".ttalgo";

        private Action<string> trace;

        public string AssemblyName { get; set; }
        public string TargetFramework { get; set; }
        public string ProjectFolder { get; set; }
        public string ProjectFile { get; set; }
        public string SolutionPath { get; set; }
        public string OutputPath { get; set; }
        public string VsVersion { get; set; }

        public PackageWriter()
        {
            this.trace = t => System.Diagnostics.Debug.WriteLine(t);
        }

        public PackageWriter(Action<string> traceWriteAction)
        {
            this.trace = traceWriteAction;
        }

        public void SaveToCommonRepository()
        {
            Save(EnvService.AlgoCommonRepositoryFolder);
        }

        public void Save(string targetFolder, string pckgFileName = null)
        {
            if (string.IsNullOrEmpty(targetFolder))
                throw new ArgumentException("targetFolder is empty.");

            if (string.IsNullOrEmpty(AssemblyName))
                throw new Exception("AssemblyName is not set.");

            if (string.IsNullOrEmpty(ProjectFolder))
                throw new Exception("ProjectFolder is not set.");

            if (string.IsNullOrEmpty(OutputPath))
                throw new Exception("OutputPath is not set.");

            if (string.IsNullOrEmpty(pckgFileName))
                pckgFileName = Path.GetFileNameWithoutExtension(AssemblyName) + DefaultExtension;

            string pckgPath = Path.Combine(targetFolder, pckgFileName);
            string pckgSrcFolder = Path.Combine(ProjectFolder, OutputPath).TrimEnd(Path.DirectorySeparatorChar);

            trace("Creating algo package...");
            trace("\tPackage name = " + pckgFileName);
            trace("\tSource folder = " + pckgSrcFolder);
            trace("\tOutput file  = " + pckgPath);

            var package = new Algo.Core.Package();
            var files = Directory.GetFiles(pckgSrcFolder);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var fileBytes = File.ReadAllBytes(filePath);
                package.AddFile(fileName, fileBytes);
            }

            package.Metadata.Runtime = TargetFramework;
            package.Metadata.IDE = "VS" + VsVersion;
            package.Metadata.ProjectFilePath = Path.Combine(ProjectFolder, ProjectFile).TrimEnd(Path.DirectorySeparatorChar);
            package.Metadata.MainBinaryFile = AssemblyName;
            package.Metadata.Workspace = SolutionPath;

            using (var pckgFs = File.OpenWrite(pckgPath))
                package.Save(pckgFs);

            trace("Done.");
        }
    }
}
