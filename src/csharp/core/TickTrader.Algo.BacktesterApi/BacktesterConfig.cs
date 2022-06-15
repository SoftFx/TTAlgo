using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage;

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


        public CoreConfig Core { get; private set; } = new CoreConfig();
        public AccountConfig Account { get; private set; } = new AccountConfig();
        public TradeServerConfig TradeServer { get; private set; } = new TradeServerConfig();
        public EnvInfo Env { get; private set; } = new EnvInfo();

        public PluginConfig PluginConfig { get; private set; }


        public static Result<BacktesterConfig> TryLoad(string filePath)
        {
            using (var file = File.Open(filePath, FileMode.Open))
                return TryLoad(file);
        }

        public static Result<BacktesterConfig> TryLoad(Stream stream)
        {
            bool TryReadEntry<T>(ZipArchive zip, string entryName, out Result<T> result, out Result<BacktesterConfig> errorAnswer)
            {
                result = TryReadZipEntryAsJson<T>(zip, entryName);
                errorAnswer = result ? null : new Result<BacktesterConfig>(result.ErrorMessage);

                return !result.HasError;
            }

            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
            {
                if (!TryReadEntry<VersionInfo>(zip, "version.json", out var version, out var error))
                    return error;

                if (!TryReadEntry<CoreConfig>(zip, "core.json", out var core, out error))
                    return error;

                if (!TryReadEntry<AccountConfig>(zip, "account.json", out var account, out error))
                    return error;

                if (!TryReadEntry<TradeServerConfig>(zip, "trade-server.json", out var server, out error))
                    return error;

                if (!TryReadEntry<EnvInfo>(zip, "env.json", out var env, out error))
                    return error;

                var res = new BacktesterConfig
                {
                    _version = version.ResultValue,
                    Core = core.ResultValue,
                    Account = account.ResultValue,
                    TradeServer = server.ResultValue,
                    Env = env.ResultValue,
                };

                var pluginRequest = TryReadZipEntry(zip, res._version.PluginConfigSubPath, Algo.Core.Config.PluginConfig.LoadFromStream);

                if (!pluginRequest)
                    return new Result<BacktesterConfig>(pluginRequest.ErrorMessage);

                res.PluginConfig = pluginRequest.ResultValue.ToDomain();

                return Result<BacktesterConfig>.Ok(res);
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
                WriteZipEntry(zip, _version.PluginConfigSubPath, Algo.Core.Config.PluginConfig.FromDomain(PluginConfig), Algo.Core.Config.PluginConfig.SaveToStream);
            }
        }

        public void SetPluginConfig(PluginConfig config)
        {
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


        private static Result<T> TryReadZipEntry<T>(ZipArchive zip, string entryName, Func<Stream, T> readerFunc)
        {
            var entry = zip.GetEntry(entryName);

            if (entry == null)
                return new Result<T>($"Folder {entryName} cannot be found");

            using (var stream = entry.Open())
            {
                return Result<T>.Ok(readerFunc(stream));
            }
        }

        private static T ReadDataAsJson<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<T>(json);
            }
        }

        private static Result<T> TryReadZipEntryAsJson<T>(ZipArchive zip, string entryName) => TryReadZipEntry(zip, entryName, ReadDataAsJson<T>);


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
            public string PluginConfigUri { get; set; } = Algo.Core.Config.PluginConfig.XmlUri;
            public string PluginConfigSubPath { get; set; } = "plugin-cfg.apr";
        }

        public class CoreConfig
        {
            public BacktesterMode Mode { get; set; }
            public DateTime EmulateFrom { get; set; }
            public DateTime EmulateTo { get; set; }
            public Dictionary<string, string> FeedConfig { get; set; } = new Dictionary<string, string>();

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
