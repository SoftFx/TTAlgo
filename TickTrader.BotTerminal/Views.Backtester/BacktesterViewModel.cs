using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using NLog;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.FeedStorage.Api;


namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Conductor<Page>.Collection.OneActive, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ISymbolCatalog _catalog;

        private AlgoEnvironment _env;
        private IShell _shell;
        private VarContext _var = new VarContext();
        private TraderClientModel _client;
        private WindowManager _localWnd;
        private DateTime _emulteFrom;
        private DateTime _emulateTo;
        private PluginDescriptor _descriptorCache;

        private BoolProperty _hasDataToSave;
        private BoolProperty _isRunning;
        private BoolProperty _isVisualizing;
        private ITestExecController _tester;
        private Dictionary<string, ISymbolInfo> _testingSymbols;
        private Property<EmulatorStates> _stateProp;
        private BoolProperty _pauseRequestedProp;
        private BoolProperty _resumeRequestedProp;

        private static readonly int[] SpeedToDelayMap = new int[] { 256, 128, 64, 32, 16, 8, 4, 2, 1, 0 };

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, ISymbolCatalog catalog, IShell shell, ProfileManager profile)
        {
            DisplayName = "Backtester";

            _env = env ?? throw new ArgumentNullException("env");
            _shell = shell ?? throw new ArgumentNullException("shell");
            _catalog = catalog;
            _client = client;

            _hasDataToSave = _var.AddBoolProperty();
            _isRunning = _var.AddBoolProperty();
            _isVisualizing = _var.AddBoolProperty();

            _localWnd = new WindowManager(this);

            SetupPage = new BacktesterSetupPageViewModel(client, catalog, env, IsRunning);
            TradesPage = new BacktesterCurrentTradesViewModel(profile);
            TradeHistoryPage = new BacktesterTradeGridViewModel(profile);
            OptimizationPage = new BacktesterOptimizerViewModel(_localWnd, IsRunning);
            ChartPage = new BacktesterChartPageViewModel();
            ResultsPage = new BacktesterReportViewModel();

            CanStart = !IsRunning & client.IsConnected & SetupPage.IsSetupValid;

            SetupPage.PluginSelected += () =>
            {
                OptimizationPage.SetPluign(SetupPage.SelectedPlugin.Value.Descriptor);
            };

            OptimizationResultsPage.ShowDetailsRequested += async r =>
            {
                ResultsPage.Clear();
                ResultsPage.ShowReport(r.Stats, _descriptorCache, r.Config.Id);
                ResultsPage.AddEquityChart(Convert(r.Equity));
                ResultsPage.AddMarginChart(Convert(r.Margin));
                await ActivateItemAsync(ResultsPage);
                //ActivateItem(ResultsPage);
            };

            _var.TriggerOnChange(SetupPage.ModeProp, a =>
            {
                OptimizationPage.IsVisible = a.New.Value == BacktesterMode.Optimization;
            });

            InitExecControl();

            Items.Add(SetupPage);
            Items.Add(OptimizationPage);
            Items.Add(JournalPage);
            Items.Add(TradesPage);
            Items.Add(TradeHistoryPage);
            Items.Add(ChartPage);
            Items.Add(OptimizationResultsPage);
            Items.Add(ResultsPage);
        }

        //public Property<ActionOverlayViewModel> ActionOverlay { get; private set; }
        public ActionViewModel ProgressMonitor { get; } = new ActionViewModel();

        public BoolVar IsRunning => _isRunning.Var;
        public BoolVar IsVisualizing => _isVisualizing.Var;
        //public BoolVar IsStopping { get; }
        public BoolVar CanStart { get; }
        public BoolVar CanPause { get; private set; }
        public BoolVar CanResume { get; private set; }
        public BoolVar CanStop { get; private set; }
        public BoolVar CanCanel { get; private set; }
        public BoolVar CanControlSpeed { get; private set; }
        public IntProperty SelectedSpeed { get; private set; }
        //public IEnumerable<Feed.Types.Timeframe> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();

        public BacktesterSetupPageViewModel SetupPage { get; }
        public BacktesterJournalViewModel JournalPage { get; } = new BacktesterJournalViewModel();
        public BacktesterReportViewModel ResultsPage { get; }
        public BacktesterChartPageViewModel ChartPage { get; }
        public BacktesterTradeGridViewModel TradeHistoryPage { get; }
        public BacktesterCurrentTradesViewModel TradesPage { get; private set; }
        public BacktesterOptimizerViewModel OptimizationPage { get; }
        public OptimizationResultsPageViewModel OptimizationResultsPage { get; } = new OptimizationResultsPageViewModel();

        private async Task DoEmulation(IActionObserver observer)
        {
            try
            {
                var cToken = observer.CancelationToken;

                _isVisualizing.Clear();
                ChartPage.Clear();
                ResultsPage.Clear();
                JournalPage.Clear();
                TradeHistoryPage.OnTesterStart(SetupPage.Settings.AccType);
                _hasDataToSave.Clear();
                TradesPage.Clear();

                SetupPage.CheckDuplicateSymbols();

                var config = new BacktesterConfig();
                SetupPage.Apply(config);
                config.Env.FeedCachePath = _catalog.OnlineCollection.StorageFolder;
                config.Env.CustomFeedCachePath = _catalog.CustomCollection.StorageFolder;
                config.Env.WorkingFolderPath = EnvService.Instance.AlgoWorkingFolder;

                try
                {
                    config.Validate();
                }
                catch (Exception ex)
                {
                    observer.StopProgress($"Validation error: {ex.Message}");
                    return;
                }

                var descriptorName = SetupPage.SelectedPlugin.Value.Descriptor.DisplayName;
                var pathPrefix = System.IO.Path.Combine(EnvService.Instance.BacktestResultsFolder, descriptorName);
                var configPath = PathHelper.GenerateUniqueFilePath(pathPrefix, ".zip");
                config.Save(configPath);

                _emulteFrom = DateTime.SpecifyKind(SetupPage.DateRange.From, DateTimeKind.Utc);
                _emulateTo = DateTime.SpecifyKind(SetupPage.DateRange.To, DateTimeKind.Utc);

                await SetupPage.PrecacheData(observer, _emulteFrom, _emulateTo);

                cToken.ThrowIfCancellationRequested();

                string resultsPath = default;
                try
                {
                    FireOnStart(SetupPage.MainSymbolSetup.SelectedSymbol.Value, config);
                    resultsPath = await RunBacktester(observer, configPath, cToken);
                }
                finally
                {
                    FireOnStop(config);
#if !DEBUG
                    // Leave config outside results archive to debug backtester host if needed
                    System.IO.File.Delete(configPath);
#endif
                }

                try
                {
                    await LoadResults(observer, resultsPath);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to load results");
                    observer.StopProgress($"Can't load results: {ex.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                observer.StopProgress("Canceled.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during emulation");
                observer.StopProgress($"Emulation error: {ex.Message}");
            }
        }

        private async Task<string> RunBacktester(IActionObserver observer, string configPath, CancellationToken cToken)
        {
            //var progressMin = _emulteFrom.GetAbsoluteDay();

            //observer.StartProgress(progressMin, _emulateTo.GetAbsoluteDay());
            const int progressMax = 100;
            observer.StartProgress(0, progressMax);
            observer.SetMessage("Emulating...");

            BacktesterRunner.Instance.BinDirPath = System.IO.Path.Combine(EnvService.Instance.AppFolder, "bin", "backtester");
            BacktesterRunner.Instance.WorkDir = EnvService.Instance.BacktestResultsFolder;
            using (var tester = await BacktesterRunner.Instance.NewInstance(configPath))
            {
                using (var reg = cToken.Register(() => tester.Stop()))
                {
                    tester.OnProgressUpdate.Subscribe(update => Execute.OnUIThread(() => observer.SetProgress(progressMax * update.Current / update.Total)));
                    await tester.Start();
                    await tester.AwaitStop();
                }
                return tester.GetResultsPath();
            }
        }

        private async Task LoadResults(IActionObserver observer, string resultsPath)
        {
            observer.SetMessage("Loading results...");

            var results = await Task.Run(() => BacktesterResults.Load(resultsPath));

            var execStatus = results.ExecStatus;
            if (execStatus.ResultsNotCorrupted)
            {
                var config = results.GetConfig();

                _testingSymbols = config.TradeServer.Symbols.Values.ToDictionary(s => s.Name, v => (ISymbolInfo)v);

                await LoadStats(observer, results);
                await LoadJournal(observer, results);
                var reports = await LoadTradeHistory(observer, results);
                await LoadChartData(config, results, reports, observer);
                TradeHistoryPage.LoadTradeHistory(reports);
            }

            if (execStatus.HasError)
                observer.StopProgress(execStatus.ToString());
            else
                observer.SetMessage(execStatus.ToString());
        }

        private void FireOnStart(ISymbolData mainSymbol, BacktesterConfig config)
        {
            if (config.Core.Mode == BacktesterMode.Backtesting)
            {
                FireOnStartBacktesting(mainSymbol, config);
            }
            else if (config.Core.Mode == BacktesterMode.Optimization)
            {
                FireOnStartOptimizing();
            }
        }

        private void FireOnStop(BacktesterConfig config)
        {
            if (config.Core.Mode == BacktesterMode.Backtesting)
            {
                FireOnStopBacktesting();
            }
            else if (config.Core.Mode == BacktesterMode.Optimization)
            {
                FireOnStopOptimizing();
            }
        }

        private void FireOnStartBacktesting(ISymbolData mainSymbol, BacktesterConfig config)
        {
            var symbols = SetupPage.FeedSymbols.Select(ss => ss.SelectedSymbol.Value.Info).ToList();
            var currecnies = _client.Currencies.Snapshot.Values.ToList();

            JournalPage.IsVisible = true;
            ChartPage.IsVisible = true;
            TradeHistoryPage.IsVisible = true;
            ResultsPage.IsVisible = true;

            ChartPage.OnStart(IsVisualizing.Value, new SymbolInfo(mainSymbol.Info), config, symbols);
            if (IsVisualizing.Value)
            {
                TradesPage.IsVisible = true;
                TradesPage.Start(config, currecnies, symbols);
            }
            else
                TradesPage.IsVisible = false;

            OptimizationResultsPage.Hide();
        }

        private void FireOnStopBacktesting()
        {
            ChartPage.OnStop();
            if (IsVisualizing.Value)
                TradesPage.Stop();
        }

        private void FireOnStartOptimizing()
        {
            JournalPage.IsVisible = false;
            ChartPage.IsVisible = false;
            TradesPage.IsVisible = false;
            TradeHistoryPage.IsVisible = false;
            ResultsPage.IsVisible = false;

            OptimizationResultsPage.Start(OptimizationPage.GetSelectedParams());
        }

        private void FireOnStopOptimizing()
        {
            OptimizationResultsPage.Stop();
        }

        private async Task LoadJournal(IActionObserver observer, BacktesterResults results)
        {
            observer.SetMessage("Loading journal...");

            await JournalPage.LoadJournal(results);

            //await Task.Run(() =>
            //{
            //foreach (var record in results.Journal)
            //{
            //    JournalPage.Append(record);
            //}
            //});
        }

        private async Task<List<BaseTransactionModel>> LoadTradeHistory(IActionObserver observer, BacktesterResults results)
        {
            observer.SetMessage("Loading trade history...");

            var tradeHistory = new List<BaseTransactionModel>(results.TradeHistory.Count);
            var accType = SetupPage.Settings.AccType;

            await Task.Run(() =>
            {
                foreach (var record in results.TradeHistory)
                {
                    var trRep = BaseTransactionModel.Create(accType, record, 5, _testingSymbols.GetOrDefault(record.Symbol));
                    tradeHistory.Add(trRep);
                }
            });

            return tradeHistory;
        }

        private void AddTradeHistoryReport(TradeReportInfo record)
        {
            var accType = SetupPage.Settings.AccType;
            var trRep = BaseTransactionModel.Create(accType, record, 5, _testingSymbols.GetOrDefault(record.Symbol));
            TradeHistoryPage.Append(trRep);
        }

        private async Task LoadStats(IActionObserver observer, BacktesterResults results)
        {
            observer.SetMessage("Loading testing result data...");

            ResultsPage.ShowReport(results.Stats, results.PluginInfo, null);
        }

        private async Task LoadChartData(BacktesterConfig config, BacktesterResults results, IEnumerable<BaseTransactionModel> tradeHistory, IActionObserver observer)
        {
            var mainSymbol = config.Core.MainSymbol;
            var mainTimeFrame = config.Core.MainTimeframe;
            //var count = results.Feed[mainSymbol].Count;

            //timeFrame = BarExtentions.AdjustTimeframe(timeFrame, count, 500, out count);

            if (results.Feed.TryGetValue(mainSymbol, out var mainBars))
            {
                observer.SetMessage("Loading feed chart data ...");
                await ChartPage.LoadMainChart(mainBars, mainTimeFrame, tradeHistory);
            }

            if (results.PluginInfo.IsTradeBot)
            {
                observer.SetMessage("Loading equity chart data...");
                var equityChartData = await LoadBarSeriesAsync(results.Equity);

                ResultsPage.AddEquityChart(equityChartData);

                observer.SetMessage("Loading margin chart data...");
                var marginChartData = await LoadBarSeriesAsync(results.Margin);

                ResultsPage.AddMarginChart(marginChartData);
            }

            //ChartPage.SetFeedSeries(feedChartData);
            //ChartPage.SetEquitySeries(equityChartData);
            //ChartPage.SetMarginSeries(marginChartData);
        }

        private Task<OhlcDataSeries<DateTime, double>> LoadBarSeriesAsync(IEnumerable<BarData> src)
        {
            return Task.Run(() =>
            {
                var chartData = new OhlcDataSeries<DateTime, double>();

                foreach (var bar in src)
                {
                    if (!double.IsNaN(bar.Open))
                        chartData.Append(bar.OpenTime.ToUtcDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
                }

                return chartData;
            });
        }

        private OhlcDataSeries<DateTime, double> Convert(IEnumerable<BarData> bars)
        {
            var chartData = new OhlcDataSeries<DateTime, double>();

            foreach (var bar in bars)
            {
                if (!double.IsNaN(bar.Open))
                    chartData.Append(bar.OpenTime.ToUtcDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
            }

            return chartData;
        }

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(!_isRunning.Value);
        }

        //public override void CanClose(Action<bool> callback)
        //{
        //    callback(!_isRunning.Value);
        //}

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            _var.Dispose();
            return base.TryCloseAsync(dialogResult);
        }

        //public override void TryClose(bool? dialogResult = null)
        //{
        //    base.TryClose(dialogResult);

        //    _var.Dispose();
        //}

        #region Execution control

        public async void StartEmulation()
        {
            //var actionObserver = new ActionViewModel();
            ///yield return OverlayPanel.ShowDialog(null);
            ///

            //var observer = new ActionOverlayViewModel(DoEmulation);

            SetupPage.CloseSetupDialog();

            //ActionOverlay.Value = observer;
            _isRunning.Set();
            ProgressMonitor.Start(DoEmulation);
            await ProgressMonitor.Completion;

            _isRunning.Clear();
            //ActionOverlay.Value = null;
        }

        public void Cancel()
        {
            ProgressMonitor.Cancel();
        }

        public void PauseEmulation()
        {
            _tester?.Pause();
        }

        public void ResumeEmulation()
        {
            _pauseRequestedProp.Set();
            _tester?.Resume();
        }

        private void InitExecControl()
        {
            _stateProp = _var.AddProperty<EmulatorStates>();
            _resumeRequestedProp = _var.AddBoolProperty();
            _pauseRequestedProp = _var.AddBoolProperty();

            CanPause = _isVisualizing.Var & _stateProp.Var == EmulatorStates.Running & !_pauseRequestedProp.Var;
            CanResume = _stateProp.Var == EmulatorStates.Paused & !_pauseRequestedProp.Var;
            CanCanel = ProgressMonitor.CanCancel;
            CanControlSpeed = _isVisualizing.Var;
            SelectedSpeed = _var.AddIntProperty();
            _var.TriggerOnChange(SelectedSpeed, a =>
            {
                if (IsVisualizing.Value)
                {
                    var delay = SpeedToDelayMap[a.New];
                    _tester?.SetExecDelay(delay);
                }
            });

            _var.TriggerOnChange(_stateProp, a =>
            {
                _pauseRequestedProp.Clear();
                _resumeRequestedProp.Clear();
            });
        }

        #endregion
    }
}
