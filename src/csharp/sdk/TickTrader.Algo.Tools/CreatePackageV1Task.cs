using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using TickTrader.Algo.Package.V1;

namespace TickTrader.Algo.Tools
{
    public class CreatePackageV1 : Task
    {
        private static readonly string AlgoCommonRepositoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AlgoTerminal", "AlgoRepository");


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


        public bool BuildMetadata { get; set; }

        public string MetadataFileName { get; set; }

        public string Repository { get; set; }

        public string ArtifactName { get; set; }

        public string Author { get; set; }

        public string RuntimeType { get; set; }


        public override bool Execute()
        {
            try
            {
                var mainFilePath = Path.Combine(BinPath, MainAssemblyName);

                if (!File.Exists(mainFilePath))
                {
                    Log.LogError($"Main assembly file not found. Check if project type is library and file exists '{mainFilePath}'");
                    return false;
                }

                var writer = new PackageWriter(Print)
                {
                    SrcFolder = BinPath,
                    MainFileName = MainAssemblyName,
                    ProjectFile = ProjectFilePath,
                    Runtime = TargetFramework
                };

                var dstPath = OutputPath;

                if (string.IsNullOrEmpty(dstPath))
                    dstPath = AlgoCommonRepositoryPath;

                var packagePath = writer.Save(dstPath, PackageName);

                if (BuildMetadata)
                {
                    var builder = new MetadataBuilder.MetadataBuilder(BinPath, MainAssemblyName, Print)
                    {
                        MetadataFileName = MetadataFileName,
                        ArtifactName = ArtifactName,
                        Repository = Repository,
                        Author = Author,

                        OutputFolder = dstPath,
                    };

                    var info = builder.BuildBaseInformation(packagePath, BinPath);

                    Print($"{nameof(RuntimeType)}={RuntimeType}");

                    if (RuntimeType == "Core")
                        info = builder.FillReflectionInfo(info);

                    builder.SaveMetadata(info);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"TTAlgo.Build Create PackageV1 failed: {ex.Message}");
                return false;
            }

            return true;
        }


        private void Print(string msg) => Log.LogMessage(MessageImportance.High, msg);
    }
}
