using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using TickTrader.Algo.Package.V1;

namespace TickTrader.Algo.Build
{
    public class CreatePackageV1 : Task
    {
        private static readonly string AlgoCommonRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AlgoRepository");


        [Required]
        public string BinPath { get; set; }

        [Required]
        public string MainAssemblyName { get; set; }

        [Required]
        public string ProjectFilePath { get; set; }

        [Required]
        public string TargetFramework { get; set; }

        public string PackageName { get; set; }

        public string OutputPath { get; set; }


        public override bool Execute()
        {
            try
            {
                var writer = new PackageWriter(msg => Log.LogMessage(MessageImportance.High, msg));
                writer.SrcFolder = BinPath;
                writer.MainFileName = MainAssemblyName;
                writer.ProjectFile = ProjectFilePath;
                writer.Runtime = TargetFramework;

                var dstPath = OutputPath;
                if (string.IsNullOrEmpty(dstPath))
                    dstPath = AlgoCommonRepositoryPath;

                writer.Save(dstPath, PackageName);
            }
            catch (Exception ex)
            {
                Log.LogError("TTAlgo.Build Create PackageV1: {0}", ex.Message);
                return false;
            }

            return true;
        }
    }
}
