﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Ext;
using TickTrader.Algo.Package;
using TickTrader.FeedStorage;

namespace TickTrader.Algo.BacktesterV1Host
{
    internal interface IBacktesterV1Callback
    {
        void SendStoppedMsg(string message);

        void SendProgress(double current, double total);

        void SendStateUpdate(EmulatorStates state);
    }

    internal class BacktesterV1Actor : Actor
    {
        private const double MaxProgessValue = 100;

        private readonly string _resultsDir;
        private IBacktesterV1Callback _callback;

        private IAlgoLogger _logger;
        private CancellationTokenSource _cancelTokenSrc;


        private BacktesterV1Actor(string resultsDir, IBacktesterV1Callback callback)
        {
            _resultsDir = resultsDir;
            _callback = callback;

            Receive<StartBacktesterRequest>(Start);
            Receive<StopBacktesterRequest>(Stop);
        }


        public static IActorRef Create(string id, string resultsDir, IBacktesterV1Callback callback)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterV1Actor(resultsDir, callback), $"{nameof(BacktesterV1Actor)} ({id})");
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

            var result = BacktesterConfig.TryLoad(Path.Combine(_resultsDir, BacktesterResults.ConfigFileName));

            if (!result)
                throw result.Exception;

            var config = result.ResultValue;

            config.Validate();

            var pkgId = config.PluginConfig.Key.PackageId;

            _logger.Debug($"Mode = {config.Core.Mode}");
            _logger.Debug($"PkgId = {pkgId}");
            _logger.Debug($"PluginId = {config.PluginConfig.Key.DescriptorId}");

            var pkgPath = config.Env.PackagePath;
            _logger.Debug($"Package path: {pkgPath}");
            PackageLoadContext.Load(pkgId, config.Env.PackagePath);
            _logger.Info("Loaded package");

            var workDir = config.Env.WorkingFolderPath;
            _logger.Debug($"Set working dir as {workDir}");
            System.IO.Directory.SetCurrentDirectory(workDir);

            _logger.Info("Started");

            _cancelTokenSrc = new CancellationTokenSource();
            var _ = SetupAndRunBacktester(config);
        }

        private void Stop(StopBacktesterRequest request)
        {
            _logger.Debug("Manual stop request");

            if (_cancelTokenSrc != null)
            {
                _cancelTokenSrc.Cancel();
                _cancelTokenSrc = null;
            }
        }


        private async Task SetupAndRunBacktester(BacktesterConfig config)
        {
            try
            {
                var from = DateTime.SpecifyKind(config.Core.EmulateFrom, DateTimeKind.Utc);
                var to = DateTime.SpecifyKind(config.Core.EmulateTo, DateTimeKind.Utc);

                await DoBacktesting(config, from, to);

                _callback.SendStoppedMsg(null);
            }
            catch (TaskCanceledException)
            {
                _logger.Info("Run cancelled");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Run failed");
                _callback.SendStoppedMsg(ex.Message);
            }
        }

        private async Task DoBacktesting(BacktesterConfig config, DateTime from, DateTime to)
        {
            using (var backtester = new Backtester.Backtester(config.PluginConfig.Key, from, to))
            {
                var execStatus = new ExecutionStatus();
                OnStartEmulation(backtester);

                try
                {
#if DEBUG
                    //const int timeOut = 20;
                    //for (ulong i = 0; i < timeOut; i++)
                    //{
                    //    await Task.Delay(500, _cancelTokenSrc.Token);
                    //    _callback.SendProgress(i, timeOut);
                    //}
#endif

                    ConfigureCommonSettings(config, backtester.CommonSettings);
                    ConfigureFeed(config, backtester.Feed, from, to);
                    backtester.JournalFlags = config.Core.JournalFlags;
                    backtester.JournalPath = Path.Combine(_resultsDir, BacktesterResults.JournalFileName);
                    backtester.SymbolDataConfig[config.Core.MainSymbol] = TestDataSeriesFlags.Snapshot;
                    backtester.OutputDataMode = TestDataSeriesFlags.Snapshot;

                    PluginConfigLoader.ApplyConfig(backtester, config.PluginConfig, config.Core.MainSymbol, config.Env.WorkingFolderPath);

                    var _ = SendProgressLoop(backtester, from, to);
                    await backtester.Run(_cancelTokenSrc.Token);

                    execStatus.AddStatus("Emulation completed");
                }
                catch (OperationCanceledException)
                {
                    _logger.Info("Emulation cancelled");
                    execStatus.AddStatus("Emulation cancelled");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Fatal emulation error");
                    execStatus.SetError("Fatal emulation error", ex.Message);
                }
                finally
                {
                    OnStopEmulation(backtester);
                }

                _callback.SendProgress(MaxProgessValue, MaxProgessValue);

                _logger.Debug("Saving results...");

                try
                {
                    backtester.SaveResults(_resultsDir);
                    execStatus.ResultsNotCorrupted = true;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to save results");
                    execStatus.SetError("Failed to save results", ex.Message);
                }

                try
                {
                    BacktesterResults.Internal.SaveExecStatus(_resultsDir, execStatus);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to save exec status");
                }

                if (_cancelTokenSrc != null)
                {
                    _cancelTokenSrc.Cancel();
                    _cancelTokenSrc = null;
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
                settings.Symbols.Add(s.Key, new SymbolInfo(s.Value));
            }
        }

        private static void ConfigureFeed(BacktesterConfig config, FeedEmulator feedEmulator, DateTime from, DateTime to)
        {
            var core = config.Core;


            foreach (var feedCfg in core.FeedConfig)
                if (FeedCacheKey.TryParse(feedCfg.Value, out var feedKey))
                {
                    var symbol = feedCfg.Key;
                    var timeframe = feedKey.TimeFrame;
                    var feedCachePath = feedKey.Origin == SymbolConfig.Types.SymbolOrigin.Online ? config.Env.FeedCachePath : config.Env.CustomFeedCachePath;

                    if (timeframe.IsTick())
                    {
                        var request = new CrossDomainReaderRequest(new FeedCacheKey(feedKey), from, to);
                        feedEmulator.AddSource(symbol, new TickCrossDomainReader(feedCachePath, request));
                    }
                    else
                    {
                        var bidRequest = new CrossDomainReaderRequest(new FeedCacheKey(feedKey, Feed.Types.MarketSide.Bid), from, to);
                        var askRequest = new CrossDomainReaderRequest(new FeedCacheKey(feedKey, Feed.Types.MarketSide.Ask), from, to);
                        feedEmulator.AddSource(symbol, timeframe, new BarCrossDomainReader(feedCachePath, bidRequest), new BarCrossDomainReader(feedCachePath, askRequest));
                    }
                }

            feedEmulator.AddBarBuilder(core.MainSymbol, core.MainTimeframe, Feed.Types.MarketSide.Bid);
        }

        private async Task SendProgressLoop(Backtester.Backtester backtester, DateTime from, DateTime to)
        {
            try
            {
                var offset = from.GetAbsoluteDay();
                var total = to.GetAbsoluteDay() - offset;
                var token = _cancelTokenSrc.Token;
                while (!token.IsCancellationRequested)
                {
                    var current = backtester.CurrentTimePoint?.GetAbsoluteDay();
                    var diff = (current == null || current < offset) ? 0 : current - offset;
                    _callback.SendProgress(MaxProgessValue * diff / total ?? 0, MaxProgessValue);
                    await Task.Delay(500, token);
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
            _callback.SendStateUpdate(state);
        }

        private void OnErrorOccurred(Exception ex)
        {
            _logger.Error(ex, "Emulator error");
        }
    }
}
