using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using Microsoft.Win32;
using NLog;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using LB = TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Core.Lib;
using System.Globalization;

namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Conductor<Page>.Collection.OneActive, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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
        private Dictionary<string, SymbolEntity> _testingSymbols;
        private Property<EmulatorStates> _stateProp;
        private BoolProperty _pauseRequestedProp;
        private BoolProperty _resumeRequestedProp;
        
        private static readonly int[] SpeedToDelayMap = new int[] { 256, 128, 64, 32, 16, 8, 4, 2, 1, 0 };

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, SymbolCatalog catalog, IShell shell, ProfileManager profile)
        {
            DisplayName = "Backtester";

            _env = env ?? throw new ArgumentNullException("env");
            _shell = shell ?? throw new ArgumentNullException("shell");
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
                OptimizationPage.SetPluign(SetupPage.SelectedPlugin.Value.Descriptor, SetupPage.PluginSetup);
            };

            OptimizationResultsPage.ShowDetailsRequested += r =>
            {
                ResultsPage.Clear();
                ResultsPage.ShowReport(r.Stats, _descriptorCache, r.Config.Id);
                ResultsPage.AddEquityChart(Convert(r.Equity));
                ResultsPage.AddMarginChart(Convert(r.Margin));
                ActivateItem(ResultsPage);
            };

            _var.TriggerOnChange(SetupPage.ModeProp, a =>
            {
                OptimizationPage.IsVisible = a.New.Value == TesterModes.Optimization;
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
        //public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();

        public BacktesterSetupPageViewModel SetupPage { get; }
        public BacktesterJournalViewModel JournalPage { get; } = new BacktesterJournalViewModel();
        public BacktesterReportViewModel ResultsPage { get; }
        public BacktesterChartPageViewModel ChartPage { get; }
        public BacktesterTradeGridViewModel TradeHistoryPage { get; }
        public BacktesterCurrentTradesViewModel TradesPage { get; private set; }
        public BacktesterOptimizerViewModel OptimizationPage { get; }
        public OptimizationResultsPageViewModel OptimizationResultsPage { get; } = new OptimizationResultsPageViewModel();

        private async Task DoEmulation(IActionObserver observer, CancellationToken cToken)
        {
            try
            {
                _isVisualizing.Clear();
                ChartPage.Clear();
                ResultsPage.Clear();
                JournalPage.Clear();
                TradeHistoryPage.OnTesterStart(SetupPage.Settings.AccType);
                _hasDataToSave.Clear();
                TradesPage.Clear();

                SetupPage.CheckDuplicateSymbols();

                _emulteFrom = DateTime.SpecifyKind(SetupPage.DateRange.From, DateTimeKind.Utc);
                _emulateTo = DateTime.SpecifyKind(SetupPage.DateRange.To, DateTimeKind.Utc);

                if (_emulteFrom == _emulateTo)
                    throw new Exception("Zero range!");

                await SetupPage.PrecacheData(observer, cToken, _emulteFrom, _emulateTo);

                cToken.ThrowIfCancellationRequested();

                await SetupAndRunBacktester(observer, cToken);
            }
            catch (OperationCanceledException)
            {
                observer.SetMessage("Canceled.");
            }
        }

        private async Task SetupAndRunBacktester(IActionObserver observer, CancellationToken cToken)
        {
            var chartSymbol = SetupPage.MainSymbolSetup.SelectedSymbol.Value;
            var chartTimeframe = SetupPage.MainSymbolSetup.SelectedTimeframe.Value;
            var chartPriceLayer = BarPriceType.Bid;

            SetupPage.InitToken();

            //var packageRef = _env.LocalAgent.Library.GetPackageRef(SelectedPlugin.Value.Info.Key.GetPackageKey());
            var pluginRef = _env.LocalAgent.Library.GetPluginRef(SetupPage.SelectedPlugin.Value.PluginInfo.Key);
            //var pluginSetupModel = new PluginSetupModel(pluginRef, this, this);

            _descriptorCache = pluginRef.Metadata.Descriptor;

            //if (PluginConfig != null)
            //    pluginSetupModel.Load(PluginConfig);

            // TODO: place correctly to avoid domain unload during backtester run
            //packageRef.IncrementRef();
            //packageRef.DecrementRef();

            if (SetupPage.Mode == TesterModes.Optimization)
                await DoOptimization(observer, cToken, pluginRef, SetupPage.PluginSetup, chartSymbol, chartTimeframe, chartPriceLayer);
            else
                await DoBacktesting(observer, cToken, pluginRef, SetupPage.PluginSetup, chartSymbol, chartTimeframe, chartPriceLayer);
        }

        private async Task DoBacktesting(IActionObserver observer, CancellationToken cToken, AlgoPluginRef pluginRef, PluginSetupModel pluginSetupModel,
            SymbolData chartSymbol, TimeFrames chartTimeframe, BarPriceType chartPriceLayer)
        {
            var progressMin = _emulteFrom.GetAbsoluteDay();

            observer.StartProgress(progressMin, _emulateTo.GetAbsoluteDay());
            observer.SetMessage("Emulating...");

            using (var backtester = new Backtester(pluginRef, new DispatcherSync(), _emulteFrom, _emulateTo))
            {
                OnStartTesting(backtester);

                try
                {
                    pluginSetupModel.Apply(backtester);

                    backtester.Executor.LogUpdated += JournalPage.Append;
                    backtester.Executor.TradeHistoryUpdated += Executor_TradeHistoryUpdated;

                    if (SetupPage.Mode == TesterModes.Visualization)
                    {
                        _isVisualizing.Set();

                        var delay = SpeedToDelayMap[SelectedSpeed.Value];
                        backtester.SetExecDelay(delay);
                        backtester.StreamExecReports = true;
                    }

                    Exception execError = null;

                    System.Action updateProgressAction = () => observer.SetProgress(backtester.CurrentTimePoint?.GetAbsoluteDay() ?? progressMin);

                    using (new UiUpdateTimer(updateProgressAction))
                    {
                        try
                        {
                            SetupPage.Apply(backtester, _emulteFrom, _emulateTo, _isVisualizing.Value);
                            backtester.Feed.AddBarBuilder(chartSymbol.Name, chartTimeframe, chartPriceLayer);

                            _testingSymbols = backtester.CommonSettings.Symbols;

                            FireOnStart(chartSymbol, pluginSetupModel, backtester);

                            _hasDataToSave.Set();

                            await Task.Run(() => backtester.Run(cToken));

                            observer.SetProgress(_emulateTo.GetAbsoluteDay());
                        }
                        catch (Exception ex)
                        {
                            execError = ex;
                        }

                        FireOnStop(backtester);
                    }

                    await LoadStats(observer, backtester);
                    await LoadChartData(backtester, observer);

                    if (SetupPage.SaveResultsToFile.Value)
                        await SaveResults(pluginSetupModel, observer);

                    if (execError != null)
                    {
                        if (execError is AlgoOperationCanceledException)
                            observer.SetMessage("Canceled by user!");
                        else
                            throw execError; //observer.SetMessage(execError.Message);
                    }
                    else
                        observer.SetMessage("Done.");
                }
                finally
                {
                    OnStopTesting();
                    //backtester = null;
                }
            }
        }

        private async Task DoOptimization(IActionObserver observer, CancellationToken cToken, AlgoPluginRef pluginRef, PluginSetupModel pluginSetupModel,
            SymbolData chartSymbol, TimeFrames chartTimeframe, BarPriceType chartPriceLayer)
        {
            using (var optimizer = new Optimizer(pluginRef, new DispatcherSync()))
            {
                OnStartTesting(optimizer);

                try
                {
                    pluginSetupModel.Apply(optimizer);

                    Exception execError = null;

                    try
                    {
                        SetupPage.Apply(optimizer, _emulteFrom, _emulateTo);

                        optimizer.Feed.AddBarBuilder(chartSymbol.Name, chartTimeframe, chartPriceLayer);

                        _testingSymbols = optimizer.CommonSettings.Symbols;

                        // setup params
                        OptimizationPage.Apply(optimizer);

                        FireOnStart(pluginRef, optimizer);

                        _hasDataToSave.Set();

                        var maxProgress = optimizer.MaxCasesNo;

                        observer.StartProgress(0, maxProgress);
                        observer.SetMessage(GetOptimizationProgressMessage(maxProgress, 0));

                        Action<OptCaseReport, long> repHandler = (r, cl) =>
                        {
                            var progress = maxProgress - cl;
                            observer.SetProgress(progress);
                            observer.SetMessage(GetOptimizationProgressMessage(maxProgress, progress));
                            OptimizationResultsPage.Update(r);
                        };

                        optimizer.CaseCompleted += repHandler;

                        try
                        {
                            await Task.Run(() => optimizer.Run(cToken));
                        }
                        finally
                        {
                            optimizer.CaseCompleted -= repHandler;
                        }
                    }
                    catch (Exception ex)
                    {
                        execError = ex;
                    }

                    FireOnStop(optimizer);

                    if (SetupPage.SaveResultsToFile.Value)
                        await OptimizationResultsPage.SaveReportAsync(pluginSetupModel, observer);

                    if (execError != null)
                    {
                        if (execError is AlgoOperationCanceledException)
                            observer.SetMessage("Canceled by user!");
                        else
                            throw execError; //observer.SetMessage(execError.Message);
                    }
                    else
                        observer.SetMessage("Done.");
                }
                finally
                {
                    OnStopTesting();
                }
            }
        }

        private void FireOnStart(SymbolData mainSymbol, PluginSetupModel setup, Backtester tester)
        {
            var symbols = SetupPage.GetAllSymbols().Select(ss => ss.SelectedSymbol.Value.InfoEntity).ToList();
            var currecnies = _client.Currencies.Snapshot.Values.ToList();

            JournalPage.IsVisible = true;
            ChartPage.IsVisible = true;
            TradeHistoryPage.IsVisible = true;
            ResultsPage.IsVisible = true;

            ChartPage.OnStart(IsVisualizing.Value, mainSymbol.InfoEntity, setup, tester, symbols);
            if (IsVisualizing.Value)
            {
                TradesPage.IsVisible = true;
                TradesPage.Start(tester, currecnies, symbols);
            }
            else
                TradesPage.IsVisible = false;

            OptimizationResultsPage.Hide();
        }

        private void FireOnStop(Backtester tester)
        {
            ChartPage.OnStop(tester);
            if (IsVisualizing.Value)
                TradesPage.Stop(tester);
        }

        private void FireOnStart(AlgoPluginRef pluginRef, Optimizer tester)
        {
            JournalPage.IsVisible = false;
            ChartPage.IsVisible = false;
            TradesPage.IsVisible = false;
            TradeHistoryPage.IsVisible = false;
            ResultsPage.IsVisible = false;

            OptimizationResultsPage.Start(OptimizationPage.GetSelectedParams(), tester);
        }

        private void FireOnStop(Optimizer tester)
        {
            OptimizationResultsPage.Stop(tester);
        }

        private PluginLogRecord CreateInfoRecord(uint no, string message)
        {
            return new PluginLogRecord(new TimeKey(_emulteFrom, no), LogSeverities.Info, message, null);
        }

        private void Executor_TradeHistoryUpdated(TradeReportEntity record)
        {
            var currencies = _client.Currencies;
            var symbols = _testingSymbols.Select(kv => new SymbolModel(kv.Value, currencies)).ToDictionary(m => m.Name);

            var accType = SetupPage.Settings.AccType;
            var trRep = TransactionReport.Create(accType, record, symbols.GetOrDefault(record.Symbol));
            TradeHistoryPage.Append(trRep);
            ChartPage.Append(accType, trRep);
        }

        private async Task LoadStats(IActionObserver observer, Backtester tester)
        {
            observer.SetMessage("Loading testing result data...");

            var statProperties = await Task.Factory.StartNew(() => tester.GetStats());
            ResultsPage.ShowReport(statProperties, tester.PluginInfo, null);
        }

        private async Task LoadChartData(Backtester backtester, IActionObserver observer)
        {
            var mainSymbol = backtester.CommonSettings.MainSymbol;
            var timeFrame = backtester.CommonSettings.MainTimeframe;
            var count = backtester.GetSymbolHistoryBarCount(mainSymbol);

            timeFrame = BarExtentions.AdjustTimeframe(timeFrame, count, 500, out count);

            //observer.SetMessage("Loading feed chart data ...");
            //var feedChartData = await LoadBarSeriesAsync(tester.GetMainSymbolHistory(timeFrame), observer, timeFrame, count);

            if (backtester.PluginInfo.Type == AlgoTypes.Robot)
            {
                observer.SetMessage("Loading equity chart data...");
                var equityChartData = await LoadBarSeriesAsync(backtester.GetEquityHistory(timeFrame), observer, timeFrame, count);

                ResultsPage.AddEquityChart(equityChartData);

                observer.SetMessage("Loading margin chart data...");
                var marginChartData = await LoadBarSeriesAsync(backtester.GetMarginHistory(timeFrame), observer, timeFrame, count);

                ResultsPage.AddMarginChart(marginChartData);
            }

            //ChartPage.SetFeedSeries(feedChartData);
            //ChartPage.SetEquitySeries(equityChartData);
            //ChartPage.SetMarginSeries(marginChartData);
        }

        private Task<OhlcDataSeries<DateTime, double>> LoadBarSeriesAsync(IPagedEnumerator<BarEntity> src, IActionObserver observer, TimeFrames timeFrame, int totalCount)
        {
            observer.StartProgress(0, totalCount);

            return Task.Run(() =>
            {
                using (src)
                {
                    var chartData = new OhlcDataSeries<DateTime, double>();

                    foreach (var bar in src.JoinPages(i => observer.SetProgress(i)))
                    {
                        if (!double.IsNaN(bar.Open))
                            chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
                    }

                    observer.SetProgress(totalCount);

                    return chartData;
                }
            });
        }

        private OhlcDataSeries<DateTime, double> Convert(IEnumerable<BarEntity> bars)
        {
            var chartData = new OhlcDataSeries<DateTime, double>();

            foreach (var bar in bars)
            {
                if (!double.IsNaN(bar.Open))
                    chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);
            }

            return chartData;
        }

        public override void CanClose(Action<bool> callback)
        {
            callback(!_isRunning.Value);
        }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            _var.Dispose();
        }

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

        private void OnStartTesting(ITestExecController tester)
        {
            _tester = tester;
            _tester.StateChanged += _backtester_StateChanged;
            _tester.ErrorOccurred += Executor_ErrorOccurred;
        }

        private void OnStopTesting()
        {
            _tester.StateChanged -= _backtester_StateChanged;
            _tester.ErrorOccurred -= Executor_ErrorOccurred;
            _stateProp.Value = EmulatorStates.Stopped;
            _tester = null;
        }

        private void _backtester_StateChanged(EmulatorStates state)
        {
            _stateProp.Value = state;
        }

        private void Executor_ErrorOccurred(Exception ex)
        {
            _logger.Error(ex, "Error occurred in backtester!");
        }

        #endregion

        #region Results saving

        private async Task SaveResults(PluginSetupModel pluginSetup, IActionObserver observer)
        {
            var dPlugin = pluginSetup.PluginRef.Metadata.Descriptor;
            var fileName = dPlugin.DisplayName + " " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture) + ".zip";
            var filePath = System.IO.Path.Combine(EnvService.Instance.BacktestResultsFolder, fileName);

            using (var stream = System.IO.File.Open(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
            {
                using (ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    var jounralEntry = archive.CreateEntry("journal.txt", CompressionLevel.Optimal);

                    using (var entryStream = jounralEntry.Open())
                        await JournalPage.SaveToFile(entryStream, observer);

                    var tradeReportsEntry = archive.CreateEntry("trades.csv", CompressionLevel.Optimal);
                    using (var entryStream = tradeReportsEntry.Open())
                        await TradeHistoryPage.SaveAsCsv(entryStream, observer);

                    observer.SetMessage("Saving report...");

                    var summaryEntry = archive.CreateEntry("report.txt", CompressionLevel.Optimal);
                    using (var entryStream = summaryEntry.Open())
                        await Task.Run(() => ResultsPage.SaveAsText(entryStream));

                    var setupEntry = archive.CreateEntry("setup.txt", CompressionLevel.Optimal);
                    using (var entryStream = setupEntry.Open())
                        await Task.Run(() => SetupPage.SaveTestSetupAsText(pluginSetup, entryStream, _emulteFrom, _emulateTo));

                    if (pluginSetup.Metadata.Descriptor.Type == AlgoTypes.Robot)
                    {
                        var equityEntry = archive.CreateEntry("equity.csv", CompressionLevel.Optimal);
                        using (var entryStream = equityEntry.Open())
                            await Task.Run(() => ResultsPage.SaveEquityCsv(entryStream, observer));

                        var marginEntry = archive.CreateEntry("margin.csv", CompressionLevel.Optimal);
                        using (var entryStream = marginEntry.Open())
                            await Task.Run(() => ResultsPage.SaveMarginCsv(entryStream, observer));
                    }
                }
            }
        }

        #endregion

        private string GetOptimizationProgressMessage(long max, long progress)
        {
            return string.Format("Optimizing... {0}/{1}", progress, max);
        }
    }
}
