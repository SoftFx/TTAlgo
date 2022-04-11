using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.Algo.BacktesterApi
{
    public enum BacktesterMode { Backtesting, Optimization, Visualization }

    public enum WarmupUnitTypes { Bars, Ticks, Days, Hours }

    [Flags]
    public enum JournalOptions
    {
        Disabled = 0,
        Enabled = 1,
        WriteInfo = 2,
        WriteCustom = 4,
        WriteTrade = 8,
        WriteOrderModifications = 128,
        WriteAlert = 256,

        Default = Enabled | WriteCustom | WriteInfo | WriteTrade | WriteAlert,
    }

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
                return Load(file);
        }

        public static BacktesterConfig Load(Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                return new BacktesterConfig
                {
                    _version = ReadZipEntryAsJson<VersionInfo>(zip, "version.json"),
                    Core = ReadZipEntryAsJson<CoreConfig>(zip, "core.json"),
                    Account = ReadZipEntryAsJson<AccountConfig>(zip, "account.json"),
                    TradeServer = ReadZipEntryAsJson<TradeServerConfig>(zip, "trade-server.json"),
                    Env = ReadZipEntryAsJson<EnvInfo>(zip, "env.json"),
                    PluginConfig = ReadZipEntry(zip, "plugin-cfg.apr", Algo.Core.Config.PluginConfig.LoadFromStream).ToDomain(),
                };
            }
        }


        public void Save(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Create))
                Save(file);
        }

        public void Save(Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Update))
            {
                WriteZipEntryAsJson(zip, "version.json", _version);
                WriteZipEntryAsJson(zip, "core.json", Core);
                WriteZipEntryAsJson(zip, "account.json", Account);
                WriteZipEntryAsJson(zip, "trade-server.json", TradeServer);
                WriteZipEntryAsJson(zip, "env.json", Env);
                WriteZipEntry(zip, "plugin-cfg.apr", Algo.Core.Config.PluginConfig.FromDomain(PluginConfig), Algo.Core.Config.PluginConfig.SaveToStream);
            }
        }

        public void SetPluginConfig(PluginConfig config)
        {
            Core.ConfigUri = PluginConfig.JsonUri;
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


        private static T ReadZipEntry<T>(ZipArchive zip, string entryName, Func<Stream, T> readerFunc)
        {
            var entry = zip.GetEntry(entryName);
            using (var stream = entry.Open())
            {
                return readerFunc(stream);
            }
        }

        private static T ReadDataAsJson<T>(Stream stream)
        {
            using(var reader  = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json);
            }
        }

        private static T ReadZipEntryAsJson<T>(ZipArchive zip, string entryName) => ReadZipEntry(zip, entryName, ReadDataAsJson<T>);


        private static void WriteZipEntry<T>(ZipArchive zip, string entryName, T data, Action<Stream, T> writerFunc)
        {
            var entry = zip.CreateEntry(entryName);
            using (var stream = entry.Open())
            {
                writerFunc(stream, data);
            }
        }

        private static void WriteDataAsJson<T>(Stream stream, T data)
        {
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonSerializer.Serialize(writer, data);
            }
        }

        private static void WriteZipEntryAsJson<T>(ZipArchive zip, string entryName, T data) => WriteZipEntry(zip, entryName, data, WriteDataAsJson);


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
            public SortedSet<string> FeedConfig { get; set; } = new SortedSet<string>();
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
            public Dictionary<string, CustomSymbolInfo> Symbols { get; set; } = new Dictionary<string, CustomSymbolInfo>();
            public Dictionary<string, CustomCurrency> Currencies { get; set; } = new Dictionary<string, CustomCurrency>();
        }

        public class EnvInfo
        {
            public string PackagePath { get; set; }
            public string FeedCachePath { get; set; }
            public string CustomFeedCachePath { get; set; }
            public string WorkingFolderPath { get; set; }
        }
    }
}
