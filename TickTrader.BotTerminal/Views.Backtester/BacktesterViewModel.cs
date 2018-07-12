using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Screen, IWindowModel, IAlgoGuiMetadata
    {
        private AlgoEnvironment _env;
        private IShell _shell;
        private SymbolCatalog _catalog;
        private Property<List<BotLogRecord>> _journalContent = new Property<List<BotLogRecord>>();
        private SymbolToken _mainSymbolToken;
        private IVarList<ISymbolInfo> _symbolTokens;
        private IReadOnlyList<ISymbolInfo> _observableSymbolTokens;
        private VarContext _var = new VarContext();
        private TraderClientModel _client;
        private WindowManager _localWnd;
        private double _initialBalance = 10000;
        private string _balanceCurrency = "USD";
        private int _leverage = 100;
        private AccountTypes _accType;
        private int _emulatedPing = 200;

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, SymbolCatalog catalog, IShell shell)
        {
            DisplayName = "Strategy/Indicator Tester";

            _env = env ?? throw new ArgumentNullException("env");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _shell = shell ?? throw new ArgumentNullException("shell");
            _client = client;

            _localWnd = new WindowManager(this);

            ProgressMonitor = new ActionViewModel();
            FeedSources = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel();
            IsUpdatingRange = new BoolProperty();
            MainTimeFrame = new Property<TimeFrames>();

            MainTimeFrame.Value = TimeFrames.M1;

            //_availableSymbols = env.Symbols;

            AddSymbol(SymbolSetupType.MainSymbol);
            AddSymbol(SymbolSetupType.MainFeed, FeedSources[0].SelectedSymbol.Var);

            SelectedPlugin = new Property<AlgoItemViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsTradeBotSelected = SelectedPlugin.Var.Check(p => p != null && p.PluginItem.Descriptor.AlgoLogicType == AlgoTypes.Robot);
            IsRunning = ProgressMonitor.IsRunning;
            IsStopping = ProgressMonitor.IsCancelling;
            CanStart = !IsRunning & client.IsConnected;
            CanSetup = !IsRunning & client.IsConnected;
            CanStop = ProgressMonitor.CanCancel;

            Plugins = env.Repo.AllPlugins
                .Where((k, p) => !string.IsNullOrEmpty(k.FileName))
                .Select((k, p) => new AlgoItemViewModel(p))
                .OrderBy((k, p) => p.Name)
                .AsObservable();

            _mainSymbolToken = new SymbolToken("[main symbol]", null);
            var predefinedSymbolTokens = new VarList<ISymbolInfo>(new ISymbolInfo[] { _mainSymbolToken });
            var existingSymbolTokens = _catalog.AllSymbols.Select(s => (ISymbolInfo)new SymbolToken(s.Name, s.Description));
            _symbolTokens = VarCollection.Combine<ISymbolInfo>(predefinedSymbolTokens, existingSymbolTokens);
            _observableSymbolTokens = _symbolTokens.AsObservable();

            env.Repo.AllPlugins.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove && a.OldItem.Key == SelectedPlugin.Value?.PluginItem.Key)
                    SelectedPlugin.Value = null;
            };

            ChartPage = new BacktesterChartPageViewModel();

            _var.TriggerOnChange(SelectedPlugin.Var, a =>
            {
                if (a.New != null)
                    PluginSetupModel = new BarBasedPluginSetup(a.New.PluginItem.Ref, _mainSymbolToken, Algo.Api.BarPriceType.Bid, this);
            });
        }

        public ActionViewModel ProgressMonitor { get; private set; }
        public IObservableList<AlgoItemViewModel> Plugins { get; private set; }
        public Property<AlgoItemViewModel> SelectedPlugin { get; private set; }
        public Property<TimeFrames> MainTimeFrame { get; private set; }
        public BoolVar IsPluginSelected { get; }
        public BoolVar IsTradeBotSelected { get; }
        public BoolVar IsRunning { get; }
        public BoolVar IsStopping { get; }
        public BoolVar CanSetup { get; }
        public BoolVar CanStart { get; }
        public BoolVar CanStop { get; }
        public BoolProperty IsUpdatingRange { get; private set; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ObservableCollection<BacktesterSymbolSetupViewModel> FeedSources { get; private set; }
        //public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public Var<List<BotLogRecord>> JournalRecords => _journalContent.Var;
        public BacktesterChartPageViewModel ChartPage { get; }
        public PluginSetup PluginSetupModel { get; private set; }

        public void OpenPluginSetup()
        {
            var setup = new PluginSetupViewModel(PluginSetupModel, _env.Repo);
            _localWnd.OpenMdiWindow("SetupAuxWnd", setup);
            //_shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", setup);
        }

        public async void OpenTradeSetup()
        {
            var setup = new BacktesterTradeSetupViewModel();
            setup.SelectedAccType.Value = _accType;
            setup.InitialBalance.Value = _initialBalance;
            setup.Leverage.Value = _leverage;
            setup.BalanceCurrency.Value = _balanceCurrency;
            setup.EmulatedServerPing.Value = _emulatedPing;

            _localWnd.OpenMdiWindow("SetupAuxWnd", setup);

            if (await setup.Result)
            {
                _emulatedPing = setup.EmulatedServerPing.Value;
                _accType = setup.SelectedAccType.Value;

                if (_accType == AccountTypes.Cash || _accType == AccountTypes.Gross)
                {
                    _initialBalance = setup.InitialBalance.Value;
                    _leverage = setup.Leverage.Value;
                    _balanceCurrency = setup.BalanceCurrency.Value;
                }
                else if (_accType == AccountTypes.Cash)
                {
                }
            }
        }

        public void Start()
        {
            ProgressMonitor.Start(DoEmulation);
        }

        public void Stop()
        {
            ProgressMonitor.Cancel();
        }

        private async Task DoEmulation(IActionObserver observer, CancellationToken cToken)
        {
            foreach (var symbolSetup in FeedSources)
                await symbolSetup.PrecacheData(observer, cToken, DateRange.From, DateRange.To);

            var chartSymbol = FeedSources[0].SelectedSymbol.Value;
            var chartTimeframe = FeedSources[0].SelectedTimeframe.Value;
            var chartPriceLayer = BarPriceType.Bid;

            _mainSymbolToken.Id = chartSymbol.Key;

            observer.StartProgress(DateRange.From.GetAbsoluteDay(), DateRange.To.GetAbsoluteDay());
            observer.SetMessage("Emulating...");

            var pluginRef = SelectedPlugin.Value.PluginItem.Ref;

            using (var tester = new Backtester(pluginRef, DateRange.From, DateRange.To))
            {
                PluginSetupModel.Apply(tester);

                foreach (var outputSetup in PluginSetupModel.Outputs)
                {
                    if (outputSetup is ColoredLineOutputSetup)
                        tester.InitOutputCollection<double>(outputSetup.Id);
                    else if (outputSetup is MarkerSeriesOutputSetup)
                        tester.InitOutputCollection<Marker>(outputSetup.Id);
                }

                var updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromMilliseconds(50);
                updateTimer.Tick += (s, a) =>
                {
                    var point = tester.CurrentTimePoint;
                    if (point != null)
                        observer.SetProgress(tester.CurrentTimePoint.Value.GetAbsoluteDay());
                };
                updateTimer.Start();

                Exception execError = null;

                try
                {
                    foreach (var symbolSetup in FeedSources)
                        symbolSetup.Apply(tester, DateRange.From, DateRange.To);

                    tester.Feed.AddBarBuilder(chartSymbol.Name, chartTimeframe, chartPriceLayer);

                    foreach (var rec in _client.Currencies.Snapshot)
                        tester.Currencies.Add(rec.Key, rec.Value);

                    //foreach (var rec in _client.Symbols.Snapshot)
                    //    tester.Symbols.Add(rec.Key, rec.Value.Descriptor);

                    tester.AccountType = _accType;
                    tester.BalanceCurrency = _balanceCurrency;
                    tester.Leverage = _leverage;
                    tester.InitialBalance = _initialBalance;

                    await Task.Run(() => tester.Run(cToken));

                    observer.SetProgress(DateRange.To.GetAbsoluteDay());
                }
                catch (Exception ex)
                {
                    execError = ex;
                }
                finally
                {
                    updateTimer.Stop();
                }

                _journalContent.Value = await CollectEvents(tester, observer);

                await LoadChartData(tester, observer, tester.Feed.GetBarSeriesData(chartSymbol.Name, chartTimeframe, chartPriceLayer));

                if (execError != null)
                    observer.SetMessage(execError.Message);
                else
                    observer.SetMessage("Done.");
            }
        }

        private Task<List<BotLogRecord>> CollectEvents(Backtester tester, IActionObserver observer)
        {
            var totalCount = tester.EventsCount;

            observer.StartProgress(0, totalCount);
            observer.SetMessage("Updating journal...");

            return Task.Run(() =>
            {
                var events = new List<BotLogRecord>(totalCount);
                var e = tester.GetEvents();

                for (int i = 0; i < totalCount;)
                {
                    var page = e.GetNextPage();
                    if (page == null)
                        break;
                    events.AddRange(page);
                    i += page.Count;
                    observer.SetProgress(i);
                }

                return events;
            });
        }

        private async Task LoadChartData(Backtester tester, IActionObserver observer, IReadOnlyList<BarEntity> data)
        {
            observer.SetMessage("Loading chart data...");

            var series = await Task.Run(() =>
            {
                var chartData = new OhlcDataSeries<DateTime, double>();

                foreach(var bar in data)
                    chartData.Append(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close);

                return chartData;
            });

            ChartPage.AddMainSeries(series);
        }

        private void AddSymbol()
        {
            AddSymbol(SymbolSetupType.AdditionalFeed);
        }

        private void AddSymbol(SymbolSetupType type, Var<SymbolData> symbolSrc = null)
        {
            var smb = new BacktesterSymbolSetupViewModel(type, _catalog.ObservableSymbols, symbolSrc);
            smb.Removed += Smb_Removed;
            smb.OnAdd += AddSymbol;

            smb.IsUpdating.PropertyChanged += IsUpdating_PropertyChanged;

            FeedSources.Add(smb);
        }

        private void Smb_Removed(BacktesterSymbolSetupViewModel smb)
        {
            FeedSources.Remove(smb);
            smb.IsUpdating.PropertyChanged -= IsUpdating_PropertyChanged;
            smb.Removed -= Smb_Removed;

            UpdateRangeState();
        }

        private void UpdateRangeState()
        {
            IsUpdatingRange.Value = FeedSources.Any(s => s.IsUpdating.Value);
            if (!IsUpdatingRange.Value)
            {
                var max = FeedSources.Max(s => s.AvailableRange.Value?.Item2);
                var min = FeedSources.Min(s => s.AvailableRange.Value?.Item1);
                DateRange.UpdateBoundaries(min ?? DateTime.MinValue, max ?? DateTime.MaxValue);
            }
        }

        private void IsUpdating_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRangeState();
        }

        //PluginSetup IAlgoSetupFactory.CreateSetup(AlgoPluginRef catalogItem)
        //{
        //    return new BarBasedPluginSetup(catalogItem, _mainSymbolToken, Algo.Api.BarPriceType.Bid, _env);
        //}

        #region IAlgoGuiMetadata

        IReadOnlyList<ISymbolInfo> IAlgoGuiMetadata.Symbols => _observableSymbolTokens;
        ExtCollection IAlgoGuiMetadata.Extentions => _env.Extentions;

        #endregion

        public class SymbolToken : ISymbolInfo
        {
            public SymbolToken(string name, string id)
            {
                Name = name;
                Id = id;
            }

            public string Name { get; }
            public string Id { get; set; }
        }
    }
}
