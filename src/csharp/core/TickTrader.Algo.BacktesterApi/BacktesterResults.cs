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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using static TickTrader.Algo.BacktesterApi.CsvMapping;

namespace TickTrader.Algo.BacktesterApi
{
    public class BacktesterResults
    {
        public const string ConfigFileName = "config.zip";
        public const string ExecStatusFileName = "status.json";
        public const string VersionFileName = "version.json";
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

        private VersionInfo _version = new VersionInfo();


        public ExecutionStatus ExecStatus { get; private set; }

        public TestingStatistics Stats { get; private set; }

        public Dictionary<string, List<BarData>> Feed { get; } = new Dictionary<string, List<BarData>>();

        public Dictionary<string, List<OutputPoint>> Outputs { get; } = new Dictionary<string, List<OutputPoint>>();

        public List<BarData> Equity { get; } = new List<BarData>();

        public List<BarData> Margin { get; } = new List<BarData>();

        public List<PluginLogRecord> Journal { get; } = new List<PluginLogRecord>();

        public List<TradeReportInfo> TradeHistory { get; } = new List<TradeReportInfo>();

        public PluginDescriptor PluginInfo { get; private set; }

        public List<ReadError> ReadErrors { get; } = new List<ReadError>();


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
                ExecStatus = AsFile.TryReadJson<ExecutionStatus>(dirPath) ?? ExecutionStatus.NotFound,
            };
            return res;
        }

        public static BacktesterResults LoadFromZipPath(string filePath)
        {
            var res = new BacktesterResults(filePath);
            using (var file = File.Open(filePath, FileMode.Open))
            using (var zip = new ZipArchive(file))
            {
                res.ExecStatus = AsZipEntry.TryReadJson<ExecutionStatus>(zip, ExecStatusFileName, res) ?? ExecutionStatus.NotFound;
                if (!res.ExecStatus.ResultsNotCorrupted)
                    return res;

                res._version = AsZipEntry.TryReadJson<VersionInfo>(zip, VersionFileName, res);
                if (res._version == null)
                {
                    res.ReadErrors.Add(new ReadError(ReadErrorCode.MissingVersion));
                }
                else if (res._version.ResultsVersion > VersionInfo.CurrentVersion)
                {
                    res.ReadErrors.Add(new ReadError(ReadErrorCode.NewerVersion, $"{res._version.ResultsVersion} > {VersionInfo.CurrentVersion}"));
                }

                res.Stats = AsZipEntry.TryReadJson<TestingStatistics>(zip, StatsFileName, res) ?? new TestingStatistics();

                if (res._version?.PluginInfoUri == PluginDescriptor.JsonUri)
                {
                    res.PluginInfo = AsZipEntry.TryReadProtoJson<PluginDescriptor>(zip, PluginInfoFileName, PluginDescriptor.JsonParser, res);
                }
                else if (res._version != null)
                {
                    res.ReadErrors.Add(new ReadError(ReadErrorCode.UnknownPluginInfo, res._version?.PluginInfoUri));
                }

                foreach (var entry in zip.Entries)
                {
                    var entryName = entry.Name;
                    if (entryName.StartsWith(FeedFilePrefix))
                    {
                        var symbol = Path.GetFileNameWithoutExtension(entryName).Substring(5);
                        var data = new List<BarData>();
                        AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, entryName, data, res);
                        res.Feed.Add(symbol, data);
                    }
                    else if (entryName.StartsWith(OutputFilePrefix))
                    {
                        var outputId = Path.GetFileNameWithoutExtension(entryName).Substring(7);
                        var data = new List<OutputPoint>();
                        AsZipEntry.TryReadOutputData(zip, entryName, data, res);
                        res.Outputs.Add(outputId, data);
                    }
                }

                AsZipEntry.TryReadCsv<PluginLogRecord, CsvMapping.ForLogRecord>(zip, JournalFileName, res.Journal, res);
                if (res.PluginInfo != null && res.PluginInfo.Type == Metadata.Types.PluginType.TradeBot)
                {
                    AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, EquityFileName, res.Equity, res);
                    AsZipEntry.TryReadCsv<BarData, CsvMapping.ForBarData>(zip, MarginFileName, res.Margin, res);
                    AsZipEntry.TryReadCsv<TradeReportInfo, CsvMapping.ForTradeReport>(zip, TradeHistoryFileName, res.TradeHistory, res);
                    res.FixTradeReports(zip);
                }
            }
            return res;
        }


        public Result<BacktesterConfig> TryGetConfig()
        {
            var path = _path;
            if (Directory.Exists(path))
                return BacktesterConfig.TryLoad(Path.Combine(path, ConfigFileName));

            if (File.Exists(path))
            {
                using (var file = File.Open(path, FileMode.Open))
                using (var zip = new ZipArchive(file))
                {
                    var entry = zip.GetEntry(ConfigFileName);
                    if (entry != null)
                        return BacktesterConfig.TryLoad(entry.Open());
                }
            }

            return new Result<BacktesterConfig>($"Backtester config not found in '{path}'");
        }

        public string FormatReadErrors()
        {
            if (ReadErrors.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var error in ReadErrors)
            {
                var (code, details, ex) = error;
                var msg = code switch
                {
                    ReadErrorCode.NotFound => $"Can't find '{details}'",
                    ReadErrorCode.ParseError => $"Failed to parse '{details}'",
                    ReadErrorCode.MissingVersion => $"Version info not found",
                    ReadErrorCode.NewerVersion => $"Result are generated using newer version ({details})",
                    ReadErrorCode.UnknownPluginInfo => $"Unsupported plugin info format '{details}'",
                    _ => $"Unknown error: code={code}, details={details}",
                };
                sb.Append(msg);
                if (ex != null)
                {
                    sb.Append(": ");
                    sb.Append(ex.Message);
                }
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }


        private void FixTradeReports(ZipArchive zip)
        {
            if (!ExecStatus.ResultsNotCorrupted)
                return;

            var entry = zip.GetEntry(ConfigFileName);
            if (entry == null)
                return;

            var configRes = BacktesterConfig.TryLoad(entry.Open());
            if (configRes.HasError)
                return;

            var symbolsMap = configRes.ResultValue.TradeServer.Symbols;
            foreach (var report in TradeHistory)
            {
                if (symbolsMap.TryGetValue(report.Symbol, out var symbol))
                {
                    report.MarginCurrency = symbol.MarginCurrency;
                    report.ProfitCurrency = symbol.ProfitCurrency;
                }
            }
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
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<TMap>();
            csv.WriteRecords(data);
        }

        private static void SaveAsCsv<T, TMap>(Stream stream, TMap map, IEnumerable<T> data)
            where TMap : ClassMap<T>
        {
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap(map);
            csv.WriteRecords(data);
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
                    case CsvMapping.ForMarkerPoint.Header: parser = CsvMapping.ForMarkerPoint.Read; break;
                    case CsvMapping.ForMarkerPoint2.Header: parser = CsvMapping.ForMarkerPoint2.Read; break;
                    // For compatibility unknown headers should be treated as simple points
                    default: parser = CsvMapping.ForDoublePoint.Read; break;
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

            public static T TryReadJson<T>(ZipArchive zip, string entryName, BacktesterResults res)
            {
                if (!TryGetZipEntry(zip, entryName, res, out var entry))
                    return default;

                return TryParse(entryName, res, () =>
                {
                    using (var stream = entry.Open())
                        return ReadAsJson<T>(stream, entry.Length);
                });
            }

            public static T TryReadProtoJson<T>(ZipArchive zip, string entryName, JsonParser jsonParser, BacktesterResults res)
                where T : IMessage, new()
            {
                if (!TryGetZipEntry(zip, entryName, res, out var entry))
                    return default;

                return TryParse(entryName, res, () =>
                {
                    using (var stream = entry.Open())
                        return ReadAsProtoJson<T>(stream, jsonParser);
                });
            }

            public static void TryReadCsv<T, TMap>(ZipArchive zip, string entryName, List<T> storage, BacktesterResults res)
                where TMap : ClassMap<T>
            {
                if (!TryGetZipEntry(zip, entryName, res, out var entry))
                    return;

                TryParse(entryName, res, () =>
                {
                    using (var stream = entry.Open())
                        ReadAsCsv<T, TMap>(stream, storage);
                });
            }

            public static void TryReadOutputData(ZipArchive zip, string entryName, List<OutputPoint> storage, BacktesterResults res)
            {
                if (!TryGetZipEntry(zip, entryName, res, out var entry))
                    return;

                TryParse(entryName, res, () =>
                {
                    using (var stream = entry.Open())
                        ReadAsOutputData(stream, storage);
                });
            }


            private static Stream CreateZipEntryStream(ZipArchive zip, string entryName)
            {
                var entry = zip.CreateEntry(entryName);
                return entry.Open();
            }

            private static bool TryGetZipEntry(ZipArchive zip, string entryName, BacktesterResults res, out ZipArchiveEntry entry)
            {
                entry = zip.GetEntry(entryName);
                var hasEntry = entry != null;
                if (res != null && !hasEntry)
                    res.ReadErrors.Add(new ReadError(ReadErrorCode.NotFound, entryName));
                return entry != null;
            }

            private static void TryParse(string entryName, BacktesterResults res, Action readAction)
            {
                try
                {
                    readAction();
                }
                catch (Exception ex)
                {
                    if (res == null)
                        throw;

                    res.ReadErrors.Add(new ReadError(ReadErrorCode.ParseError, entryName, ex));
                }
            }

            private static T TryParse<T>(string entryName, BacktesterResults res, Func<T> readAction)
            {
                try
                {
                    return readAction();
                }
                catch (Exception ex)
                {
                    if (res == null)
                        throw;

                    res.ReadErrors.Add(new ReadError(ReadErrorCode.ParseError, entryName, ex));
                }

                return default;
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

            public static void SaveRoundedCsv<T, TMap>(string path, IEnumerable<T> data, int precision)
                where T : IOutputPoint
                where TMap : RoundClassMap<T>, new()
            {
                var map = new TMap();

                using (var file = File.Open(path, FileMode.Create))
                    SaveAsCsv(file, map.SetPricision(precision), data);
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
                while (cnt++ < 16)
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
                var fileName = Path.GetFileName(dirPath) + ".zip";
                ZipFile.CreateFromDirectory(dirPath, Path.Combine(dirPath, "..", fileName));
                Directory.Delete(dirPath, true);
            }

            public static ExecutionStatus TryReadExecStatus(string resultsDirPath) => AsFile.TryReadJson<ExecutionStatus>(Path.Combine(resultsDirPath, ExecStatusFileName));

            public static void SaveExecStatus(string resultsDirPath, ExecutionStatus status) => AsFile.SaveJson(Path.Combine(resultsDirPath, ExecStatusFileName), status);

            public static void SaveVersionInfo(string resultsDirPath) => AsFile.SaveJson(Path.Combine(resultsDirPath, VersionFileName), new VersionInfo());

            public static void SaveStats(string resultsDirPath, TestingStatistics stats) => AsFile.SaveJson(Path.Combine(resultsDirPath, StatsFileName), stats);

            public static void SavePluginInfo(string resultsDirPath, PluginDescriptor pluginInfo) => AsFile.SaveProtoJson(Path.Combine(resultsDirPath, PluginInfoFileName), pluginInfo, PluginDescriptor.JsonFormatter);

            public static void SaveBarData(string filePath, IEnumerable<BarData> bars) => AsFile.SaveCsv<BarData, ForBarData>(filePath, bars);

            public static void SaveFeedData(string resultsDirPath, string symbolName, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, $"{FeedFilePrefix}{symbolName}.csv"), bars);

            public static void SaveOutputData(string resultsDirPath, string outputName, int precision, IReadOnlyList<OutputPoint> points)
            {
                var filePath = Path.Combine(resultsDirPath, $"{OutputFilePrefix}{outputName}.csv");
                object firstPointMetadata = default;
                if (points.Count > 0)
                    firstPointMetadata = points[0].Metadata;

                switch (firstPointMetadata)
                {
                    case MarkerInfo:
                        AsFile.SaveRoundedCsv<MarkerPointWrapper, ForMarkerPoint2>(filePath, points.Select(p => new MarkerPointWrapper(p)), precision);
                        break;
                    default:
                        AsFile.SaveRoundedCsv<OutputPoint, ForDoublePoint>(filePath, points, precision);
                        break;
                }
            }

            public static void SaveEquity(string resultsDirPath, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, EquityFileName), bars);

            public static void SaveMargin(string resultsDirPath, IEnumerable<BarData> bars) => SaveBarData(Path.Combine(resultsDirPath, MarginFileName), bars);

            public static void SaveTradeHistory(string resultsDirPath, IEnumerable<TradeReportInfo> reports) => AsFile.SaveCsv<TradeReportInfo, ForTradeReport>(Path.Combine(resultsDirPath, "trade-history.csv"), reports);
        }


        private class VersionInfo
        {
            public const int CurrentVersion = 1;

            public int ResultsVersion { get; set; } = CurrentVersion;
            public string PluginInfoUri { get; set; } = PluginDescriptor.JsonUri;
        }


        public enum ReadErrorCode { NotFound, ParseError, MissingVersion, NewerVersion, UnknownPluginInfo }


        public record ReadError(ReadErrorCode Code, string Details, Exception Exception)
        {
            public ReadError(ReadErrorCode code) : this(code, null, null) { }

            public ReadError(ReadErrorCode code, string details) : this(code, details, null) { }
        }
    }
}
