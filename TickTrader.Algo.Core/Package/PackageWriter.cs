using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class PackageWriter
    {
        public const string MetadataFileName = "metadata.xml";
        public const string DefaultExtension = ".ttalgo";

        private Action<string> trace;

        public string MainFileName { get; set; }
        public string Runtime { get; set; }
        public string ProjectFile { get; set; }
        public string Workspace { get; set; }
        public string SrcFolder { get; set; }
        public string Ide { get; set; }

        public PackageWriter()
        {
            this.trace = t => System.Diagnostics.Debug.WriteLine(t);
        }

        public PackageWriter(Action<string> traceWriteAction)
        {
            this.trace = traceWriteAction;
        }

        public void Save(string targetFolder, string pckgFileName = null)
        {
            if (string.IsNullOrEmpty(targetFolder))
                throw new ArgumentException("targetFolder is empty.");

            if (string.IsNullOrEmpty(MainFileName))
                throw new Exception("MainFileName is not set.");

            if (string.IsNullOrEmpty(SrcFolder))
                throw new Exception("SrcFolder is not set.");

            if (string.IsNullOrEmpty(pckgFileName))
                pckgFileName = Path.GetFileNameWithoutExtension(MainFileName) + DefaultExtension;

            string pckgPath = Path.Combine(targetFolder, pckgFileName);

            trace("Creating algo package...");
            trace("\tPackage name = " + pckgFileName);
            trace("\tSource folder = " + SrcFolder);
            trace("\tOutput file  = " + pckgPath);

            var package = new Algo.Core.Package();
            var files = Directory.GetFiles(SrcFolder);

            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var fileBytes = File.ReadAllBytes(filePath);
                package.AddFile(fileName, fileBytes);
            }

            package.Metadata.Runtime = Runtime;
            package.Metadata.IDE = Ide;
            package.Metadata.ProjectFilePath = ProjectFile;
            package.Metadata.MainBinaryFile = MainFileName;
            package.Metadata.Workspace = Workspace;

            Save(package, pckgPath);

            trace("Done.");
        }


        private void Save(Package pckg, string path)
        {
            int retry = 1;
            while(true)
            {
                FileStream stream;
                if (TryOpenWrite(path, out stream))
                {
                    using (stream) pckg.Save(stream);
                    break;
                }

                trace("File is locked! Retry " + retry + " ...");

                if (++retry <= 10)
                    Thread.Sleep(200);
                else
                    break;
            }
        }

        private bool TryOpenWrite(string path, out FileStream stream)
        {
            try
            {
                stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                return true;
            }
            catch (IOException ex)
            {
                if (ex.IsLockExcpetion())
                {
                    stream = null;
                    return false;
                }

                throw;
            }
        }
    }
}
