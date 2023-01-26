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
        private readonly string _sourceFolder;


        private string DefaultMetadataName => $"{Path.GetFileNameWithoutExtension(_mainFileName)}_metainfo";


        public string OutputFolder { get; set; }

        public string MetadataFileName { get; set; }

        public string Author { get; set; }

        public string Repository { get; set; }

        public string ArtifactName { get; set; }


        internal MetadataBuilder(string sourceFolder, string mainFile, Action<string> print)
        {
            _sourceFolder = sourceFolder;
            _mainFileName = mainFile;

            _mainDllPath = TryGetFilePath(_sourceFolder, mainFile);
            _apiDllPath = TryGetFilePath(_sourceFolder, ApiFileName);

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

        public MetadataInfo FillReflectionInfo(MetadataInfo packageInfo)
        {
            _print("\tStarting reflection...");

            _print($"\tMain file: {_mainDllPath}");
            _print($"\tApi file: {_apiDllPath}");

            LoadExtraAssemblies();

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_apiDllPath);

            packageInfo.ApiVersion = GetApiVersion(assembly);

            assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(_mainDllPath);

            foreach (var assemblyType in assembly.GetTypes())
            {
                foreach (var rawType in assemblyType.GetCustomAttributes())
                {
                    var castType = rawType.TypeId as Type;

                    if (castType.FullName == TradeBotAttributeFullName)
                    {
                        var info = new PluginsInfo();

                        foreach (var prop in castType.GetProperties())
                        {
                            string ReadValue()
                            {
                                var value = $"{prop.GetValue(rawType)}";

                                _print($"\t\t{prop.Name}={value}");

                                return value;
                            }

                            switch (prop.Name)
                            {
                                case nameof(PluginsInfo.DisplayName):
                                    info.DisplayName = ReadValue();
                                    break;

                                case nameof(PluginsInfo.Copyright):
                                    info.Copyright = ReadValue();
                                    break;

                                case nameof(PluginsInfo.Description):
                                    info.Description = ReadValue();
                                    break;

                                case nameof(PluginsInfo.Category):
                                    info.Category = ReadValue();
                                    break;

                                case nameof(PluginsInfo.Version):
                                    info.Version = ReadValue();
                                    break;
                            }
                        }

                        _print(Environment.NewLine);

                        packageInfo.Plugins.Add(info);
                    }
                }
            }

            return packageInfo;
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

        private void LoadExtraAssemblies()
        {
            foreach (string dll in Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, _sourceFolder), "*.dll"))
            {
                if (dll != _mainDllPath && dll != _apiDllPath)
                {
                    _print($"Loading new assembly: {dll}");

                    AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    _print($"New assembly has been loaded: {dll}");
                }
            }
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
