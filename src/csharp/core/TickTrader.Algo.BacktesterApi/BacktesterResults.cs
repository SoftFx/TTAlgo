using CsvHelper;
using CsvHelper.Configuration;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.BacktesterApi
{
    public class BacktesterResults
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals };


        public TestingStatistics Stats { get; private set; }

        public Dictionary<string, List<BarData>> Feed { get; } = new Dictionary<string, List<BarData>>();

        public Dictionary<string, List<OutputPoint>> Outputs { get; } = new Dictionary<string, List<OutputPoint>>();

        public List<BarData> Equity { get; } = new List<BarData>();

        public List<BarData> Margin { get; } = new List<BarData>();

        public List<PluginLogRecord> Journal { get; } = new List<PluginLogRecord>();

        public List<TradeReportInfo> TradeHistory { get; } = new List<TradeReportInfo>();

        public PluginDescriptor PluginDescriptor { get; private set; }


        public static BacktesterResults Load(string filePath)
        {
            var res = new BacktesterResults();
            using (var file = File.Open(filePath, FileMode.Open))
            using (var zip = new ZipArchive(file))
            {
                res.Stats = ReadZipEntryAsJson<TestingStatistics>(zip, "stats.json");
                res.PluginDescriptor = TryReadZipEntryAsProtoJson<PluginDescriptor>(zip, "plugin-info.json", PluginDescriptor.JsonParser);
                foreach (var entry in zip.Entries)
                {
                    var entryName = entry.Name;
                    if (entryName.StartsWith("feed"))
                    {
                        var symbol = Path.GetFileNameWithoutExtension(entryName).Substring(5);
                        var data = new List<BarData>();
                        TryReadZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, entryName, data);
                        res.Feed.Add(symbol, data);
                    }
                    else if (entryName.StartsWith("output"))
                    {
                        var outputId = Path.GetFileNameWithoutExtension(entryName).Substring(7);
                        var data = new List<OutputPoint>();
                        TryReadZipEntryAsOutputData(zip, entryName, data);
                        res.Outputs.Add(outputId, data);
                    }
                }

                TryReadZipEntryAsCsv<PluginLogRecord, CsvMapping.ForLogRecord>(zip, "journal.csv", res.Journal);
                TryReadZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, "equity.csv", res.Equity);
                TryReadZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, "margin.csv", res.Margin);
                TryReadZipEntryAsCsv<TradeReportInfo, CsvMapping.ForTradeReport>(zip, "trade-history.csv", res.TradeHistory);
            }
            return res;
        }


        public static void SaveJson<T>(ZipArchive zip, string entryName, T data)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, data, _jsonOptions);
            }
        }

        public static void SaveProtoJson(ZipArchive zip, string entryName, IMessage data, JsonFormatter jsonFormatter)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(jsonFormatter.Format(data));
            }
        }

        public static void SaveBarData(ZipArchive zip, string entryName, IEnumerable<BarData> bars) => SaveZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, entryName, bars);

        public static void SaveOutputData(ZipArchive zip, string entryName, IReadOnlyList<OutputPoint> points)
        {
            var first = points[0];
            if (first.Metadata is MarkerInfo)
            {
                SaveZipEntryAsCsv<CsvMapping.MarkerPointWrapper, CsvMapping.ForMarkerPoint>(zip, entryName, points.Select(p => new CsvMapping.MarkerPointWrapper(p)));
            }
            else
            {
                SaveZipEntryAsCsv<OutputPoint, CsvMapping.ForDoublePoint>(zip, entryName, points);
            }
        }

        public static void SaveTradeHistory(ZipArchive zip, IEnumerable<TradeReportInfo> reports) => SaveZipEntryAsCsv<TradeReportInfo, CsvMapping.ForTradeReport>(zip, "trade-history.csv", reports);


        private static T ReadZipEntryAsJson<T>(ZipArchive zip, string entryName)
        {
            var entry = zip.GetEntry(entryName);
            using (var stream = entry.Open())
            {
                var data = new byte[entry.Length];
                stream.Read(data, 0, data.Length);
                return JsonSerializer.Deserialize<T>(data, _jsonOptions);
            }
        }

        private static T TryReadZipEntryAsProtoJson<T>(ZipArchive zip, string entryName, JsonParser jsonParser)
            where T : IMessage, new()
        {
            var entry = zip.GetEntry(entryName);
            if (entry == null)
                return default;

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                return jsonParser.Parse<T>(text);
            }
        }

        private static void TryReadZipEntryAsCsv<T, TMap>(ZipArchive zip, string entryName, List<T> storage)
            where TMap : ClassMap<T>
        {
            var entry = zip.GetEntry(entryName);
            if (entry == null)
                return;

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                storage.AddRange(csv.GetRecords<T>());
            }
        }

        private static void TryReadZipEntryAsOutputData(ZipArchive zip, string entryName, List<OutputPoint> storage)
        {
            var entry = zip.GetEntry(entryName);
            if (entry == null)
                return;

            using (var stream = entry.Open())
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

        private static void SaveZipEntryAsCsv<T, TMap>(ZipArchive zip, string entryName, IEnumerable<T> data)
            where TMap : ClassMap<T>
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<TMap>();
                csv.WriteRecords(data);
            }
        }
    }
}
