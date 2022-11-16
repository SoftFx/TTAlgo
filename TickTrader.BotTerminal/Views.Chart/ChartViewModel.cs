using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.BotTerminal.Controls.Chart;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class ChartViewModel : Screen, IDropHandler
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ChartModelBase activeChart;
        private readonly IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
        private readonly SymbolInfo smb;
        private VarDictionary<string, AlgoBotViewModel> _chartBots = new VarDictionary<string, AlgoBotViewModel>();
        private readonly IVarList<PluginOutputModel> _allIndicators;
        private readonly IVarList<AlgoBotViewModel> _allBots;
        private readonly ChartHostProxy _chartHost;

        public BarChartModel BarChart { get; }

        public ChartViewModel(string chartId, string symbol, Feed.Types.Timeframe period, AlgoEnvironment algoEnv)
        {
            Symbol = symbol;
            DisplayName = symbol;
            _algoEnv = algoEnv;

            ChartWindowId = chartId;

            _shell = _algoEnv.Shell;
            smb = _algoEnv.ClientModel.Symbols.GetOrDefault(symbol);

            _chartHost = algoEnv.LocalAgent.IndicatorHost.CreateChart(symbol, period, Feed.Types.MarketSide.Bid).Result;
            IndicatorObserver = new DynamicIndicatorObserver(_chartHost, smb.Digits);

            this.BarChart = new BarChartModel(smb, _algoEnv);

            Chart = BarChart;
            PriceDigits = smb.Digits;
            this.UiLock = new UiLock();

            _allIndicators = _chartHost.Indicators.TransformToList();
            _allBots = _chartBots.OrderBy((id, bot) => id);

            Indicators = _allIndicators.Chain().Select(i => new IndicatorViewModel(_chartHost, i)).UseSyncContext().Chain().AsObservable();
            Bots = _allBots.Chain().AsObservable();

            FilterChartBots();
            _algoEnv.LocalAgent.BotUpdated += BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated += BotsOnUpdated;

            Indicators.CollectionChanged += Indicators_CollectionChanged;

            SelectedTimeframe = period;

            CloseCommand = new GenericCommand(o => TryCloseAsync());
        }

        public string ChartWindowId { get; }

        public Feed.Types.Timeframe[] AvailablePeriods { get; } = TimeFrameModel.BarTimeFrames;

        public ChartModelBase Chart
        {
            get { return activeChart; }
            private set
            {
                if (activeChart != null)
                    DeinitChart();
                activeChart = value;
                NotifyOfPropertyChange();
                InitChart();
            }
        }

        private Feed.Types.Timeframe _selTime;

        public Feed.Types.Timeframe SelectedTimeframe
        {
            get => _selTime;
            set
            {
                _selTime = value;
                DisplayName = $"{Symbol}, {value}";

                ActivateBarChart(value);
                NotifyOfPropertyChange(nameof(SelectedTimeframe));
            }
        }

        private bool _enCross;

        public bool EnableCrosshair
        {
            get => _enCross;

            set
            {
                _enCross = value;

                NotifyOfPropertyChange(nameof(EnableCrosshair));
            }
        }

        private int _digits;

        public int PriceDigits
        {
            get => _digits;

            set
            {
                _digits = value;

                NotifyOfPropertyChange(nameof(PriceDigits));
            }
        }

        public IObservableList<IndicatorViewModel> Indicators { get; private set; }
        public IObservableList<AlgoBotViewModel> Bots { get; private set; }
        public GenericCommand CloseCommand { get; private set; }

        public DynamicIndicatorObserver IndicatorObserver { get; }

        public bool HasIndicators { get { return (Indicators as ICollection)?.Count > 0; } }
        public bool CanAddBot => false; /*Chart.TimeFrame != Feed.Types.Timeframe.Ticks;*/
        public bool CanAddIndicator => Chart.TimeFrame != Feed.Types.Timeframe.Ticks;


        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            var task = Task.WhenAll(base.TryCloseAsync(dialogResult), _chartHost.DisposeAsync().AsTask());

            //Indicators.ForEach(i => _shell.Agent.IdProvider.UnregisterPlugin(i.Model.InstanceId));

            _algoEnv.LocalAgent.BotUpdated -= BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated -= BotsOnUpdated;

            Indicators.CollectionChanged -= Indicators_CollectionChanged;

            _shell.ToolWndManager.CloseWindowByKey(this);

            BarChart.Dispose();
            //tickChart.Dispose();
            IndicatorObserver.Dispose();

            Indicators.Dispose();
            Bots.Dispose();

            _chartBots.Clear();
            _allIndicators.Dispose();
            _allBots.Dispose();

            return task;
        }

        public ChartStorageEntry GetSnapshot()
        {
            return new ChartStorageEntry
            {
                Id = ChartWindowId,
                Symbol = Symbol,
                SelectedPeriod = ChartStorageEntry.ConvertPeriod(SelectedTimeframe),
                SelectedChartType = Chart.SelectedChartType,
                CrosshairEnabled = EnableCrosshair,
                Indicators = Indicators.Select(i => new IndicatorStorageEntry
                {
                    Config = Algo.Core.Config.PluginConfig.FromDomain(i.Model.Info.Config),
                }).ToList(),
            };
        }

        public void RestoreFromSnapshot(ChartStorageEntry snapshot)
        {
            if (Symbol != snapshot.Symbol)
            {
                return;
            }

            Chart.SelectedChartType = snapshot.SelectedChartType;
            EnableCrosshair = snapshot.CrosshairEnabled;
            snapshot.Indicators?.ForEach(i => RestoreIndicator(i));
        }

        private void RestoreIndicator(IndicatorStorageEntry entry)
        {
            if (entry.Config == null)
            {
                logger.Error("Indicator not configured!");
            }
            if (entry.Config.Key == null)
            {
                logger.Error("Indicator key missing!");
            }

            //Chart.AddIndicator(entry.Config.ToDomain());
            _ = AddIndicator(entry.Config.ToDomain());
        }

        #region Algo

        public void DumpChartData()
        {
            try
            {
                var dirName = $"{Symbol}.{SelectedTimeframe} {DateTime.UtcNow:yyyy-MM-dd HH-mm-ss.fff}";
                var dirPath = Path.Combine(EnvService.Instance.AppFolder, "ChartDumps", dirName);
                PathHelper.EnsureDirectoryCreated(dirPath);

                var doubleFormat = "F" + smb.Digits.ToString();
                var path = Path.Combine(dirPath, "_bars.csv");
                using (var file = File.Open(path, FileMode.Create))
                using (var writer = new StreamWriter(file, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("Time,Open,High,Low,Close"); // header
                    foreach (var bar in BarChart.BarVector)
                    {
                        writer.Write(bar.Date.ToUniversalTime().ToString("yyyy-MM-dd HH-mm-ss"));
                        writer.Write(",");
                        writer.Write(bar.Open?.ToString(doubleFormat, CultureInfo.InvariantCulture));
                        writer.Write(",");
                        writer.Write(bar.High?.ToString(doubleFormat, CultureInfo.InvariantCulture));
                        writer.Write(",");
                        writer.Write(bar.Low?.ToString(doubleFormat, CultureInfo.InvariantCulture));
                        writer.Write(",");
                        writer.Write(bar.Close?.ToString(doubleFormat, CultureInfo.InvariantCulture));

                        writer.WriteLine();
                    }
                }

                IndicatorObserver.DumpPoints(dirPath);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to dump points");
            }
        }

        public void OpenPlugin(object descriptorObj)
        {
            OpenAlgoSetup((AlgoPluginViewModel)descriptorObj);
        }

        private void OpenAlgoSetup(AlgoPluginViewModel item)
        {
            try
            {
                if (item.Descriptor.IsTradeBot)
                {
                    _algoEnv.LocalAgentVM.OpenBotSetup(item.PluginInfo, Chart.GetSetupContextInfo());
                    return;
                }

                var model = new LocalPluginSetupViewModel(_shell.Agent, item.Key, Metadata.Types.PluginType.Indicator, Chart.GetSetupContextInfo());
                if (!model.Setup.CanBeSkipped)
                    _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
                else
                    AttachPlugin(model);

                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void AttachPlugin(LocalPluginSetupViewModel setupModel)
        {
            if (setupModel == null)
            {
                return;
            }

            switch (setupModel.Setup.Descriptor.Type)
            {
                case Metadata.Types.PluginType.Indicator:
                    //Chart.AddIndicator(setupModel.GetConfig());
                    _ = AddIndicator(setupModel.GetConfig());
                    break;
                default:
                    throw new Exception($"Unknown plugin type '{setupModel.Setup.Descriptor.Type}'");
            }
        }

        void AlgoSetupClosed(LocalPluginSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
                AttachPlugin(setupModel);
        }

        #endregion

        public void Drop(object o)
        {
            var plugin = o as AlgoPluginViewModel;
            if (plugin != null)
            {
                OpenAlgoSetup(plugin);
            }
        }

        private void ActivateBarChart(Feed.Types.Timeframe timeFrame)
        {
            //BarChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = BarChart;
            BarChart.Activate(timeFrame);
            _ = _chartHost.ChangeTimeframe(timeFrame);
            FilterChartBots();
        }

        private void ActivateTickChart()
        {
            //this.Chart = tickChart;
            //tickChart.Activate();
            //barChart.Deactivate();
            FilterChartBots();
        }

        private void Chart_ParamsLocked()
        {
            _shell.ConnectionLock.Lock();
            UiLock.Lock();
        }

        private void Chart_ParamsUnlocked()
        {
            _shell.ConnectionLock.Release();
            UiLock.Release();
        }

        private void Chart_TimeframeChanged()
        {
            NotifyOfPropertyChange(nameof(CanAddBot));
            NotifyOfPropertyChange(nameof(CanAddIndicator));
        }


        private void Indicators_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(nameof(HasIndicators));
        }

        private void InitChart()
        {
            Chart_TimeframeChanged();

            Chart.TimeframeChanged += Chart_TimeframeChanged;
            Chart.ParamsLocked += Chart_ParamsLocked;
            Chart.ParamsUnlocked += Chart_ParamsUnlocked;
        }

        private void DeinitChart()
        {
            Chart.TimeframeChanged -= Chart_TimeframeChanged;
            Chart.ParamsLocked -= Chart_ParamsLocked;
            Chart.ParamsUnlocked -= Chart_ParamsUnlocked;
        }

        public bool CanDrop(object o)
        {
            var plugin = o as AlgoPluginViewModel;
            if (plugin != null && plugin.Agent.Name == _algoEnv.LocalAgentVM.Name)
            {
                if (plugin.IsIndicator && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                    return true;
                //if (plugin.IsTradeBot && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                //    return true;
            }
            return false;
        }

        private void FilterChartBots()
        {
            _chartBots.Clear();
            _algoEnv.LocalAgentVM.Bots.Snapshot.Where(IsChartBot).ForEach(AddChartBot);
        }

        private bool BotBelongsToChart(LocalTradeBot bot)
        {
            return bot.Descriptor != null && bot.Config != null && Chart != null
                && bot.Descriptor.SetupMainSymbol
                && bot.Config.MainSymbol.Name == Symbol
                && bot.Config.Timeframe == Chart.TimeFrame;
        }

        private bool IsChartBot(AlgoBotViewModel botVM)
        {
            return botVM.Model is LocalTradeBot localBot && BotBelongsToChart(localBot);
        }

        private void AddChartBot(AlgoBotViewModel botVM)
        {
            if (botVM != null)
                Execute.OnUIThread(() => _chartBots.Add(botVM.InstanceId, botVM));
        }

        private void RemoveChartBot(string botId)
        {
            if (_chartBots.ContainsKey(botId))
            {
                Execute.OnUIThread(() => _chartBots.Remove(botId));
            }
        }

        private void BotsOnUpdated(ListUpdateArgs<AlgoBotViewModel> args)
        {
            if (args.Action == DLinqAction.Insert && IsChartBot(args.NewItem))
            {
                AddChartBot(args.NewItem);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                RemoveChartBot(args.OldItem.InstanceId);
            }
        }

        private void BotOnUpdated(ITradeBot bot)
        {
            if (bot is LocalTradeBot localBot)
            {
                var belongsToChart = BotBelongsToChart(localBot);
                if (_chartBots.ContainsKey(bot.InstanceId) && !belongsToChart)
                {
                    RemoveChartBot(bot.InstanceId);
                }
                else if (!_chartBots.ContainsKey(bot.InstanceId) && belongsToChart)
                {
                    var botVM = _algoEnv.LocalAgentVM.BotList.FirstOrDefault(b => b.InstanceId == bot.InstanceId);
                    AddChartBot(botVM);
                }
            }
        }

        private async Task AddIndicator(PluginConfig config)
        {
            await _chartHost.AddIndicator(config);
            _algoEnv.LocalAgent.IdProvider.RegisterPluginId(config.InstanceId);
        }
    }
}