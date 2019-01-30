using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using Microsoft.Win32;
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

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, SymbolCatalog catalog, IShell shell)
        {
            DisplayName = "Backtester";

            _env = env ?? throw new ArgumentNullException("env");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _shell = shell ?? throw new ArgumentNullException("shell");
            _client = client;

            _allSymbolsValid = _var.AddBoolProperty();
            _hasDataToSave = _var.AddBoolProperty();
            _isRunning = _var.AddBoolProperty();

            _localWnd = new WindowManager(this);

            ActionOverlay = new Property<ActionOverlayViewModel>();
            AdditionalSymbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel(false);
            IsUpdatingRange = new BoolProperty();
            MainTimeFrame = new Property<TimeFrames>();
            
            TradesPage = new BacktesterTradeGridViewModel();

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

                if (SelectedModel.Value < a.New)
                    SelectedModel.Value = a.New;
            });

            _var.TriggerOnChange(SelectedModel, a =>
            {
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedSymbol, a =>
            {
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            });

            client.Connected += () =>
            {
                GetAllSymbols().Foreach(s => s.Reset());
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            };

            UpdateTradeSummary();
        }

        public Property<ActionOverlayViewModel> ActionOverlay { get; private set; }
        public IObservableList<AlgoPluginViewModel> Plugins { get; private set; }
        public Property<List<TimeFrames>> AvailableModels { get; private set; }
        public Property<TimeFrames> SelectedModel { get; private set; }
        public Property<AlgoPluginViewModel> SelectedPlugin { get; private set; }
        public Property<TimeFrames> MainTimeFrame { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolSetup { get; private set; }
        public Property<string> TradeSettingsSummary { get; private set; }
        public BoolProperty SaveResultsToFile { get; }
        public BoolVar IsPluginSelected { get; }
        public BoolVar IsTradeBotSelected { get; }
        public BoolVar IsRunning => _isRunning.Var;
        //public BoolVar IsStopping { get; }
        public BoolVar CanSetup { get; }
        public BoolVar CanStart { get; }
        //public BoolVar CanStop { get; }
        public BoolProperty IsUpdatingRange { get; private set; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ObservableCollection<BacktesterSymbolSetupViewModel> AdditionalSymbols { get; private set; }
        //public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public BacktesterJournalViewModel JournalPage { get; } = new BacktesterJournalViewModel();
        public BacktesterReportViewModel ResultsPage { get; }
        public BacktesterChartPageViewModel ChartPage { get; }
        public BacktesterTradeGridViewModel TradesPage { get; }
        public PluginConfig PluginConfig { get; private set; }

        public void OpenPluginSetup()
        {
            var setup = PluginConfig == null
                ? new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo())
                : new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo(), PluginConfig);
            _localWnd.OpenMdiWindow("SetupAuxWnd", setup);
            setup.Closed += PluginSetupClosed;
            //_shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", setup);
        }

        public async void OpenTradeSetup()
        {
            var setup = new BacktesterTradeSetupViewModel(_settings, _client.SortedCurrenciesNames);

            _localWnd.OpenMdiWindow("SetupAuxWnd", setup);

            if (await setup.Result)
            {
                _settings = setup.GetSettings();
                UpdateTradeSummary();
            }
        }

        public async void StartEmulation()
        {
            //ProgressMonitor.Start(DoEmulation);
            //var actionObserver = new ActionViewModel();
            ///yield return OverlayPanel.ShowDialog(null);
            ///
            
            var observer = new ActionOverlayViewModel(DoEmulation);

            ActionOverlay.Value = observer;
            IsRunning.Set();
            await observer.Completed;

            IsRunning.Unset();
            ActionOverlay.Value = null;
        }

        //public void Stop()
        //{
        //    ActionOverlay.Cancel();
        //}

        [Conditional("DEBUG")]
        public void PrintCacheData()
        {
            MainSymbolSetup.PrintCacheData(SelectedModel.Value);
        }

        public void SaveResults()
        {
        }

        private void PluginSetupClosed(BacktesterPluginSetupViewModel setup, bool dlgResult)
        {
            if (dlgResult)
                PluginConfig = setup.GetConfig();

            setup.Closed -= PluginSetupClosed;
        }

        private async Task DoEmulation(IActionObserver observer, CancellationToken cToken)
        {
            try
            {
                ChartPage.Clear();
                ResultsPage.Clear();
                JournalPage.Clear();
                TradesPage.Clear(_settings.AccType);
                _hasDataToSave.Clear();

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

            using (var tester = new Backtester(pluginRef, _emulteFrom, _emulateTo))
            {
                pluginSetupModel.Apply(tester);

                foreach (var outputSetup in pluginSetupModel.Outputs)
                {
                    if (outputSetup is ColoredLineOutputSetupModel)
                        tester.InitOutputCollection<double>(outputSetup.Id);
                    else if (outputSetup is MarkerSeriesOutputSetupModel)
                        tester.InitOutputCollection<Marker>(outputSetup.Id);
                }

                Exception execError = null;

                System.Action updateProgressAction = () => observer.SetProgress(tester.CurrentTimePoint?.GetAbsoluteDay() ?? progressMin);

                using (new UiUpdateTimer(updateProgressAction))
                {
                    try
                    {
                        MainSymbolSetup.Apply(tester, _emulteFrom, _emulateTo, SelectedModel.Value);

                        foreach (var symbolSetup in AdditionalSymbols)
                            symbolSetup.Apply(tester, _emulteFrom, _emulateTo);

                        tester.Feed.AddBarBuilder(chartSymbol.Name, chartTimeframe, chartPriceLayer);

                        foreach (var rec in _client.Currencies.Snapshot)
                            tester.Currencies.Add(rec.Key, rec.Value);

                        //foreach (var rec in _client.Symbols.Snapshot)
                        //    tester.Symbols.Add(rec.Key, rec.Value.Descriptor);

                        _settings.Apply(tester);

                        _hasDataToSave.Set();

                        await Task.Run(() => tester.Run(cToken));

                        observer.SetProgress(DateRange.To.GetAbsoluteDay());
                    }
                    catch (Exception ex)
                    {
                        execError = ex;
                    }
                }

                await CollectEvents(tester, pluginSetupModel, observer);
                await LoadTradeHistory(tester, observer);
                await LoadStats(observer, tester);
                await LoadChartData(tester, observer, tester);

                if (SaveResultsToFile.Value)
                    await SaveResults(pluginSetupModel, observer);

                if (execError != null)
                    throw execError; //observer.SetMessage(execError.Message);
                else
                    observer.SetMessage("Done.");
            }
        }

        private async Task CollectEvents(Backtester tester, PluginSetupModel pluginSetup, IActionObserver observer)
        {
            var totalCount = tester.EventsCount;

            observer.StartProgress(0, totalCount);
            observer.SetMessage("Updating journal...");

            JournalPage.SetData(await Task.Run(() =>
            {
                var events = new List<BotLogRecord>(totalCount);

                events.Add(CreateInfoRecord(0, PluginSetupToText(pluginSetup, true)));
                events.Add(CreateInfoRecord(1, FeedSetupToText(pluginSetup)));
                events.Add(CreateInfoRecord(2, _settings.ToText(true)));

                using (var cde = tester.GetEvents())
                {
                    foreach (var record in cde.JoinPages(i => observer.SetProgress(i)))
                        events.Add(record);

                    return events;
                }
            }));
        }

        private BotLogRecord CreateInfoRecord(uint no, string message)
        {
            return new BotLogRecord(new TimeKey(_emulteFrom, no), LogSeverities.Info, message, null);
        }

        private async Task LoadTradeHistory(Backtester tester, IActionObserver observer)
        {
            var totalCount = tester.TradesCount;

            observer.StartProgress(0, totalCount);
            observer.SetMessage("Loading trades...");

            var items = await Task.Run(() =>
            {
                var trades = new List<TransactionReport>(totalCount);

                using (var cde = tester.GetTradeHistory())
                {
                    var symbols = _client.Symbols;
                    var accType = tester.AccountType;

                    foreach (var record in cde.JoinPages(i => observer.SetProgress(i)))
                        trades.Add(TransactionReport.Create(accType, record, symbols.GetOrDefault(record.Symbol)));

                    return trades;
                }
            });


            TradesPage.Fill(items);
        }

        private async Task LoadStats(IActionObserver observer, Backtester tester)
        {
            observer.SetMessage("Loading testing result data...");

            ResultsPage.Stats = await Task.Factory.StartNew(() => tester.GetStats());
        }

        private async Task LoadChartData(Backtester tester, IActionObserver observer, Backtester backtester)
        {
            var timeFrame = backtester.MainTimeframe;
            var count = backtester.BarHistoryCount;

            timeFrame = AdjustTimeframe(timeFrame, count, out count);

            observer.SetMessage("Loading feed chart data ...");
            var feedChartData = await LoadBarSeriesAsync(tester.GetMainSymbolHistory(timeFrame), observer, timeFrame, count);

            observer.SetMessage("Loading equity chart data...");
            var equityChartData = await LoadBarSeriesAsync(tester.GetEquityHistory(timeFrame), observer, timeFrame, count);

            observer.SetMessage("Loading margin chart data...");
            var marginChartData = await LoadBarSeriesAsync(tester.GetMarginHistory(timeFrame), observer, timeFrame, count);

            ChartPage.SetFeedSeries(feedChartData);
            ChartPage.SetEquitySeries(equityChartData);
            ChartPage.SetMarginSeries(marginChartData);
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
                        chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);

                    observer.SetProgress(totalCount);

                    return chartData;
                }
            });
        }

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
                        await TradesPage.SaveAsCsv(entryStream, observer);

                    observer.SetMessage("Saving report...");

                    var summaryEntry = archive.CreateEntry("report.txt", CompressionLevel.Optimal);
                    using (var entryStream = summaryEntry.Open())
                        await Task.Run(() => ResultsPage.SaveAsText(entryStream));

                    var setupEntry = archive.CreateEntry("setup.txt", CompressionLevel.Optimal);
                    using (var entryStream = setupEntry.Open())
                        await Task.Run(() => SaveTestSetupAsText(pluginSetup, entryStream));
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
            const int maxGraphSize = 1000;

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
                TradeSettingsSummary.Value = string.Format("{0} {1} {2} l:{3}", _settings.AccType, _settings.InitialBalance, _settings.BalanceCurrency, _settings.Leverage);
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

        #endregion IPluginIdProvider

        #region IAlgoSetupContext

        TimeFrames IAlgoSetupContext.DefaultTimeFrame => MainTimeFrame.Value;

        ISymbolInfo IAlgoSetupContext.DefaultSymbol => _mainSymbolToken;

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

        #endregion IAlgoSetupContext
    }
}
