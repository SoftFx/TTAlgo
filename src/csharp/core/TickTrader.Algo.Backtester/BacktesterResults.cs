using Google.Protobuf.WellKnownTypes;
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
                        TryReadZipEntryAsCsv(zip, entryName, ParseBarData, data);
                        res.Feed.Add(symbol, data);
                    }
                    else if (entryName.StartsWith("output"))
                    {
                        var outputId = Path.GetFileNameWithoutExtension(entryName).Substring(7);
                        var data = new List<OutputPoint>();
                        TryReadZipEntryAsCsv(zip, entryName, ParseOutputPoint, data);
                        res.Outputs.Add(outputId, data);
                    }
                }

                TryReadZipEntryAsCsv(zip, "journal.csv", ParseLogRecord, res.Journal);
                TryReadZipEntryAsCsv(zip, "equity.csv", ParseBarData, res.Equity);
                TryReadZipEntryAsCsv(zip, "margin.csv", ParseBarData, res.Margin);
                TryReadZipEntryAsCsv(zip, "trade-history.bd64", ParseTradeReport, res.TradeHistory);
            }
            return res;
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

        private static void TryReadZipEntryAsCsv<T>(ZipArchive zip, string entryName, Func<string, T> parser, List<T> storage)
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

        private static PluginLogRecord ParseLogRecord(string dataStr)
        {
            var parts = dataStr.Split(',');
            return new PluginLogRecord
            {
                TimeUtc = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp(),
                Severity = (PluginLogRecord.Types.LogSeverity)System.Enum.Parse(typeof(PluginLogRecord.Types.LogSeverity), parts[1]),
                Message = parts[2],
                Details = parts[3],
            };
        }

        private static BarData ParseBarData(string dataStr)
        {
            var parts = dataStr.Split(',');
            return new BarData
            {
                OpenTime = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp(),
                Open = double.Parse(parts[1]),
                High = double.Parse(parts[2]),
                Low = double.Parse(parts[3]),
                Close = double.Parse(parts[4])
            };
        }

        private static OutputPoint ParseOutputPoint(string dataStr)
        {
            var parts = dataStr.Split(',');
            return new OutputPoint
            {
                Time = DateTime.Parse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime().ToTimestamp(),
                Index = int.Parse(parts[1]),
                Value = (Any)Any.Descriptor.Parser.ParseFrom(Convert.FromBase64String(parts[2]))
            };
        }

        private static TradeReportInfo ParseTradeReport(string dataStr)
        {
            return (TradeReportInfo)TradeReportInfo.Descriptor.Parser.ParseFrom(Convert.FromBase64String(dataStr));
        }
    }
}
