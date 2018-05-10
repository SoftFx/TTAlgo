using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
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

namespace TickTrader.BotTerminal
{
    internal class BacktesterViewModel : Screen, IWindowModel, IAlgoSetupFactory
    {
        private AlgoEnvironment _env;
        private IShell _shell;
        private SymbolCatalog _catalog;
        private Property<List<BotLogRecord>> _journalContent = new Property<List<BotLogRecord>>();

        public BacktesterViewModel(AlgoEnvironment env, TraderClientModel client, SymbolCatalog catalog, IShell shell)
        {
            _env = env ?? throw new ArgumentNullException("env");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _shell = shell ?? throw new ArgumentNullException("shell");

            ProgressMonitor = new ActionViewModel();
            Symbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel();
            IsUpdatingRange = new BoolProperty();
            MainTimeFrame = new Property<TimeFrames>();

            MainTimeFrame.Value = TimeFrames.M1;

            //_availableSymbols = env.Symbols;

            AddSymbol(SymbolSetupType.MainSymbol);
            AddSymbol(SymbolSetupType.MainFeed, Symbols[0].SelectedSymbol.Var);

            SelectedPlugin = new Property<AlgoItemViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsStarted = ProgressMonitor.IsRunning;
            CanStart = !IsStarted & client.IsConnected;
            CanSetup = !IsStarted & client.IsConnected;

            Plugins = env.Repo.AllPlugins
                .Where((k, p) => !string.IsNullOrEmpty(k.FileName))
                .Select((k, p) => new AlgoItemViewModel(p))
                .OrderBy((k, p) => p.Name)
                .AsObservable();

            env.Repo.AllPlugins.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove && a.OldItem.Key == SelectedPlugin.Value?.PluginItem.Key)
                    SelectedPlugin.Value = null;
            };
        }

        public ActionViewModel ProgressMonitor { get; private set; }
        public IObservableList<AlgoItemViewModel> Plugins { get; private set; }
        public Property<AlgoItemViewModel> SelectedPlugin { get; private set; }
        public Property<TimeFrames> MainTimeFrame { get; private set; }
        public BoolVar IsPluginSelected { get; }
        public BoolVar IsStarted { get; }
        public BoolVar CanSetup { get; }
        public BoolVar CanStart { get; }
        public BoolProperty IsUpdatingRange { get; private set; }
        public DateRangeSelectionViewModel DateRange { get; }
        public ObservableCollection<BacktesterSymbolSetupViewModel> Symbols { get; private set; }
        public IEnumerable<TimeFrames> AvailableTimeFrames => EnumHelper.AllValues<TimeFrames>();
        public Var<List<BotLogRecord>> JournalRecords => _journalContent.Var;

        public void OpenPluginSetup()
        {
            var setup = new PluginSetupViewModel(_env, SelectedPlugin.Value.PluginItem, this);
            _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", setup);
        }

        public void Start()
        {
            ProgressMonitor.Start(DoEmulation);
        }

        private async Task DoEmulation(IActionObserver observer, CancellationToken cToken)
        {
            foreach (var symbolSetup in Symbols)
                await symbolSetup.PrecacheData(observer, cToken, DateRange.From, DateRange.To);

            observer.StartProgress(DateRange.From.GetAbsoluteDay(), DateRange.To.GetAbsoluteDay());
            observer.SetMessage("Emulating...");

            var pluginRef = SelectedPlugin.Value.PluginItem.Ref;

            using (var tester = new Backtester(pluginRef))
            {
                tester.EmulationPeriodStart = DateRange.From;
                tester.EmulationPeriodEnd = DateRange.To;

                var updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromMilliseconds(50);
                updateTimer.Tick += (s, a) =>
                {
                    var point = tester.CurrentTimePoint;
                    if (point != null)
                        observer.SetProgress(tester.CurrentTimePoint.Value.GetAbsoluteDay());
                };
                updateTimer.Start();

                try
                {
                    foreach (var symbolSetup in Symbols)
                        symbolSetup.Apply(tester, DateRange.From, DateRange.To);

                    await Task.Run(() => tester.Run(cToken));

                    observer.SetProgress(DateRange.To.GetAbsoluteDay());
                }
                finally
                {
                    updateTimer.Stop();
                }

                _journalContent.Value =  await CollectEvents(tester, observer);

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

            Symbols.Add(smb);
        }

        private void Smb_Removed(BacktesterSymbolSetupViewModel smb)
        {
            Symbols.Remove(smb);
            smb.IsUpdating.PropertyChanged -= IsUpdating_PropertyChanged;
            smb.Removed -= Smb_Removed;

            UpdateRangeState();
        }

        private void UpdateRangeState()
        {
            IsUpdatingRange.Value = Symbols.Any(s => s.IsUpdating.Value);
            if (!IsUpdatingRange.Value)
            {
                var max = Symbols.Max(s => s.AvailableRange.Value?.Item2);
                var min = Symbols.Min(s => s.AvailableRange.Value?.Item1);
                DateRange.UpdateBoundaries(min ?? DateTime.MinValue, max ?? DateTime.MaxValue);
            }
        }

        private void IsUpdating_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRangeState();
        }

        PluginSetup IAlgoSetupFactory.CreateSetup(AlgoPluginRef catalogItem)
        {
            return new BarBasedPluginSetup(catalogItem, "", Algo.Api.BarPriceType.Ask, _env);
        }
    }
}
