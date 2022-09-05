using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class ChartViewModel : Screen, IDropHandler
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ChartModelBase activeChart;
        private readonly IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
        private readonly VarList<ChartModelBase> charts = new VarList<ChartModelBase>();
        private readonly SymbolInfo smb;
        private VarDictionary<string, AlgoBotViewModel> _chartBots = new VarDictionary<string, AlgoBotViewModel>();
        private readonly IVarList<IndicatorModel> _allIndicators;
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

            this.BarChart = new BarChartModel(smb, _algoEnv);

            Chart = BarChart;
            PriceDigits = smb.Digits;
            //this.tickChart = new TickChartModel(smb, _algoEnv);
            this.UiLock = new UiLock();

            _allIndicators = charts.SelectMany(c => c.Indicators);
            _allBots = _chartBots.OrderBy((id, bot) => id);

            Indicators = _allIndicators.Chain().Select(i => new IndicatorViewModel(Chart, i)).Chain().AsObservable();
            Bots = _allBots.Chain().AsObservable();

            //var dataSeries = charts.SelectMany(c => c.DataSeriesCollection);

            // index from VarCollection.CombineChained doesn't work properly when first collection changes size
            //_indicatorOutputs = new VarList<OutputGroupViewModel>();
            //_botOutputs = new VarList<OutputGroupViewModel>();
            //var allOutputs = VarCollection.CombineChained(_indicatorOutputs, _botOutputs);

            FilterChartBots();
            _algoEnv.LocalAgent.BotUpdated += BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated += BotsOnUpdated;

            _allIndicators.Updated += AllIndicators_Updated;
            _allBots.Updated += AllBots_Updated;

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

        public bool HasIndicators { get { return Indicators.Count() > 0; } }
        public bool CanAddBot => false; /*Chart.TimeFrame != Feed.Types.Timeframe.Ticks;*/
        public bool CanAddIndicator => Chart.TimeFrame != Feed.Types.Timeframe.Ticks;


        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override Task TryCloseAsync(bool? dialogResult = null)
        {
            var task = base.TryCloseAsync(dialogResult);

            //Indicators.ForEach(i => _shell.Agent.IdProvider.UnregisterPlugin(i.Model.InstanceId));

            _algoEnv.LocalAgent.BotUpdated -= BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated -= BotsOnUpdated;

            _shell.ToolWndManager.CloseWindowByKey(this);

            BarChart.Dispose();
            //tickChart.Dispose();

            Indicators.Dispose();
            Bots.Dispose();

            charts.Clear();
            _chartBots.Clear();
            _allIndicators.Updated -= AllIndicators_Updated;
            _allBots.Updated -= AllBots_Updated;
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
                    Config = Algo.Core.Config.PluginConfig.FromDomain(i.Model.Config),
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
            _chartHost.AddIndicator(entry.Config.ToDomain());
        }

        #region Algo

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
                    _chartHost.AddIndicator(setupModel.GetConfig());
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


        private void Indicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            NotifyOfPropertyChange(nameof(HasIndicators));
        }

        private void AllIndicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            //var allOutputs = ChartControl.OutputGroups;

            //if (args.Action == DLinqAction.Insert)
            //{
            //    allOutputs.Add(new OutputGroupViewModel(args.NewItem, ChartWindowId, Chart, smb, IsCrosshairEnabled));
            //}
            //else if (args.Action == DLinqAction.Replace)
            //{
            //    var index = allOutputs.IndexOf(allOutputs.Values.First(o => o.Model == args.OldItem));
            //    allOutputs[index].Dispose();
            //    allOutputs[index] = new OutputGroupViewModel(args.NewItem, ChartWindowId, Chart, smb, IsCrosshairEnabled);
            //}
            //else if (args.Action == DLinqAction.Remove)
            //{
            //    var output = allOutputs.Values.First(o => o.Model == args.OldItem);
            //    output.Dispose();
            //    allOutputs.Remove(output);
            //}
            NotifyOfPropertyChange(nameof(HasIndicators));
        }

        private void AllBots_Updated(ListUpdateArgs<AlgoBotViewModel> args)
        {
            //var allOutputs = ChartControl.OutputGroups;

            //if (args.Action == DLinqAction.Insert)
            //{
            //    allOutputs.Add(new OutputGroupViewModel((TradeBotModel)args.NewItem.Model, ChartWindowId, Chart, smb, IsCrosshairEnabled));
            //}
            //else if (args.Action == DLinqAction.Replace)
            //{
            //    var index = allOutputs.IndexOf(allOutputs.Values.First(o => o.Model == args.OldItem.Model));
            //    allOutputs[index].Dispose();
            //    allOutputs[index] = new OutputGroupViewModel((TradeBotModel)args.NewItem.Model, ChartWindowId, Chart, smb, IsCrosshairEnabled);
            //}
            //else if (args.Action == DLinqAction.Remove)
            //{
            //    var output = allOutputs.Values.First(o => o.Model == args.OldItem.Model);
            //    output.Dispose();
            //    allOutputs.Remove(output);
            //}
        }

        private void InitChart()
        {
            charts.Clear();
            charts.Add(Chart);

            Chart_TimeframeChanged();

            Chart.TimeframeChanged += Chart_TimeframeChanged;
            //Chart.TimeAxis.PropertyChanged += TimeAxis_PropertyChanged;
            Chart.ParamsLocked += Chart_ParamsLocked;
            Chart.ParamsUnlocked += Chart_ParamsUnlocked;
            Chart.Indicators.Updated += Indicators_Updated;
        }

        private void DeinitChart()
        {
            //Chart.TimeAxis.PropertyChanged -= TimeAxis_PropertyChanged;
            Chart.TimeframeChanged -= Chart_TimeframeChanged;
            Chart.ParamsLocked -= Chart_ParamsLocked;
            Chart.ParamsUnlocked -= Chart_ParamsUnlocked;
            Chart.Indicators.Updated -= Indicators_Updated;
        }

        public bool CanDrop(object o)
        {
            var plugin = o as AlgoPluginViewModel;
            if (plugin != null && plugin.Agent.Name == _algoEnv.LocalAgentVM.Name)
            {
                if (plugin.IsIndicator && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                    return true;
                if (plugin.IsTradeBot && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                    return true;
            }
            return false;
        }

        private void FilterChartBots()
        {
            _chartBots.Clear();
            _algoEnv.LocalAgentVM.Bots.Snapshot.Where(IsChartBot).ForEach(AddChartBot);
        }

        private bool BotBelongsToChart(PluginModel bot)
        {
            return bot.Descriptor != null && bot.Config != null && Chart != null
                && bot.Descriptor.SetupMainSymbol
                && bot.Config.MainSymbol.Name == Symbol
                && bot.Config.Timeframe == Chart.TimeFrame;
        }

        private bool IsChartBot(AlgoBotViewModel botVM)
        {
            return botVM.Model is TradeBotModel && BotBelongsToChart((TradeBotModel)botVM.Model);
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
            if (_chartBots.ContainsKey(bot.InstanceId) && !BotBelongsToChart((TradeBotModel)bot))
            {
                RemoveChartBot(bot.InstanceId);
            }
            else if (!_chartBots.ContainsKey(bot.InstanceId) && bot is TradeBotModel && BotBelongsToChart((TradeBotModel)bot))
            {
                var botVM = _algoEnv.LocalAgentVM.BotList.FirstOrDefault(b => b.InstanceId == bot.InstanceId);
                AddChartBot(botVM);
            }
        }
    }
}