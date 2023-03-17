using Caliburn.Micro;
using Machinarium.Var;
using Microsoft.Win32;
using NLog;
//using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
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
                //ResultsPage.AddEquityChart(Convert(r.Equity));
                //ResultsPage.AddMarginChart(Convert(r.Margin));
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


        private void ResetResultsView()
        {
            _isVisualizing.Clear();
            ChartPage.Clear();
            ResultsPage.Clear();
            JournalPage.Clear();
            TradeHistoryPage.Clear();
            _hasDataToSave.Clear();
            TradesPage.Clear();
        }

        private async Task DoEmulation(IActionObserver observer)
        {
            try
            {
                var cToken = observer.CancelationToken;

                ResetResultsView();

                SetupPage.CheckDuplicateSymbols();

                var config = SetupPage.CreateConfig();

                try
                {
                    config.Validate();
                }
                catch (Exception ex)
                {
                    observer.StopProgress($"Validation error: {ex.Message}");
                    return;
                }

                var descriptorName = PathHelper.Escape(SetupPage.SelectedPlugin.Value.Descriptor.DisplayName);
                var backtesterDir = PathHelper.EnsureDirectoryCreated(EnvService.Instance.BacktestResultsFolder); // could be deleted between emulations
                var pathPrefix = System.IO.Path.Combine(backtesterDir, descriptorName);
                var configPath = PathHelper.GenerateUniqueFilePath(pathPrefix, ".zip");
                config.Save(configPath);

                _emulteFrom = DateTime.SpecifyKind(SetupPage.DateRange.From, DateTimeKind.Utc);
                _emulateTo = DateTime.SpecifyKind(SetupPage.DateRange.To, DateTimeKind.Utc);

                await SetupPage.PrecacheData(observer, _emulteFrom, _emulateTo);

                cToken.ThrowIfCancellationRequested();

                string resultsPath = default;
                try
                {
                    FireOnStart(config);
                    resultsPath = await RunBacktester(observer, configPath, cToken);
                }
                finally
                {
                    FireOnStop(config);
                    System.IO.File.Delete(configPath);
                }

                try
                {
                    await LoadResults(observer, resultsPath, false);
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

        private async Task LoadResults(IActionObserver observer, string resultsPath, bool loadConfig)
        {
            observer.SetMessage("Loading results...");

            var results = await Task.Run(() => BacktesterResults.Load(resultsPath));

            var execStatus = results.ExecStatus;
            if (execStatus.ResultsNotCorrupted)
            {
                var config = results.TryGetConfig().ResultValue;

                if (loadConfig)
                    await SetupPage.LoadConfig(config);

                OnLoadingResults(config);

                await LoadStats(observer, results);
                await LoadJournal(observer, results);
                await LoadTradeHistory(observer, results, config);
                await LoadChartData(config, results, TradeHistoryPage.Reports, observer);
            }

            var statusBuilder = new StringBuilder();
            statusBuilder.AppendLine(execStatus.ToString());
            if (loadConfig && SetupPage.SelectedPlugin.Value == null)
                statusBuilder.AppendLine($"Missing package '{SetupPage.PluginConfig?.Key?.PackageId}'");
            var hasError = execStatus.HasError;
            if (results.ReadErrors.Count > 0)
            {
                hasError = true;
                statusBuilder.AppendLine(results.FormatReadErrors());
                foreach (var error in results.ReadErrors)
                {
                    var ex = error.Exception;
                    if (ex != null)
                        _logger.Error(ex, "Backtester results read error");
                }
            }

            var status = statusBuilder.ToString().TrimEnd();
            if (hasError)
                observer.StopProgress(status);
            else
                observer.SetMessage(status);
        }

        private void FireOnStart(BacktesterConfig config)
        {
            if (config.Core.Mode == BacktesterMode.Visualization)
            {
                JournalPage.IsVisible = true;
                ChartPage.IsVisible = true;
                TradeHistoryPage.IsVisible = true;
                ResultsPage.IsVisible = true;
                TradesPage.IsVisible = true;

                OptimizationResultsPage.IsVisible = false;

                ChartPage.OnStart(config);
                TradeHistoryPage.OnStart(config);
                TradesPage.Start(config);
            }
        }

        private void FireOnStop(BacktesterConfig config)
        {
            if (config.Core.Mode == BacktesterMode.Visualization)
            {
                TradesPage.Stop();
            }
        }

        private void OnLoadingResults(BacktesterConfig config)
        {
            if (config.Core.Mode == BacktesterMode.Backtesting || config.Core.Mode == BacktesterMode.Visualization)
            {
                OnLoadingBacktestingResults(config);
            }
            else if (config.Core.Mode == BacktesterMode.Optimization)
            {
                OnLoadingOptimizationResults();
            }
        }

        private void OnLoadingBacktestingResults(BacktesterConfig config)
        {
            JournalPage.IsVisible = true;
            ChartPage.IsVisible = true;
            TradeHistoryPage.IsVisible = true;
            ResultsPage.IsVisible = true;

            TradesPage.IsVisible = false;
            OptimizationResultsPage.IsVisible = false;

            ChartPage.Init(config);
            TradeHistoryPage.Init(config);
        }

        private void OnLoadingOptimizationResults()
        {
            JournalPage.IsVisible = false;
            ChartPage.IsVisible = false;
            TradeHistoryPage.IsVisible = false;
            ResultsPage.IsVisible = false;
            TradesPage.IsVisible = false;

            OptimizationPage.IsVisible = true;

            OptimizationResultsPage.Init(OptimizationPage.GetSelectedParams());
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

        private async Task LoadTradeHistory(IActionObserver observer, BacktesterResults results, BacktesterConfig config)
        {
            observer.SetMessage("Loading trade history...");

            await TradeHistoryPage.LoadTradeHistory(results, config);
        }

        private void AddTradeHistoryReport(TradeReportInfo record)
        {
            TradeHistoryPage.Append(record);
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

                ChartPage.LoadMainChart(mainBars, tradeHistory);
                ChartPage.LoadOutputs(config, results);
            }

            if (results.PluginInfo.IsTradeBot)
            {
                observer.SetMessage("Loading equity chart data...");
                //var equityChartData = await LoadBarSeriesAsync(results.Equity);

                //ResultsPage.AddEquityChart(equityChartData);

                //observer.SetMessage("Loading margin chart data...");
                //var marginChartData = await LoadBarSeriesAsync(results.Margin);

                //ResultsPage.AddMarginChart(marginChartData);
            }

            //ChartPage.SetFeedSeries(feedChartData);
            //ChartPage.SetEquitySeries(equityChartData);
            //ChartPage.SetMarginSeries(marginChartData);
        }

        //private Task<OhlcDataSeries<DateTime, double>> LoadBarSeriesAsync(IEnumerable<BarData> src)
        //{
        //    return Task.Run(() =>
        //    {
        //        var chartData = new OhlcDataSeries<DateTime, double>();

        //        foreach (var bar in src)
        //        {
        //            if (!double.IsNaN(bar.Open))
        //                chartData.Append(bar.OpenTime.ToUtcDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
        //        }

        //        return chartData;
        //    });
        //}

        //private OhlcDataSeries<DateTime, double> Convert(IEnumerable<BarData> bars)
        //{
        //    var chartData = new OhlcDataSeries<DateTime, double>();

        //    foreach (var bar in bars)
        //    {
        //        if (!double.IsNaN(bar.Open))
        //            chartData.Append(bar.OpenTime.ToUtcDateTime(), bar.Open, bar.High, bar.Low, bar.Close);
        //    }

        //    return chartData;
        //}

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


        #region Export/import

        private const string ZipFileFilter = "Zip files (*.zip)|*.zip";

        public IEnumerable<IResult> SaveConfig()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = PathHelper.Escape(SetupPage.SelectedPlugin.Value.Descriptor.DisplayName) + ".zip";
            dialog.Filter = ZipFileFilter;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                string saveError = null;

                try
                {
                    var config = SetupPage.CreateConfig();
                    config.Save(dialog.FileName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to save backtester config");
                    saveError = ex.Message;
                }

                if (saveError != null)
                    yield return VmActions.ShowError($"Can't save backtester config: {saveError}", "Error");
            }
        }

        public IEnumerable<IResult> LoadConfig()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = ZipFileFilter;
            dialog.CheckFileExists = true;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                string loadError = null;

                try
                {
                    var _ = LoadConfigWrapped(dialog.FileName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to load backtester config");
                    loadError = ex.Message;
                }

                if (loadError != null)
                    yield return VmActions.ShowError($"Can't load backtester config: {loadError}", "Error");
            }
        }

        public IEnumerable<IResult> LoadResults()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = ZipFileFilter;
            dialog.CheckFileExists = true;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                string loadError = null;

                try
                {
                    var _ = LoadResultsWrapped(dialog.FileName);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to load backtester results");
                    loadError = ex.Message;
                }

                if (loadError != null)
                    yield return VmActions.ShowError($"Can't load backtester results: {loadError}", "Error");
            }
        }


        private async Task LoadConfigWrapped(string configPath)
        {
            SetupPage.CloseSetupDialog();

            IActionObserver observer = ProgressMonitor.Progress;
            _isRunning.Set();

            try
            {
                ResetResultsView();

                var result = BacktesterConfig.TryLoad(configPath);

                if (!result)
                    throw result.Exception;

                await SetupPage.LoadConfig(result.ResultValue);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load config");
                observer.StopProgress($"Can't load config: {ex.Message}");
            }

            _isRunning.Clear();
        }

        private async Task LoadResultsWrapped(string resultsPath)
        {
            SetupPage.CloseSetupDialog();

            IActionObserver observer = ProgressMonitor.Progress;
            _isRunning.Set();

            try
            {
                ResetResultsView();
                await LoadResults(observer, resultsPath, true);
                observer.StopProgress();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load results");
                observer.StopProgress($"Can't load results: {ex.Message}");
            }

            _isRunning.Clear();
        }

        #endregion
    }
}
