using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;
using TickTrader.Algo.Package;
using TickTrader.FeedStorage;

namespace TickTrader.Algo.BacktesterV1Host
{
    internal class BacktesterV1Actor : Actor
    {
        private readonly string _id;
        private BacktesterV1HostHandler _handler;

        private IAlgoLogger _logger;
        private CancellationTokenSource _cancelTokenSrc;


        private BacktesterV1Actor(string id, BacktesterV1HostHandler handler)
        {
            _id = id;
            _handler = handler;

            Receive<StartBacktesterRequest>(Start);
            Receive<StopBacktesterRequest>(Stop);
        }


        public static IActorRef Create(string id, BacktesterV1HostHandler handler)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterV1Actor(id, handler), $"{nameof(BacktesterV1Actor)} ({id})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private void Start(StartBacktesterRequest request)
        {
            if (_cancelTokenSrc != null)
                throw new AlgoException("Backtester already started");

            _logger.Info("Starting...");

            // load default reduction to metadata cache
            PackageExplorer.ScanAssembly(MappingDefaults.DefaultExtPackageId, typeof(BarCloseReduction).Assembly);

            var config = BacktesterConfig.Load(request.ConfigPath);
            config.Validate();

            var pkgId = config.PluginConfig.Key.PackageId;

            _logger.Debug($"Mode = {config.Core.Mode}");
            _logger.Debug($"PkgId = {pkgId}");
            _logger.Debug($"PluginId = {config.PluginConfig.Key.DescriptorId}");

            var pkgPath = config.Env.PackagePath;
            _logger.Debug($"Package path: {pkgPath}");
            PackageLoadContext.Load(pkgId, config.Env.PackagePath);
            _logger.Info("Loaded package");

            _logger.Info("Started");

            _cancelTokenSrc = new CancellationTokenSource();
            var _ = SetupAndRunBacktester(config);
        }

        private void Stop(StopBacktesterRequest request)
        {
            _logger.Debug("Manual stop request");

            if (_cancelTokenSrc != null)
            {
                _cancelTokenSrc?.Cancel();
                _cancelTokenSrc = null;
            }
        }


        private async Task SetupAndRunBacktester(BacktesterConfig config)
        {
            try
            {
                var from = DateTime.SpecifyKind(config.Core.EmulateFrom, DateTimeKind.Utc);
                var to = DateTime.SpecifyKind(config.Core.EmulateTo, DateTimeKind.Utc);

                //await RunInternal(config); // stub
                await DoBacktesting(config, from, to);

                _handler.SendStoppedMsg(null);
            }
            catch (TaskCanceledException)
            {
                _logger.Info("Run cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Run failed");
                _handler.SendStoppedMsg(ex.Message);
            }
        }

        private async Task RunInternal(BacktesterConfig config)
        {
            const ulong total = 10;
            for (ulong i = 0; i < total; i++)
            {
                await Task.Delay(1000, _cancelTokenSrc.Token);
                _handler.SendProgress(i, total);
            }

            _handler.SendStoppedMsg(null);

            _logger.Debug("Finished");
        }

        private async Task DoBacktesting(BacktesterConfig config, DateTime from, DateTime to)
        {
            using (var backtester = new Backtester.Backtester(config.PluginConfig.Key, null, from, to))
            {
                OnStartEmulation(backtester);

                try
                {
                    //backtester.Executor.LogUpdated += JournalPage.Append;
                    //backtester.Executor.TradeHistoryUpdated += Executor_TradeHistoryUpdated;

                    ConfigureCommonSettings(config, backtester.CommonSettings);
                    ConfigureFeed(config, backtester.Feed, from, to);
                    backtester.JournalFlags = config.Core.JournalFlags;

                    PluginConfigLoader.ApplyConfig(backtester, config.PluginConfig, config.Core.MainSymbol, config.Env.WorkingFolderPath);

                    var _ = SendProgressLoop();
                    await Task.Delay(10000);
                    _logger.Debug($"Main symbol: {backtester.CommonSettings.MainSymbol}");
                    await backtester.Run(_cancelTokenSrc.Token);
                }
                finally
                {
                    OnStopEmulation(backtester);
                }
            }
        }

        private void ConfigureCommonSettings(BacktesterConfig config, CommonTestSettings settings)
        {
            var core = config.Core;
            var acc = config.Account;

            settings.MainSymbol = core.MainSymbol;
            settings.MainTimeframe = core.MainTimeframe;
            settings.ModelTimeframe = core.ModelTimeframe;

            settings.ServerPing = TimeSpan.FromMilliseconds(core.ServerPingMs);
            settings.WarmupSize = core.WarmupValue;
            settings.WarmupUnits = core.WarmupUnits;

            settings.AccountType = acc.Type;
            settings.Leverage = acc.Leverage;
            settings.InitialBalance = acc.InitialBalance;
            settings.BalanceCurrency = acc.BalanceCurrency;

            var traderServer = config.TradeServer;
            foreach (var c in traderServer.Currencies)
            {
                settings.Currencies.Add(c.Key, c.Value.ToAlgo());
            }
            foreach (var s in traderServer.Symbols)
            {
                settings.Symbols.Add(s.Key, s.Value.ToAlgo());
            }
        }

        private void ConfigureFeed(BacktesterConfig config, FeedEmulator feedEmulator, DateTime from, DateTime to)
        {
            var core = config.Core;
            var feedCachePath = config.Env.FeedCachePath;

            foreach (var feedCfg in core.FeedConfig)
            {
                var symbol = feedCfg.Key;
                var timeframe = symbol == core.MainSymbol ? core.ModelTimeframe : feedCfg.Value;

                if (timeframe == Feed.Types.Timeframe.Ticks || timeframe == Feed.Types.Timeframe.TicksLevel2)
                {
                    var request = new CrossDomainReaderRequest(new FeedCacheKey(symbol, timeframe), from, to);
                    feedEmulator.AddSource(symbol, new TickCrossDomainReader(feedCachePath, request));
                }
                else
                {
                    var bidRequest = new CrossDomainReaderRequest(new FeedCacheKey(symbol, timeframe, Feed.Types.MarketSide.Bid), from, to);
                    var askRequest = new CrossDomainReaderRequest(new FeedCacheKey(symbol, timeframe, Feed.Types.MarketSide.Ask), from, to);
                    feedEmulator.AddSource(symbol, timeframe, new BarCrossDomainReader(feedCachePath, bidRequest), new BarCrossDomainReader(feedCachePath, askRequest));
                }
            }

            feedEmulator.AddBarBuilder(core.MainSymbol, core.MainTimeframe, Feed.Types.MarketSide.Bid);
        }

        private async Task SendProgressLoop()
        {
            try
            {
                var token = _cancelTokenSrc.Token;
                while (!token.IsCancellationRequested)
                {
                    //_handler.SendProgress(backtester.CurrentTimePoint?.GetAbsoluteDay() ?? progressMin);
                    await Task.Delay(1000, token);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.Error(ex, "Send progress loop failed");
            }
        }

        private void OnStartEmulation(ITestExecController tester)
        {
            tester.StateChanged += OnStateChanged;
            tester.ErrorOccurred += OnErrorOccurred;
        }

        private void OnStopEmulation(ITestExecController tester)
        {
            tester.StateChanged -= OnStateChanged;
            tester.ErrorOccurred -= OnErrorOccurred;
        }

        private void OnStateChanged(EmulatorStates state)
        {
            _logger.Info($"Emulator state: {state}");
        }

        private void OnErrorOccurred(Exception ex)
        {
            _logger.Error(ex, "Emulator error");
        }
    }
}
