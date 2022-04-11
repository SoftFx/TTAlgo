using CsvHelper;
using CsvHelper.Configuration;
using Google.Protobuf;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.BacktesterApi
{
    public class BacktesterResults
    {
        public const string ConfigFileName = "config.zip";
        public const string ExecStatusFileName = "status.json";
        public const string StatsFileName = "stats.json";
        public const string PluginInfoFileName = "plugin-info.json";
        public const string FeedFilePrefix = "feed.";
        public const string OutputFilePrefix = "output.";
        public const string JournalFileName = "journal.csv";
        public const string EquityFileName = "equity.csv";
        public const string MarginFileName = "margin.csv";
        public const string TradeHistoryFileName = "trade-history.csv";

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };

        private readonly string _path;


        public ExecutionStatus ExecStatus { get; private set; }

        public TestingStatistics Stats { get; private set; }

        public Dictionary<string, List<BarData>> Feed { get; } = new Dictionary<string, List<BarData>>();

        public Dictionary<string, List<OutputPoint>> Outputs { get; } = new Dictionary<string, List<OutputPoint>>();

        public List<BarData> Equity { get; } = new List<BarData>();

        public List<BarData> Margin { get; } = new List<BarData>();

        public List<PluginLogRecord> Journal { get; } = new List<PluginLogRecord>();

        public List<TradeReportInfo> TradeHistory { get; } = new List<TradeReportInfo>();

        public PluginDescriptor PluginInfo { get; private set; }


        public BacktesterResults(string path)
        {
            _path = path;
        }


        public static BacktesterResults Load(string path)
        {
            if (File.Exists(path))
                return LoadFromZipPath(path);
            if (Directory.Exists(path))
                return LoadFromDirPath(path);

            throw new ArgumentException($"Provided path doesn't exist: '{path}'");
        }

        public static BacktesterResults LoadFromDirPath(string dirPath)
        {
            var res = new BacktesterResults(dirPath)
            {
                ExecStatus = AsFile.TryReadJson<ExecutionStatus>(dirPath)
            };
            return res;
        }

        public static BacktesterResults LoadFromZipPath(string filePath)
        {
            var res = new BacktesterResults(filePath);
            using (var file = File.Open(filePath, FileMode.Open))
            using (var zip = new ZipArchive(file))
            {
                res.ExecStatus = AsZipEntry.TryReadJson<ExecutionStatus>(zip, ExecStatusFileName);
                res.Stats = AsZipEntry.TryReadJson<TestingStatistics>(zip, StatsFileName);
                res.PluginInfo = AsZipEntry.TryReadProtoJson<PluginDescriptor>(zip, PluginInfoFileName, PluginDescriptor.JsonParser);
                foreach (var entry in zip.Entries)
                {
                    var entryName = entry.Name;
                    if (entryName.StartsWith(FeedFilePrefix))
                    {
                        var symbol = Path.GetFileNameWithoutExtension(entryName).Substring(5);
                        var data = new List<BarData>();
                        AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, entryName, data);
                        res.Feed.Add(symbol, data);
                    }
                    else if (entryName.StartsWith(OutputFilePrefix))
                    {
                        var outputId = Path.GetFileNameWithoutExtension(entryName).Substring(7);
                        var data = new List<OutputPoint>();
                        AsZipEntry.TryReadOutputData(zip, entryName, data);
                        res.Outputs.Add(outputId, data);
                    }
                }

                AsZipEntry.TryReadCsv<PluginLogRecord, CsvMapping.ForLogRecord>(zip, JournalFileName, res.Journal);
                AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, EquityFileName, res.Equity);
                AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, MarginFileName, res.Margin);
                AsZipEntry.TryReadCsv<TradeReportInfo, CsvMapping.ForTradeReport>(zip, TradeHistoryFileName, res.TradeHistory);
            }
            return res;
        }


        public BacktesterConfig GetConfig()
        {
            var path = _path;
            if (Directory.Exists(path))
                return BacktesterConfig.Load(Path.Combine(path, ConfigFileName));

            if (File.Exists(path))
            {
                using (var file = File.Open(path, FileMode.Open))
                using (var zip = new ZipArchive(file))
                {
                    var entry = zip.GetEntry(ConfigFileName);
                    if (entry != null)
                        return BacktesterConfig.Load(entry.Open());
                }
            }

            throw new AlgoException($"Backtester config not found in '{path}'");
        }


        private static void SaveAsJson<T>(Stream stream, T data)
        {
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, data, _jsonOptions);
            }
        }

        private static void SaveAsProtoJson(Stream stream, IMessage data, JsonFormatter jsonFormatter)
        {
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(jsonFormatter.Format(data));
            }
        }

        private static void SaveAsCsv<T, TMap>(Stream stream, IEnumerable<T> data)
            where TMap : ClassMap<T>
        {
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                csv.WriteRecords(data);
            }
        }

        private static T ReadAsJson<T>(Stream stream, long length)
        {
            if (length > int.MaxValue)
                return default;

            var size = (int)length;
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                stream.Read(buffer, 0, size);
                return JsonSerializer.Deserialize<T>(buffer.AsSpan(0, size), _jsonOptions);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static T ReadAsProtoJson<T>(Stream stream, JsonParser jsonParser)
            where T : IMessage, new()
        {
            using (var reader = new StreamReader(stream))
            {
                return jsonParser.Parse<T>(reader);
            }
        }

        private static void ReadAsCsv<T, TMap>(Stream stream, List<T> storage)
            where TMap : ClassMap<T>
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                storage.AddRange(csv.GetRecords<T>());
            }
        }

        private static void ReadAsOutputData(Stream stream, List<OutputPoint> storage)
        {
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                if (!csv.Read())
                    return;

                if (!csv.ReadHeader())
                    return;

                Func<IReaderRow, OutputPoint> parser = null;
                switch (csv.HeaderRecord[0])
                {
                    case CsvMapping.ForDoublePoint.Header: parser = CsvMapping.ForDoublePoint.Read; break;
                    case CsvMapping.ForMarkerPoint.Header: parser = CsvMapping.ForMarkerPoint.Read; break;
                    default: return;
                }

                while (csv.Read())
                {
                    storage.Add(parser(csv));
                }
            }
        }


        public static class AsZipEntry
        {
            public static void SaveJson<T>(ZipArchive zip, string entryName, T data)
            {
                using (var stream = CreateZipEntryStream(zip, entryName))
                    SaveAsJson(stream, data);
            }

            public static void SaveProtoJson<T>(ZipArchive zip, string entryName, IMessage data, JsonFormatter jsonFormatter)
            {
                using (var stream = CreateZipEntryStream(zip, entryName))
                    SaveAsProtoJson(stream, data, jsonFormatter);
            }

            public static void SaveCsv<T, TMap>(ZipArchive zip, string entryName, IEnumerable<T> data)
                where TMap : ClassMap<T>
            {
                using (var stream = CreateZipEntryStream(zip, entryName))
                    SaveAsCsv<T, TMap>(stream, data);
            }

            public static T TryReadJson<T>(ZipArchive zip, string entryName)
            {
                if (!TryGetZipEntry(zip, entryName, out var entry))
                    return default;

                using (var stream = entry.Open())
                {
                    return ReadAsJson<T>(stream, entry.Length);
                }
            }

            public static T TryReadProtoJson<T>(ZipArchive zip, string entryName, JsonParser jsonParser)
                where T : IMessage, new()
            {
                if (!TryGetZipEntry(zip, entryName, out var entry))
                    return default;

                using (var stream = entry.Open())
                    return ReadAsProtoJson<T>(stream, jsonParser);
            }

            public static void TryReadCsv<T, TMap>(ZipArchive zip, string entryName, List<T> storage)
                where TMap : ClassMap<T>
            {
                if (!TryGetZipEntry(zip, entryName, out var entry))
                    return;

                using (var stream = entry.Open())
                    ReadAsCsv<T, TMap>(stream, storage);
            }

            public static void TryReadOutputData(ZipArchive zip, string entryName, List<OutputPoint> storage)
            {
                if (!TryGetZipEntry(zip, entryName, out var entry))
                    return;

                using (var stream = entry.Open())
                    ReadAsOutputData(stream, storage);
            }


            private static Stream CreateZipEntryStream(ZipArchive zip, string entryName)
            {
                var entry = zip.CreateEntry(entryName);
                return entry.Open();
            }

            private static bool TryGetZipEntry(ZipArchive zip, string entryName, out ZipArchiveEntry entry)
            {
                entry = zip.GetEntry(entryName);
                return entry != null;
            }
        }

        public static class AsFile
        {
            public static void SaveJson<T>(string path, T data)
            {
                using (var file = CreateFileStream(path))
                    SaveAsJson(file, data);
            }

            public static void SaveProtoJson(string path, IMessage data, JsonFormatter jsonFormatter)
            {
                using (var file = File.Open(path, FileMode.Create))
                    SaveAsProtoJson(file, data, jsonFormatter);
            }

            public static void SaveCsv<T, TMap>(string path, IEnumerable<T> data)
                where TMap : ClassMap<T>
            {
                using (var file = File.Open(path, FileMode.Create))
                    SaveAsCsv<T, TMap>(file, data);
            }

            public static T TryReadJson<T>(string path)
            {
                if (!TryGetFileStream(path, out var file))
                    return default;

                using (file)
                    return ReadAsJson<T>(file, file.Length);
            }

            public static T TryReadProtoJson<T>(string path, JsonParser parser)
                where T : IMessage, new()
            {
                if (!TryGetFileStream(path, out var file))
                    return default;

                using (file)
                    return ReadAsProtoJson<T>(file, parser);
            }

            public static void TryReadCsv<T, TMap>(string path, List<T> storage)
                where TMap : ClassMap<T>
            {
                if (!TryGetFileStream(path, out var file))
                    return;

                using (file)
                    ReadAsCsv<T, TMap>(file, storage);
            }

            public static void TryReadOutputData(string path, List<OutputPoint> storage)
            {
                if (!TryGetFileStream(path, out var file))
                    return;

                using (file)
                    ReadAsOutputData(file, storage);
            }


            private static Stream CreateFileStream(string path)
            {
                return File.Open(path, FileMode.Create);
            }

            private static bool TryGetFileStream(string path, out Stream file)
            {
                file = default;
                if (!File.Exists(path))
                    return false;

                file = File.Open(path, FileMode.Open);
                return true;
            }
        }


        public static class Internal
        {
            public static async Task<string> CreateResultsDir(string parentDir, string configPath)
            {
                var dirNamePrefix = Path.GetFileNameWithoutExtension(configPath);

                var cnt = 0;
                while (cnt < 16)
                {
                    var dirPath = Path.Combine(parentDir, $"{dirNamePrefix}.{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}");
                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                        try
                        {
                            File.Copy(configPath, Path.Combine(dirPath, ConfigFileName));
                            return dirPath;
                        }
                        catch (IOException) { } // in concurrent scenarios file might already be created by another thread due to race condition
                    }

                    await Task.Delay(1000);
                }

                throw new AlgoException($"Timeout while trying to create backtester results directory with prefix {dirNamePrefix} in '{parentDir}'");
            }

            public static void CompressResultsDir(string dirPath)
            {
                var fileName = Path.GetDirectoryName(dirPath) + ".zip";
                ZipFile.CreateFromDirectory(dirPath, Path.Combine(dirPath, "..", fileName));
            }

            public static ExecutionStatus TryReadExecStatus(string resultsDirPath) => AsFile.TryReadJson<ExecutionStatus>(Path.Combine(resultsDirPath, ExecStatusFileName));

            public static void SaveExecStatus(string resultsDirPath, ExecutionStatus status) => AsFile.SaveJson(Path.Combine(resultsDirPath, ExecStatusFileName), status);

            public static void SaveStats(string resultsDirPath, TestingStatistics stats) => AsFile.SaveJson(Path.Combine(resultsDirPath, StatsFileName), stats);

            public static void SavePluginInfo(string resultsDirPath, PluginDescriptor pluginInfo) => AsFile.SaveProtoJson(Path.Combine(resultsDirPath, PluginInfoFileName), pluginInfo, PluginDescriptor.JsonFormatter);

            public static void SaveBarData(string filePath, IEnumerable<BarData> bars) => AsFile.SaveCsv<BarData, CsvMapping.ForBarData>(filePath, bars);

            public static void SaveFeedData(string resultsDirPath, string symbolName, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, $"{FeedFilePrefix}{symbolName}.csv"), bars);

            public static void SaveOutputData(string resultsDirPath, string outputName, IReadOnlyList<OutputPoint> points)
            {
                var filePath = Path.Combine(resultsDirPath, $"{OutputFilePrefix}{outputName}.csv");
                var first = points[0];
                if (first.Metadata is MarkerInfo)
                    AsFile.SaveCsv<CsvMapping.MarkerPointWrapper, CsvMapping.ForMarkerPoint>(filePath, points.Select(p => new CsvMapping.MarkerPointWrapper(p)));
                else
                    AsFile.SaveCsv<OutputPoint, CsvMapping.ForDoublePoint>(filePath, points);
            }

            public static void SaveEquity(string resultsDirPath, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, EquityFileName), bars);

            public static void SaveMargin(string resultsDirPath, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, MarginFileName), bars);

            public static void SaveTradeHistory(string resultsDirPath, IEnumerable<TradeReportInfo> reports) => AsFile.SaveCsv<TradeReportInfo, CsvMapping.ForTradeReport>(Path.Combine(resultsDirPath, "trade-history.csv"), reports);
        }
    }
}
