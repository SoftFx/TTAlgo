using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Json;

namespace TickTrader.Algo.Tools.MetadataBuilder
{
    internal sealed class MetadataBuilder
    {
        private const string TradeBotAttributeFullName = "TickTrader.Algo.Api.TradeBotAttribute";

        private readonly Action<string> _print;

        private readonly string _mainFileName;
        private readonly string _mainDllPath;
        private readonly string _sourceFolder;


        private string DefaultMetadataName => $"{Path.GetFileNameWithoutExtension(_mainFileName)}_metainfo";


        public string OutputFolder { get; set; }

        public string MetadataFileName { get; set; }

        public string Author { get; set; }

        public string Repository { get; set; }

        public string PackageName { get; set; }


        internal MetadataBuilder(string sourceFolder, string mainFile, Action<string> print)
        {
            _sourceFolder = sourceFolder;
            _mainFileName = mainFile;

            _mainDllPath = TryGetFilePath(_sourceFolder, mainFile);

            _print = print;

            _print("\nBuilding metadata...");
        }


        public MetadataInfo BuildBaseInformation(string packagePath)
        {
            _print("\tBuilding base info...");

            return new MetadataInfo()
            {
                Author = Author,
                Source = Repository,
                PackageName = PackageName,

                PackageSize = GetPackageSize(packagePath),
                BuildDate = GetBuildDate(),
            };
        }

        public MetadataInfo FillReflectionInfo(MetadataInfo packageInfo)
        {
            _print("\tStarting reflection...");

            _print($"\tMain file: {_mainDllPath}");

            using (var context = new DynamicAssemblyLoadContext(_sourceFolder, _print))
            {
                var assembly = context.LoadFromAssemblyPath(_mainDllPath);

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

                packageInfo.ApiVersion = GetApiVersion(context);
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


        private string GetApiVersion(DynamicAssemblyLoadContext context)
        {
            _print("\tGetting api version...");

            var value = context.ApiVersion;

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

        private static string TryGetFilePath(string folder, string fileName)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, folder, fileName);

            if (!File.Exists(filePath))
                throw new Exception($"Cannot find file {filePath}");

            return filePath;
        }
    }
}
