using Caliburn.Micro;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SciChart.Charting.Services;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using NLog;
using Machinarium.Qnil;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;
using System.Windows.Input;
using TickTrader.Algo.Common.Model;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Controls;
using TickTrader.Algo.Common.Info;
using Machinarium.Var;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    public enum ChartPeriods { MN1, W1, D1, H4, H1, M30, M15, M5, M1, S10, S1, Ticks };

    internal class ChartViewModel : Screen, IDropHandler
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ChartModelBase activeChart;
        private readonly BarChartModel barChart;
        private readonly TickChartModel tickChart;
        private readonly IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
        private readonly VarList<ChartModelBase> charts = new VarList<ChartModelBase>();
        private readonly SymbolInfo smb;
        private VarDictionary<string, AlgoBotViewModel> _chartBots = new VarDictionary<string, AlgoBotViewModel>();

        public ChartViewModel(string chartId, string symbol, ChartPeriods period, AlgoEnvironment algoEnv)
        {
            Symbol = symbol;
            DisplayName = symbol;
            _algoEnv = algoEnv;

            ChartWindowId = chartId;

            _shell = _algoEnv.Shell;
            smb = _algoEnv.LocalAgent.ClientModel.Symbols.GetOrDefault(symbol);

            this.barChart = new BarChartModel(smb, _algoEnv);
            this.tickChart = new TickChartModel(smb, _algoEnv);
            this.UiLock = new UiLock();

            var allIndicators = charts.SelectMany(c => c.Indicators);
            var allBots = _chartBots.OrderBy((id, bot) => id);

            Indicators = allIndicators.Select(i => new IndicatorViewModel(Chart, i)).AsObservable();
            Bots = allBots.AsObservable();

            var dataSeries = charts.SelectMany(c => c.DataSeriesCollection);

            ChartControl = new AlgoChartViewModel(dataSeries);
            ChartControl.SymbolInfo.Value = smb;
            ChartControl.ChartWindowId.Value = ChartWindowId;

            // index from VarCollection.CombineChained doesn't work properly when first collection changes size
            //_indicatorOutputs = new VarList<OutputGroupViewModel>();
            //_botOutputs = new VarList<OutputGroupViewModel>();
            //var allOutputs = VarCollection.CombineChained(_indicatorOutputs, _botOutputs);

            FilterChartBots();
            _algoEnv.LocalAgent.BotUpdated += BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated += BotsOnUpdated;

            allIndicators.Updated += AllIndicators_Updated;
            allBots.Updated += AllBots_Updated;

            periodActivatos.Add(ChartPeriods.MN1, () => ActivateBarChart(Feed.Types.Timeframe.MN, "MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.W1, () => ActivateBarChart(Feed.Types.Timeframe.W, "d MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.D1, () => ActivateBarChart(Feed.Types.Timeframe.D, "d MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.H4, () => ActivateBarChart(Feed.Types.Timeframe.H4, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.H1, () => ActivateBarChart(Feed.Types.Timeframe.H1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M30, () => ActivateBarChart(Feed.Types.Timeframe.M30, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M15, () => ActivateBarChart(Feed.Types.Timeframe.M15, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M5, () => ActivateBarChart(Feed.Types.Timeframe.M5, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M1, () => ActivateBarChart(Feed.Types.Timeframe.M1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.S10, () => ActivateBarChart(Feed.Types.Timeframe.S10, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add(ChartPeriods.S1, () => ActivateBarChart(Feed.Types.Timeframe.S1, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add(ChartPeriods.Ticks, () => ActivateTickChart());

            SelectedPeriod = periodActivatos.ContainsKey(period) ? periodActivatos.FirstOrDefault(p => p.Key == period) : periodActivatos.ElementAt(8);

            CloseCommand = new GenericCommand(o => TryClose());

            ChartControl.Overlay = new BotListOverlayViewModel(Bots);
        }

        #region Bindable Properties

        private readonly Dictionary<ChartPeriods, System.Action> periodActivatos = new Dictionary<ChartPeriods, System.Action>();
        private KeyValuePair<ChartPeriods, System.Action> selectedPeriod;

        public string ChartWindowId { get; }

        public Dictionary<ChartPeriods, System.Action> AvailablePeriods => periodActivatos;

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

        public AlgoChartViewModel ChartControl { get; }

        public BoolVar IsCrosshairEnabled => ChartControl.IsCrosshairEnabled.Var;

        public KeyValuePair<ChartPeriods, System.Action> SelectedPeriod
        {
            get { return selectedPeriod; }
            set
            {
                selectedPeriod = value;
                NotifyOfPropertyChange(nameof(SelectedPeriod));
                DisplayName = $"{Symbol}, {SelectedPeriod.Key}";
                selectedPeriod.Value();
            }
        }

        public IReadOnlyList<IndicatorViewModel> Indicators { get; private set; }
        public IReadOnlyList<AlgoBotViewModel> Bots { get; private set; }
        public GenericCommand CloseCommand { get; private set; }

        public bool HasIndicators { get { return Indicators.Count > 0; } }
        public bool CanAddBot => Chart.TimeFrame != Feed.Types.Timeframe.Ticks;
        public bool CanAddIndicator => Chart.TimeFrame != Feed.Types.Timeframe.Ticks;

        #endregion

        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            Indicators.Foreach(i => _shell.Agent.IdProvider.UnregisterPlugin(i.Model.InstanceId));

            _algoEnv.LocalAgent.BotUpdated -= BotOnUpdated;
            _algoEnv.LocalAgentVM.Bots.Updated -= BotsOnUpdated;

            _shell.ToolWndManager.CloseWindowByKey(this);

            ChartControl.Dispose();
            barChart.Dispose();
            tickChart.Dispose();
        }

        public ChartStorageEntry GetSnapshot()
        {
            return new ChartStorageEntry
            {
                Id = ChartWindowId,
                Symbol = Symbol,
                SelectedPeriod = SelectedPeriod.Key,
                SelectedChartType = Chart.SelectedChartType,
                CrosshairEnabled = ChartControl.IsCrosshairEnabled.Value,
                Indicators = Indicators.Select(i => new IndicatorStorageEntry
                {
                    Config = Algo.Common.Model.Config.PluginConfig.FromDomain(i.Model.Config),
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
            ChartControl.IsCrosshairEnabled.Value = snapshot.CrosshairEnabled;
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

            Chart.AddIndicator(entry.Config.ToDomain());
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
                if (item.Descriptor.Type == AlgoTypes.Robot)
                {
                    _algoEnv.LocalAgentVM.OpenBotSetup(item.PluginInfo, Chart.GetSetupContextInfo());
                    return;
                }

                var model = new LocalPluginSetupViewModel(_shell.Agent, item.Key, AlgoTypes.Indicator, Chart.GetSetupContextInfo());
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
                case AlgoTypes.Indicator:
                    Chart.AddIndicator(setupModel.GetConfig());
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

        private void ActivateBarChart(Feed.Types.Timeframe timeFrame, string dateLabelFormat)
        {
            barChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = barChart;
            barChart.Activate(timeFrame);
            FilterChartBots();
        }

        private void ActivateTickChart()
        {
            this.Chart = tickChart;
            tickChart.Activate();
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
            ChartControl.SetTimeframe(Chart.TimeFrame);
            NotifyOfPropertyChange(nameof(CanAddBot));
            NotifyOfPropertyChange(nameof(CanAddIndicator));
        }

        private void TimeAxis_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChartControl.TimeAxis.Value = Chart.TimeAxis.Value;
        }

        private void Indicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            NotifyOfPropertyChange(nameof(HasIndicators));
        }

        private void AllIndicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            var allOutputs = ChartControl.OutputGroups;

            if (args.Action == DLinqAction.Insert)
            {
                allOutputs.Add(new OutputGroupViewModel(args.NewItem, ChartWindowId, Chart, smb, IsCrosshairEnabled));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var index = allOutputs.IndexOf(allOutputs.Values.First(o => o.Model == args.OldItem));
                allOutputs[index].Dispose();
                allOutputs[index] = new OutputGroupViewModel(args.NewItem, ChartWindowId, Chart, smb, IsCrosshairEnabled);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var output = allOutputs.Values.First(o => o.Model == args.OldItem);
                output.Dispose();
                allOutputs.Remove(output);
            }
            NotifyOfPropertyChange(nameof(HasIndicators));
        }

        private void AllBots_Updated(ListUpdateArgs<AlgoBotViewModel> args)
        {
            var allOutputs = ChartControl.OutputGroups;

            if (args.Action == DLinqAction.Insert)
            {
                allOutputs.Add(new OutputGroupViewModel((TradeBotModel)args.NewItem.Model, ChartWindowId, Chart, smb, IsCrosshairEnabled));
            }
            else if (args.Action == DLinqAction.Replace)
            {
                var index = allOutputs.IndexOf(allOutputs.Values.First(o => o.Model == args.OldItem.Model));
                allOutputs[index].Dispose();
                allOutputs[index] = new OutputGroupViewModel((TradeBotModel)args.NewItem.Model, ChartWindowId, Chart, smb, IsCrosshairEnabled);
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var output = allOutputs.Values.First(o => o.Model == args.OldItem.Model);
                output.Dispose();
                allOutputs.Remove(output);
            }
        }

        private void InitChart()
        {
            charts.Clear();
            charts.Add(Chart);

            //ChartControl.TimeAxis.Value = Chart.TimeAxis.Value;
            ChartControl.BindAxis(Chart.TimeAxis);
            ChartControl.BindCurrentRate(Chart.CurrentRate);
            Chart_TimeframeChanged();

            Chart.TimeframeChanged += Chart_TimeframeChanged;
            Chart.TimeAxis.PropertyChanged += TimeAxis_PropertyChanged;
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
                //if (plugin.Type == AlgoTypes.Indicator)
                if (plugin.Type == AlgoTypes.Indicator && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                    return true;
                if (plugin.Type == AlgoTypes.Robot && Chart.TimeFrame != Feed.Types.Timeframe.Ticks)
                    return true;
            }
            return false;
        }

        private void FilterChartBots()
        {
            _chartBots.Clear();
            _algoEnv.LocalAgentVM.Bots.Where(IsChartBot).Snapshot.Foreach(AddChartBot);
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