using CsvHelper;
using CsvHelper.Configuration;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    public class BacktesterResults
    {
        public TestingStatistics Stats { get; private set; }

        public Dictionary<string, List<BarData>> Feed { get; } = new Dictionary<string, List<BarData>>();

        public Dictionary<string, List<OutputPoint>> Outputs { get; } = new Dictionary<string, List<OutputPoint>>();

        public List<BarData> Equity { get; } = new List<BarData>();

        public List<BarData> Margin { get; } = new List<BarData>();

        public List<PluginLogRecord> Journal { get; } = new List<PluginLogRecord>();

        public List<TradeReportInfo> TradeHistory { get; } = new List<TradeReportInfo>();


        public static BacktesterResults Load(string filePath)
        {
            var res = new BacktesterResults();
            using (var file = File.Open(filePath, FileMode.Open))
            using (var zip = new ZipArchive(file))
            {
                res.Stats = ReadZipEntryAsJson<TestingStatistics>(zip, "stats.json");
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
                        TryReadZipEntryAsCsv<OutputPoint, CsvMapping.ForOutputPoint>(zip, entryName, data);
                        res.Outputs.Add(outputId, data);
                    }
                }

                TryReadZipEntryAsCsv<PluginLogRecord, CsvMapping.ForLogRecord>(zip, "journal.csv", res.Journal);
                TryReadZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, "equity.csv", res.Equity);
                TryReadZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, "margin.csv", res.Margin);
                TryReadZipEntry(zip, "trade-history.bd64", ParseTradeReport, res.TradeHistory);
            }
            return res;
        }


        internal static void SaveJson<T>(ZipArchive zip, string entryName, T data)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, data);
            }
        }

        internal static void SaveBarData(ZipArchive zip, string entryName, IEnumerable<BarData> bars) => SaveZipEntryAsCsv<BarData, CsvMapping.ForBarData>(zip, entryName, bars);

        internal static void SaveOutputData(ZipArchive zip, string entryName, IEnumerable<OutputPoint> points) => SaveZipEntryAsCsv<OutputPoint, CsvMapping.ForOutputPoint>(zip, entryName, points);

        internal static void SaveTradeHistory(ZipArchive zip, IEnumerable<TradeReportInfo> reports)
        {
            var entry = zip.CreateEntry("trade-history.bd64");
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream))
            {
                writer.Write("Base64 TradeReportInfo protobuf data"); // header is added on purpose

                foreach (var report in reports)
                {
                    writer.WriteLine();
                    writer.Write(report.ToByteString().ToBase64());
                }
            }

            SaveZipEntryAsCsv<TradeReportInfo, CsvMapping.ForTradeReport>(zip, "trade-history.csv", reports);
        }


        private static T ReadZipEntryAsJson<T>(ZipArchive zip, string entryName)
        {
            var entry = zip.GetEntry(entryName);
            using (var stream = entry.Open())
            {
                var data = new byte[entry.Length];
                stream.Read(data, 0, data.Length);
                return JsonSerializer.Deserialize<T>(data);
            }
        }

        private static void TryReadZipEntry<T>(ZipArchive zip, string entryName, Func<string, T> parser, List<T> storage)
        {
            var entry = zip.GetEntry(entryName);
            if (entry == null)
                return;

            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream))
            {
                var header = reader.ReadLine(); // skip header
                var dataStr = reader.ReadLine();
                while (!string.IsNullOrEmpty(dataStr))
                {
                    storage.Add(parser(dataStr));
                    dataStr = reader.ReadLine();
                }
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

        private static TradeReportInfo ParseTradeReport(string dataStr)
        {
            return (TradeReportInfo)TradeReportInfo.Descriptor.Parser.ParseFrom(Convert.FromBase64String(dataStr));
        }
    }
}
