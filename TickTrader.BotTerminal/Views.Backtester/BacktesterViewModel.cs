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
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Screen, IWindowModel, IAlgoSetupMetadata, IPluginIdProvider, IAlgoSetupContext
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _env;
        private IShell _shell;
        private SymbolCatalog _catalog;

        private SymbolToken _mainSymbolToken;
        private IVarList<ISymbolInfo> _symbolTokens;
        private IReadOnlyList<ISymbolInfo> _observableSymbolTokens;
        private VarContext _var = new VarContext();
        private TraderClientModel _client;
        private WindowManager _localWnd;
        private DateTime _emulteFrom;
        private DateTime _emulateTo;
        private BacktesterSettings _settings = new BacktesterSettings();
        private BoolProperty _allSymbolsValid;
        private BoolProperty _hasDataToSave;
        private BoolProperty _isRunning;
        private BoolProperty _isVisualizing;
        private Backtester _backtester;
        private Property<EmulatorStates> _stateProp;
        private BoolProperty _pauseRequestedProp;
        private BoolProperty _resumeRequestedProp;
        private BacktesterPluginSetupViewModel _openedPluginSetup;

        private static readonly int[] SpeedToDelayMap = new int[] { 256, 128, 64, 32, 16, 8, 4, 2, 1, 0 };

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, SymbolCatalog catalog, IShell shell, ProfileManager profile)
        {
            DisplayName = "Backtester";

            _env = env ?? throw new ArgumentNullException("env");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _shell = shell ?? throw new ArgumentNullException("shell");
            _client = client;

            profile.OpenBacktester = true;

            _allSymbolsValid = _var.AddBoolProperty();
            _hasDataToSave = _var.AddBoolProperty();
            _isRunning = _var.AddBoolProperty();
            _isVisualizing = _var.AddBoolProperty();

            _localWnd = new WindowManager(this);

            //ActionOverlay = new Property<ActionOverlayViewModel>();
            AdditionalSymbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel(false);
            IsUpdatingRange = new BoolProperty();
            MainTimeFrame = new Property<TimeFrames>();

            TradesPage = new BacktesterCurrentTradesViewModel(profile);
            TradeHistoryPage = new BacktesterTradeGridViewModel(profile);

            MainTimeFrame.Value = TimeFrames.M1;

            SaveResultsToFile = new BoolProperty();
            SaveResultsToFile.Set();

            //_availableSymbols = env.Symbols;

            MainSymbolSetup = CreateSymbolSetupModel(SymbolSetupType.Main);
            UpdateSymbolsState();

            AvailableModels = _var.AddProperty<List<TimeFrames>>();
            SelectedModel = _var.AddProperty<TimeFrames>(TimeFrames.M1);

            SelectedPlugin = new Property<AlgoPluginViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsTradeBotSelected = SelectedPlugin.Var.Check(p => p != null && p.Descriptor.Type == AlgoTypes.Robot);
            //IsRunning = ActionOverlay.IsRunning;
            //IsStopping = ActionOverlay.IsCancelling;
            CanStart = !IsRunning & client.IsConnected & !IsUpdatingRange.Var & IsPluginSelected & _allSymbolsValid.Var;
            CanSetup = !IsRunning & client.IsConnected;
            //CanStop = ActionOverlay.CanCancel;
            //CanSave = !IsRunning & _hasDataToSave.Var;
            IsVisualizationEnabled = _var.AddBoolProperty();

            Plugins = env.LocalAgentVM.PluginList;

            TradeSettingsSummary = _var.AddProperty<string>();

            _mainSymbolToken = SpecialSymbols.MainSymbolPlaceholder;
            var predefinedSymbolTokens = new VarList<ISymbolInfo>(new ISymbolInfo[] { _mainSymbolToken });
            var existingSymbolTokens = _catalog.AllSymbols.Select(s => (ISymbolInfo)s.ToSymbolToken());
            _symbolTokens = VarCollection.Combine<ISymbolInfo>(predefinedSymbolTokens, existingSymbolTokens);
            _observableSymbolTokens = _symbolTokens.AsObservable();

            env.LocalAgentVM.Plugins.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove && a.OldItem.Key == SelectedPlugin.Value?.Key)
                    SelectedPlugin.Value = null;
            };

            ChartPage = new BacktesterChartPageViewModel();
            ResultsPage = new BacktesterReportViewModel();

            _var.TriggerOnChange(SelectedPlugin, a =>
            {
                if (a.New != null)
                {
                    PluginConfig = null;
                }
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedTimeframe, a =>
            {
                AvailableModels.Value = EnumHelper.AllValues<TimeFrames>().Where(t => t >= a.New).ToList();

                if (_openedPluginSetup != null)
                    _openedPluginSetup.Setup.SelectedTimeFrame = a.New;

                if (SelectedModel.Value < a.New)
                    SelectedModel.Value = a.New;
            });

            _var.TriggerOnChange(SelectedModel, a =>
            {
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedSymbol, a =>
            {
                _mainSymbolToken.Id = a.New.Name;

                if (_openedPluginSetup != null)
                    _openedPluginSetup.Setup.MainSymbol = a.New.ToSymbolToken();

                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            });

            client.Connected += () =>
            {
                GetAllSymbols().Foreach(s => s.Reset());
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            };

            InitExecControl();
            UpdateTradeSummary();
        }

        //public Property<ActionOverlayViewModel> ActionOverlay { get; private set; }
        public ActionViewModel ProgressMonitor { get; } = new ActionViewModel();
        public IObservableList<AlgoPluginViewModel> Plugins { get; private set; }
        public Property<List<TimeFrames>> AvailableModels { get; private set; }
        public Property<TimeFrames> SelectedModel { get; private set; }
        public Property<AlgoPluginViewModel> SelectedPlugin { get; private set; }
        public Property<TimeFrames> MainTimeFrame { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolSetup { get; private set; }
        public PluginConfig PluginConfig { get; private set; }
        public Property<string> TradeSettingsSummary { get; private set; }
        public BoolProperty IsVisualizationEnabled { get; }
        public BoolProperty SaveResultsToFile { get; }
        public BoolVar IsPluginSelected { get; }
        public BoolVar IsTradeBotSelected { get; }
        public BoolVar IsRunning => _isRunning.Var;
        public BoolVar IsVisualizing => _isVisualizing.Var;
        //public BoolVar IsStopping { get; }
        public BoolVar CanSetup { get; }
        public BoolVar CanStart { get; }
        public BoolVar CanPause { get; private set; }
        public BoolVar CanResume { get; private set; }
        public BoolVar CanStop { get; private set; }
        public BoolVar CanCanel { get; private set; }
        public BoolVar CanControlSpeed { get; private set; }
        public IntProperty SelectedSpeed { get; private set; }
        public BoolProperty IsUpdatingRange { get; private set; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ObservableCollection<BacktesterSymbolSetupViewModel> AdditionalSymbols { get; private set; }
        //public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();

        public BacktesterJournalViewModel JournalPage { get; } = new BacktesterJournalViewModel();
        public BacktesterReportViewModel ResultsPage { get; }
        public BacktesterChartPageViewModel ChartPage { get; }
        public BacktesterTradeGridViewModel TradeHistoryPage { get; }
        public BacktesterCurrentTradesViewModel TradesPage { get; private set; }

        public async void OpenTradeSetup()
        {
            var setup = new BacktesterTradeSetupViewModel(_settings, _client.SortedCurrenciesNames);

            _localWnd.OpenMdiWindow(SetupWndKey, setup);

            if (await setup.Result)
            {
                _settings = setup.GetSettings();
                UpdateTradeSummary();
            }
        }

        [Conditional("DEBUG")]
        public void PrintCacheData()
        {
            MainSymbolSetup.PrintCacheData(SelectedModel.Value);
        }

        #region Plugin Setup

        private const string SetupWndKey = "SetupAuxWnd";

        public void OpenPluginSetup()
        {
            _localWnd.OpenOrActivateWindow(SetupWndKey, () =>
            {
                _openedPluginSetup = PluginConfig == null
                    ? new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo())
                    : new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo(), PluginConfig);
                //_localWnd.OpenMdiWindow(wndKey, _openedPluginSetup);
                _openedPluginSetup.Setup.MainSymbol = MainSymbolSetup.SelectedSymbol.Value.ToSymbolToken();
                _openedPluginSetup.Setup.SelectedTimeFrame = MainSymbolSetup.SelectedTimeframe.Value;
                _openedPluginSetup.Closed += PluginSetupClosed;
                _openedPluginSetup.Setup.ConfigLoaded += Setup_ConfigLoaded;
                return _openedPluginSetup;
            });
        }

        private void CloseSetupDialog()
        {
            _localWnd.CloseWindowByKey(SetupWndKey);
        }

        private void PluginSetupClosed(BacktesterPluginSetupViewModel setup, bool dlgResult)
        {
            if (dlgResult)
                PluginConfig = setup.GetConfig();

            setup.Closed -= PluginSetupClosed;
            setup.Setup.ConfigLoaded -= Setup_ConfigLoaded;

            _openedPluginSetup = null;
        }

        private void Setup_ConfigLoaded(PluginConfigViewModel config)
        {
            MainSymbolSetup.SelectedSymbol.Value = _catalog.GetSymbol(config.MainSymbol);
            MainSymbolSetup.SelectedTimeframe.Value = config.SelectedTimeFrame;
        }

        #endregion

        private async Task DoEmulation(IActionObserver observer, CancellationToken cToken)
        {
            try
            {
                _isVisualizing.Clear();
                ChartPage.Clear();
                ResultsPage.Clear();
                JournalPage.Clear();
                TradeHistoryPage.OnTesterStart(_settings.AccType);
                _hasDataToSave.Clear();
                TradesPage.Clear();

                CheckDuplicateSymbols();

                _emulteFrom = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
                _emulateTo = DateTime.SpecifyKind(DateRange.To, DateTimeKind.Utc);

                if (_emulteFrom == _emulateTo)
                    throw new Exception("Zero range!");

                await PrecacheData(observer, cToken);

                cToken.ThrowIfCancellationRequested();

                await SetupAndRunBacktester(observer, cToken);
            }
            catch (OperationCanceledException)
            {
                observer.SetMessage("Canceled.");
            }
        }

        private async Task PrecacheData(IActionObserver observer, CancellationToken cToken)
        {
            await MainSymbolSetup.PrecacheData(observer, cToken, _emulteFrom, _emulateTo, SelectedModel.Value);

            foreach (var symbolSetup in AdditionalSymbols)
                await symbolSetup.PrecacheData(observer, cToken, _emulteFrom, _emulateTo);
        }

        private async Task SetupAndRunBacktester(IActionObserver observer, CancellationToken cToken)
        {
            var chartSymbol = MainSymbolSetup.SelectedSymbol.Value;
            var chartTimeframe = MainSymbolSetup.SelectedTimeframe.Value;
            var chartPriceLayer = BarPriceType.Bid;

            var progressMin = _emulteFrom.GetAbsoluteDay();

            _mainSymbolToken.Id = chartSymbol.Name;

            observer.StartProgress(progressMin, _emulateTo.GetAbsoluteDay());
            observer.SetMessage("Emulating...");

            var packageRef = _env.LocalAgent.Library.GetPackageRef(SelectedPlugin.Value.Info.Key.GetPackageKey());
            var pluginRef = _env.LocalAgent.Library.GetPluginRef(SelectedPlugin.Value.Info.Key);
            var pluginSetupModel = new PluginSetupModel(pluginRef, this, this);

            if (PluginConfig != null)
                pluginSetupModel.Load(PluginConfig);
            pluginSetupModel.MainSymbolPlaceholder.Id = chartSymbol.Name;

            // TODO: place correctly to avoid domain unload during backtester run
            //packageRef.IncrementRef();
            //packageRef.DecrementRef();

            using (_backtester = new Backtester(pluginRef, new DispatcherSync(), _emulteFrom, _emulateTo))
            {
                OnStartTesting();

                try
                {
                    pluginSetupModel.Apply(_backtester);

                    foreach (var outputSetup in pluginSetupModel.Outputs)
                    {
                        if (outputSetup is ColoredLineOutputSetupModel)
                            _backtester.InitOutputCollection<double>(outputSetup.Id);
                        else if (outputSetup is MarkerSeriesOutputSetupModel)
                            _backtester.InitOutputCollection<Marker>(outputSetup.Id);
                    }

                    _backtester.Executor.LogUpdated += JournalPage.Append;
                    _backtester.Executor.TradeHistoryUpdated += Executor_TradeHistoryUpdated;

                    if (IsVisualizationEnabled.Value)
                    {
                        _isVisualizing.Set();

                        var delay = SpeedToDelayMap[SelectedSpeed.Value];
                        _backtester.SetExecDelay(delay);
                        _backtester.StreamExecReports = true;
                    }

                    Exception execError = null;

                    System.Action updateProgressAction = () => observer.SetProgress(_backtester.CurrentTimePoint?.GetAbsoluteDay() ?? progressMin);

                    using (new UiUpdateTimer(updateProgressAction))
                    {
                        try
                        {
                            MainSymbolSetup.Apply(_backtester, _emulteFrom, _emulateTo, SelectedModel.Value, _isVisualizing.Value);

                            foreach (var symbolSetup in AdditionalSymbols)
                                symbolSetup.Apply(_backtester, _emulteFrom, _emulateTo, _isVisualizing.Value);

                            _backtester.Feed.AddBarBuilder(chartSymbol.Name, chartTimeframe, chartPriceLayer);

                            foreach (var rec in _client.Currencies.Snapshot)
                                _backtester.Currencies.Add(rec.Key, rec.Value);

                            //foreach (var rec in _client.Symbols.Snapshot)
                            //    tester.Symbols.Add(rec.Key, rec.Value.Descriptor);

                            _settings.Apply(_backtester);

                            FireOnStart(chartSymbol, pluginSetupModel, _backtester);

                            _hasDataToSave.Set();

                            await Task.Run(() => _backtester.Run(cToken));

                            observer.SetProgress(DateRange.To.GetAbsoluteDay());
                        }
                        catch (Exception ex)
                        {
                            execError = ex;
                        }

                        FireOnStop(_backtester);
                    }

                    await LoadStats(observer, _backtester);
                    await LoadChartData(_backtester, observer);

                    if (SaveResultsToFile.Value)
                        await SaveResults(pluginSetupModel, observer);

                    if (execError != null)
                        throw execError; //observer.SetMessage(execError.Message);
                    else
                        observer.SetMessage("Done.");
                }
                finally
                {
                    OnStopTesting();
                    _backtester = null;
                }
            }
        }

        private void FireOnStart(SymbolData mainSymbol, PluginSetupModel setup, Backtester tester)
        {
            var symbols = GetAllSymbols().Select(ss => ss.SelectedSymbol.Value.InfoEntity).ToList();
            var currecnies = _client.Currencies.Snapshot.Values.ToList();

            ChartPage.OnStart(IsVisualizing.Value, mainSymbol.InfoEntity, setup, tester, symbols);
            if (IsVisualizing.Value)
                TradesPage.Start(tester, currecnies, symbols);
        }

        private void FireOnStop(Backtester tester)
        {
            ChartPage.OnStop(tester);
            if (IsVisualizing.Value)
                TradesPage.Stop(tester);
        }

        private BotLogRecord CreateInfoRecord(uint no, string message)
        {
            return new BotLogRecord(new TimeKey(_emulteFrom, no), LogSeverities.Info, message, null);
        }

        private void Executor_TradeHistoryUpdated(TradeReportEntity record)
        {
            var symbols = _client.Symbols;
            var accType = _settings.AccType;
            var trRep = TransactionReport.Create(accType, record, symbols.GetOrDefault(record.Symbol));
            TradeHistoryPage.Append(trRep);
            ChartPage.Append(accType, trRep);
        }

        private async Task LoadStats(IActionObserver observer, Backtester tester)
        {
            observer.SetMessage("Loading testing result data...");

            var statProperties = await Task.Factory.StartNew(() => tester.GetStats());
            ResultsPage.ShowReport(statProperties, tester.PluginInfo);
        }

        private async Task LoadChartData(Backtester backtester, IActionObserver observer)
        {
            var mainSymbol = backtester.MainSymbol;
            var timeFrame = backtester.MainTimeframe;
            var count = backtester.GetSymbolHistoryBarCount(mainSymbol);

            timeFrame = AdjustTimeframe(timeFrame, count, out count);

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

        #region Execution control

        public async void StartEmulation()
        {
            //var actionObserver = new ActionViewModel();
            ///yield return OverlayPanel.ShowDialog(null);
            ///

            //var observer = new ActionOverlayViewModel(DoEmulation);

            CloseSetupDialog();

            //ActionOverlay.Value = observer;
            IsRunning.Set();
            ProgressMonitor.Start(DoEmulation);
            await ProgressMonitor.Completion;

            IsRunning.Unset();
            //ActionOverlay.Value = null;
        }

        public void Cancel()
        {
            ProgressMonitor.Cancel();
        }

        public void PauseEmulation()
        {
            _backtester?.Pause();
        }

        public void ResumeEmulation()
        {
            _pauseRequestedProp.Set();
            _backtester?.Resume();
        }

        private void InitExecControl()
        {
            _stateProp = _var.AddProperty<EmulatorStates>();
            _resumeRequestedProp = _var.AddBoolProperty();
            _pauseRequestedProp = _var.AddBoolProperty();

            CanPause = IsVisualizationEnabled.Var & _stateProp.Var == EmulatorStates.Running & !_pauseRequestedProp.Var;
            CanResume = _stateProp.Var == EmulatorStates.Paused & !_pauseRequestedProp.Var;
            CanCanel = ProgressMonitor.CanCancel;
            CanControlSpeed = IsVisualizationEnabled.Var;
            SelectedSpeed = _var.AddIntProperty();
            _var.TriggerOnChange(SelectedSpeed, a =>
            {
                if (IsVisualizing.Value)
                {
                    var delay = SpeedToDelayMap[a.New];
                    _backtester?.SetExecDelay(delay);
                }
            });

            _var.TriggerOnChange(_stateProp, a =>
            {
                _pauseRequestedProp.Clear();
                _resumeRequestedProp.Clear();
            });
        }

        private void OnStartTesting()
        {
            _backtester.StateChanged += _backtester_StateChanged;
            _backtester.Executor.ErrorOccurred += Executor_ErrorOccurred;
        }

        private void OnStopTesting()
        {
            _backtester.StateChanged -= _backtester_StateChanged;
            _backtester.Executor.ErrorOccurred -= Executor_ErrorOccurred;
            _stateProp.Value = EmulatorStates.Stopped;
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
            var fileName = dPlugin.DisplayName + " " + DateTime.Now.ToString("yyyy-dd-M HH-mm-ss") + ".zip";
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
                        await Task.Run(() => SaveTestSetupAsText(pluginSetup, entryStream));

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

        private void SaveTestSetupAsText(PluginSetupModel setup, System.IO.Stream stream)
        {
            var dPlugin = setup.Metadata.Descriptor;

            using (var writer = new System.IO.StreamWriter(stream))
            {
                writer.WriteLine(FeedSetupToText(setup));
                writer.WriteLine(TradeSetupToText());
                writer.WriteLine(PluginSetupToText(setup, false));
            }
        }

        private string FeedSetupToText(PluginSetupModel setup)
        {
            var writer = new StringBuilder();

            writer.AppendLine("Main symbol: " + MainSymbolSetup.AsText());
            writer.AppendLine("Model: based on " + SelectedModel.Value);

            foreach (var addSymbols in AdditionalSymbols)
                writer.AppendLine("+Symbol " + addSymbols.AsText());

            writer.AppendFormat("Period: {0} to {1}", _emulteFrom.ToShortDateString(), _emulateTo.ToShortDateString());

            return writer.ToString();
        }

        private string TradeSetupToText()
        {
            return _settings.ToText(false);
        }

        private string PluginSetupToText(PluginSetupModel setup, bool compact)
        {
            var writer = new StringBuilder();
            var dPlugin = setup.Metadata.Descriptor;

            if (dPlugin.Type == AlgoTypes.Indicator)
                writer.AppendFormat("Indicator: {0} v{1}", dPlugin.DisplayName, dPlugin.Version).AppendLine();
            else if (dPlugin.Type == AlgoTypes.Robot)
                writer.AppendFormat("Trade Bot: {0} v{1}", dPlugin.DisplayName, dPlugin.Version).AppendLine();

            int count = 0;
            foreach (var param in setup.Parameters)
            {
                if (compact && count > 0)
                    writer.Append(", ");
                writer.AppendFormat("{0}={1}", param.DisplayName, param.GetQuotedValue());
                if (!compact)
                    writer.AppendLine();
                count++;
            }

            foreach (var input in setup.Inputs)
            {
                if (compact)
                    writer.Append(' ');
                writer.AppendFormat("{0} = {1}", input.DisplayName, input.ValueAsText);
                if (!compact)
                    writer.AppendLine();
            }

            return writer.ToString();
        }

        #endregion

        private TimeFrames AdjustTimeframe(TimeFrames currentFrame, int currentSize, out int aproxNewSize)
        {
            const int maxGraphSize = 500;

            for (var i = currentFrame; i > TimeFrames.MN; i--)
            {
                aproxNewSize = BarExtentions.GetApproximateTransformSize(currentFrame, currentSize, i);
                if (aproxNewSize <= maxGraphSize)
                    return i;
            }

            aproxNewSize = BarExtentions.GetApproximateTransformSize(currentFrame, currentSize, TimeFrames.MN);
            return TimeFrames.MN;
        }

        private void AddSymbol()
        {
            AdditionalSymbols.Add(CreateSymbolSetupModel(SymbolSetupType.Additional));
            UpdateSymbolsState();
        }

        private BacktesterSymbolSetupViewModel CreateSymbolSetupModel(SymbolSetupType type, Var<SymbolData> symbolSrc = null)
        {
            var smb = new BacktesterSymbolSetupViewModel(type, _catalog.ObservableSymbols, symbolSrc);
            smb.Removed += Smb_Removed;
            smb.OnAdd += AddSymbol;

            smb.IsUpdating.PropertyChanged += IsUpdating_PropertyChanged;
            smb.IsSymbolSelected.PropertyChanged += IsSymbolSelected_PropertyChanged;

            return smb;
        }

        private void Smb_Removed(BacktesterSymbolSetupViewModel smb)
        {
            AdditionalSymbols.Remove(smb);
            smb.IsUpdating.PropertyChanged -= IsUpdating_PropertyChanged;
            smb.IsSymbolSelected.PropertyChanged -= IsSymbolSelected_PropertyChanged;
            smb.Removed -= Smb_Removed;

            UpdateRangeState();
            UpdateSymbolsState();
        }

        private void UpdateRangeState()
        {
            var allSymbols = GetAllSymbols();

            IsUpdatingRange.Value = allSymbols.Any(s => s.IsUpdating.Value);

            var max = allSymbols.Max(s => s.AvailableRange.Value?.Item2);
            var min = allSymbols.Min(s => s.AvailableRange.Value?.Item1);

            bool wasEmpty = DateRange.From == DateTime.MinValue;

            DateRange.UpdateBoundaries(min ?? DateTime.MinValue, max ?? DateTime.MaxValue);

            if (wasEmpty)
                DateRange.ResetSelectedRange();
        }

        private void UpdateSymbolsState()
        {
            _allSymbolsValid.Value = GetAllSymbols().All(s => s.IsSymbolSelected.Value);
        }

        private IEnumerable<BacktesterSymbolSetupViewModel> GetAllSymbols()
        {
            yield return MainSymbolSetup;

            foreach (var smb in AdditionalSymbols)
                yield return smb;
        }

        private void IsUpdating_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRangeState();
        }

        private void IsSymbolSelected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSymbolsState();
        }

        private void CheckDuplicateSymbols()
        {
            var unique = new HashSet<string>();
            unique.Add(MainSymbolSetup.SelectedSymbol.Value.Name);

            foreach (var smb in AdditionalSymbols)
            {
                var name = smb.SelectedSymbol.Value.Name;

                if (unique.Contains(name))
                    throw new Exception("Duplicate symbol: " + name);

                unique.Add(name);
            }
        }

        private void UpdateTradeSummary()
        {
            if (_settings.AccType == AccountTypes.Gross || _settings.AccType == AccountTypes.Net)
                TradeSettingsSummary.Value = string.Format("{0} {1} {2} L={3}, D={4}, {5}ms", _settings.AccType,
                    _settings.InitialBalance, _settings.BalanceCurrency, _settings.Leverage, "Default", _settings.ServerPingMs);
        }

        #region IAlgoSetupMetadata

        IReadOnlyList<ISymbolInfo> IAlgoSetupMetadata.Symbols => _observableSymbolTokens;

        MappingCollection IAlgoSetupMetadata.Mappings => _env.LocalAgent.Mappings;

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => this;

        #endregion

        #region IPluginIdProvider

        string IPluginIdProvider.GeneratePluginId(PluginDescriptor descriptor)
        {
            return descriptor.DisplayName;
        }

        bool IPluginIdProvider.IsValidPluginId(AlgoTypes pluginType, string pluginId)
        {
            return true;
        }

        void IPluginIdProvider.RegisterPluginId(string pluginId)
        {
            return;
        }

        #endregion IPluginIdProvider

        #region IAlgoSetupContext

        TimeFrames IAlgoSetupContext.DefaultTimeFrame => MainTimeFrame.Value;

        ISymbolInfo IAlgoSetupContext.DefaultSymbol => _mainSymbolToken;

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

        #endregion IAlgoSetupContext
    }
}
