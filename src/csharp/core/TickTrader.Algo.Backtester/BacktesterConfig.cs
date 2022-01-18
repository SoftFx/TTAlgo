using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;

namespace TickTrader.Algo.Backtester
{
    public class BacktesterConfig
    {
        public BacktesterCoreConfig Core { get; private set; } = new BacktesterCoreConfig();
        public BacktesterAccountConfig Account { get; private set; } = new BacktesterAccountConfig();
        public BacktesterTradeServerConfig TradeServer { get; private set; } = new BacktesterTradeServerConfig();

        public PluginConfig PluginConfig { get; private set; }


        public static BacktesterConfig Load(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Create))
            using (var zip = new ZipArchive(file))
            {
                return new BacktesterConfig
                {
                    Core = ReadZipEntryAsJson<BacktesterCoreConfig>(zip, "core.json"),
                    Account = ReadZipEntryAsJson<BacktesterAccountConfig>(zip, "account.json"),
                    TradeServer = ReadZipEntryAsJson<BacktesterTradeServerConfig>(zip, "trade-server.json"),
                    PluginConfig = ReadZipEntryAsProtoMsg(zip, "plugin.cfg", PluginConfig.Parser),
                };
            }
        }


        public void Save(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Create))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Update))
            {
                WriteZipEntryAsJson(zip, "core.json", Core);
                WriteZipEntryAsJson(zip, "account.json", Account);
                WriteZipEntryAsJson(zip, "trade-server.json", TradeServer);
                WriteZipEntryAsProtoMsg(zip, "plugin.cfg", PluginConfig);
            }
        }

        public void SetPluginConfig(PluginConfig config)
        {
            Core.ConfigUri = PluginConfig.Descriptor.FullName;
            PluginConfig = config;
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

        private static T ReadZipEntryAsProtoMsg<T>(ZipArchive zip, string entryName, MessageParser<T> parser)
            where T : IMessage<T>
        {
            var entry = zip.GetEntry(entryName);
            using (var stream = entry.Open())
            {
                return parser.ParseFrom(stream);
            }
        }


        private void WriteZipEntryAsJson<T>(ZipArchive zip, string entryName, T value)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, value);
            }
        }

        private void WriteZipEntryAsProtoMsg(ZipArchive zip, string entryName, IMessage data)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            {
                data.WriteTo(stream);
            }
        }
    }

    public enum BacktesterMode { Backtesting, Optimization, Visualization }

    public class BacktesterCoreConfig
    {
        public BacktesterMode Mode { get; set; }
        public DateTime? EmulateFrom { get; set; }
        public DateTime? EmulateTo { get; set; }
        public Dictionary<string, Feed.Types.Timeframe> FeedConfig { get; set; } = new Dictionary<string, Feed.Types.Timeframe>();
        public string ConfigUri { get; set; }

        public string MainSymbol { get; set; }
        public Feed.Types.Timeframe MainTimeframe { get; set; }
        public Feed.Types.Timeframe ModelTimeframe { get; set; }

        // advanced
        public int WarmupValue { get; set; } = 10;
        public WarmupUnitTypes WarmupUnits { get; set; } = WarmupUnitTypes.Bars;
        public uint ServerPingMs { get; set; } = 200;
        public JournalOptions JournalFlags { get; set; }
    }

    public class BacktesterAccountConfig
    {
        public AccountInfo.Types.Type Type { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; } = 100;
        public double InitialBalance { get; set; } = 10000;
        public Dictionary<string, AssetInfo> InitialAssets { get; } = new Dictionary<string, AssetInfo>();
    }

    public class BacktesterTradeServerConfig
    {
        public Dictionary<string, CustomSymbol> Symbols { get; } = new Dictionary<string, CustomSymbol>();
        public Dictionary<string, CustomCurrency> Currencies { get; } = new Dictionary<string, CustomCurrency>();
    }
}
