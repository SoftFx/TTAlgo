using System;
using System.IO;
using System.Threading;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Package.V1
{
    public class PackageWriter
    {
        public const string DefaultExtension = ".ttalgo";

        private readonly Action<string> _trace;

        public string MainFileName { get; set; }

        public string Runtime { get; set; }

        public string ProjectFile { get; set; }

        public string Workspace { get; set; }

        public string SrcFolder { get; set; }

        public string Ide { get; set; }


        public PackageWriter()
        {
            _trace = t => System.Diagnostics.Debug.WriteLine(t);
        }

        public PackageWriter(Action<string> traceWriteAction)
        {
            _trace = traceWriteAction;
        }


        public string Save(string targetFolder, string pkgFileName = null)
        {
            if (string.IsNullOrEmpty(targetFolder))
                throw new ArgumentException("targetFolder is empty.");

            if (string.IsNullOrEmpty(MainFileName))
                throw new Exception("MainFileName is not set.");

            if (string.IsNullOrEmpty(SrcFolder))
                throw new Exception("SrcFolder is not set.");

            if (string.IsNullOrEmpty(pkgFileName))
                pkgFileName = GetPackageNameWithExtension(Path.GetFileNameWithoutExtension(MainFileName));
            else if (!pkgFileName.EndsWith(DefaultExtension))
                pkgFileName = GetPackageNameWithExtension(pkgFileName);

            string pckgPath = Path.Combine(targetFolder, pkgFileName);

            _trace("Creating Algo package...");
            _trace("\tPackage name = " + pkgFileName);
            _trace("\tSource folder = " + SrcFolder);
            _trace("\tOutput file  = " + pckgPath);

            var package = new Package();
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

            Directory.CreateDirectory(targetFolder);

            Save(package, pckgPath);

            _trace("Done.");

            return pckgPath;
        }

        public static string GetPackageNameWithExtension(string pkgFileName) => $"{pkgFileName}{DefaultExtension}";


        private void Save(Package pckg, string path)
        {
            int retry = 1;
            while (true)
            {
                FileStream stream = null;
                try
                {
                    if (TryOpenWrite(path, out stream))
                    {
                        pckg.Save(ref stream);
                        break;
                    }
                }
                finally
                {
                    stream?.Dispose();
                }

                _trace($"File is locked! Retry {retry}...");

                if (++retry <= 10)
                    Thread.Sleep(200);
                else
                    break;
            }
        }

        private static bool TryOpenWrite(string path, out FileStream stream)
        {
            try
            {
                stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
                return true;
            }
            catch (IOException ex)
            {
                if (ex.IsLockException())
                {
                    stream = null;
                    return false;
                }

                throw;
            }
        }
    }
}
