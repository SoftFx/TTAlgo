using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.Algo.Backtester
{
    public enum BacktesterMode { Backtesting, Optimization, Visualization }

    public class BacktesterConfig
    {
        private VersionInfo _version = new VersionInfo();


        public int ConfigVersion => _version.ConfigVersion;
        public int PackageVersion => _version.PackageVersion;

        public CoreConfig Core { get; private set; } = new CoreConfig();
        public AccountConfig Account { get; private set; } = new AccountConfig();
        public TradeServerConfig TradeServer { get; private set; } = new TradeServerConfig();
        public EnvInfo Env { get; private set; } = new EnvInfo();

        public PluginConfig PluginConfig { get; private set; }


        public static BacktesterConfig Load(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Open))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                return new BacktesterConfig
                {
                    _version = ReadZipEntryAsJson<VersionInfo>(zip, "version.json"),
                    Core = ReadZipEntryAsJson<CoreConfig>(zip, "core.json"),
                    Account = ReadZipEntryAsJson<AccountConfig>(zip, "account.json"),
                    TradeServer = ReadZipEntryAsJson<TradeServerConfig>(zip, "trade-server.json"),
                    Env = ReadZipEntryAsJson<EnvInfo>(zip, "env.json"),
                    PluginConfig = ReadZipEntryAsProtoMsg(zip, "plugin.cfg", PluginConfig.Parser),
                };
            }
        }


        public void Save(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.CreateNew))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Update))
            {
                WriteZipEntryAsJson(zip, "version.json", _version);
                WriteZipEntryAsJson(zip, "core.json", Core);
                WriteZipEntryAsJson(zip, "account.json", Account);
                WriteZipEntryAsJson(zip, "trade-server.json", TradeServer);
                WriteZipEntryAsJson(zip, "env.json", Env);
                WriteZipEntryAsProtoMsg(zip, "plugin.cfg", PluginConfig);
            }
        }

        public void SetPluginConfig(PluginConfig config)
        {
            Core.ConfigUri = PluginConfig.Descriptor.FullName;
            PluginConfig = config;
        }

        public void Validate()
        {
            var core = Core;
            if (core.Mode != BacktesterMode.Backtesting)
                throw new AlgoException("Mode not supported yet!");
            if (core.EmulateFrom == core.EmulateTo)
                throw new AlgoException("Zero range!");
            if (core.ServerPingMs < 0)
                throw new ArgumentException("Invalid ping value");
            if (Account.Type == AccountInfo.Types.Type.Cash)
                throw new ArgumentException("Cash accounts are not supported");
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


        private static void WriteZipEntryAsJson<T>(ZipArchive zip, string entryName, T value)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, value);
            }
        }

        private static void WriteZipEntryAsProtoMsg(ZipArchive zip, string entryName, IMessage data)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            {
                data.WriteTo(stream);
            }
        }


        private class VersionInfo
        {
            public int ConfigVersion { get; set; } = 1;
            public int PackageVersion { get; set; } = 1;
        }

        public class CoreConfig
        {
            public BacktesterMode Mode { get; set; }
            public DateTime EmulateFrom { get; set; }
            public DateTime EmulateTo { get; set; }
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

        public class AccountConfig
        {
            public AccountInfo.Types.Type Type { get; set; }
            public string BalanceCurrency { get; set; }
            public int Leverage { get; set; } = 100;
            public double InitialBalance { get; set; } = 10000;
            public Dictionary<string, AssetInfo> InitialAssets { get; set; } = new Dictionary<string, AssetInfo>();
        }

        public class TradeServerConfig
        {
            public Dictionary<string, ISymbolData> Symbols { get; set; } = new Dictionary<string, ISymbolData>();
            public Dictionary<string, CustomCurrency> Currencies { get; set; } = new Dictionary<string, CustomCurrency>();
        }

        public class EnvInfo
        {
            public string PackagePath { get; set; }
            public string FeedCachePath { get; set; }
            public string ResultsPath { get; set; }
            public string WorkingFolderPath { get; set; }
        }
    }
}
