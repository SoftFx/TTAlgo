using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Serialization.Json;

namespace TickTrader.Algo.Tools.MetadataBuilder
{
    internal sealed class MetadataBuilder
    {
        private const string ApiFileName = "TickTrader.Algo.Api.dll";
        private const string TradeBotAttributeFullName = "TickTrader.Algo.Api.TradeBotAttribute";

        private readonly Action<string> _print;

        private readonly string _mainFileName;
        private readonly string _mainDllPath;
        private readonly string _apiDllPath;


        private string DefaultMetadataName => $"{Path.GetFileNameWithoutExtension(_mainFileName)}_manifest";


        public string OutputFolder { get; set; }

        public string MetadataFileName { get; set; }

        public string Author { get; set; }

        public string Repository { get; set; }

        public string ArtifactName { get; set; }


        internal MetadataBuilder(string sourceFolder, string mainFile, Action<string> print)
        {
            _mainFileName = mainFile;

            _mainDllPath = TryGetFilePath(sourceFolder, mainFile);
            _apiDllPath = TryGetFilePath(sourceFolder, ApiFileName);

            _print = print;

            _print("\nBuilding metadata...");
        }


        public MetadataInfo BuildBaseInformation(string packagePath, string srcFolder)
        {
            _print("\tBuilding base info...");

            return new MetadataInfo()
            {
                Author = Author,
                Source = Repository,
                ArtifactName = ArtifactName,

                PackageSize = GetPackageSize(packagePath),
                LastUpdate = GetLastUpdate(srcFolder),
                BuildDate = GetBuildDate(),
            };
        }

        public MetadataInfo FillReflectionInfo(MetadataInfo info)
        {
            _print("\tStarting reflection...");

            _print($"\tMain file: {_mainDllPath}");
            _print($"\tApi file: {_apiDllPath}");

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_apiDllPath);

            info.ApiVersion = GetApiVersion(assembly);

            assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_mainDllPath);

            foreach (var assemblyType in assembly.GetTypes())
            {
                foreach (var rawType in assemblyType.GetCustomAttributes())
                {
                    var castType = rawType.TypeId as Type;

                    if (castType.FullName == TradeBotAttributeFullName)
                    {
                        foreach (var prop in castType.GetProperties())
                        {
                            string ReadValue()
                            {
                                var value = $"{prop.GetValue(rawType)}";

                                _print($"\t{prop.Name}={value}");

                                return value;
                            }

                            switch (prop.Name)
                            {
                                case nameof(MetadataInfo.DisplayName):
                                    info.DisplayName = ReadValue();
                                    break;

                                case nameof(MetadataInfo.Copyright):
                                    info.Copyright = ReadValue();
                                    break;

                                case nameof(MetadataInfo.Description):
                                    info.Description = ReadValue();
                                    break;

                                case nameof(MetadataInfo.Category):
                                    info.Category = ReadValue();
                                    break;

                                case nameof(MetadataInfo.Version):
                                    info.Version = ReadValue();
                                    break;
                            }
                        }
                    }
                }
            }

            return info;
        }

        public void SaveMetadata(MetadataInfo info)
        {
            var manifestName = string.IsNullOrEmpty(MetadataFileName) ? DefaultMetadataName : MetadataFileName;

            var path = Path.Combine(OutputFolder, $"{manifestName}.json");
            var serializator = new DataContractJsonSerializer(typeof(MetadataInfo));

            using (var mStream = new MemoryStream())
            {
                _print("\tOpening memory stream...");

                serializator.WriteObject(mStream, info);

                mStream.Position = 0;

                _print($"\tWriting info to {path}...");

                using (var fs = File.Create(path))
                {
                    mStream.CopyTo(fs);
                }

                _print("\tInfo has been written");
            }

            _print("Done.");
        }


        private string GetApiVersion(Assembly assembly)
        {
            _print("\tGetting api version...");

            var value = assembly.GetName()?.Version.ToString();

            _print($"\t{nameof(MetadataInfo.ApiVersion)}={value}");

            return value;
        }

        private string GetBuildDate()
        {
            _print("\tGetting build date...");

            var value = DateTime.UtcNow.ToString(MetadataInfo.DateTimeFormat);

            _print($"\t{nameof(MetadataInfo.BuildDate)}={value}");

            return value;
        }

        private long GetPackageSize(string packagePath)
        {
            _print("\tGetting package size...");

            var info = new FileInfo(packagePath);
            var value = info?.Length ?? 0L;

            _print($"\t{nameof(MetadataInfo.PackageSize)}={value}");

            return value;
        }

        private string GetLastUpdate(string srcFolder)
        {
            _print($"\tReading last update in folder {srcFolder}...");

            var files = Directory.GetFiles(srcFolder);
            var lastUpdate = DateTime.MinValue;

            foreach (var filePath in files)
            {
                var fi = new FileInfo(filePath);

                if (fi.LastWriteTimeUtc > lastUpdate)
                    lastUpdate = fi.LastWriteTimeUtc;
            }

            var value = (lastUpdate == DateTime.MinValue ? DateTime.UtcNow : lastUpdate).ToString(MetadataInfo.DateTimeFormat);

            _print($"\t{nameof(MetadataInfo.LastUpdate)}={value}");

            return value;
        }

        private static string TryGetFilePath(string folder, string fileName)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, folder, fileName);

            if (!File.Exists(filePath))
                throw new Exception($"Cannot find file {filePath}");

            return filePath;
        }
    }
}
