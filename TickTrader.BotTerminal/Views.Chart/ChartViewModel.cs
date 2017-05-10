using Caliburn.Micro;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals.RenderableSeries;
using SoftFX.Extended;
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

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Screen, IDropHandler
    {
        private static int idSeed;
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TraderClientModel clientModel;
        private readonly AlgoEnvironment algoEnv;
        private ChartModelBase activeChart;
        private readonly BarChartModel barChart;
        private readonly TickChartModel tickChart;
        private readonly IShell shell;
        private readonly DynamicList<BotControlViewModel> bots = new DynamicList<BotControlViewModel>();
        private readonly DynamicList<ChartModelBase> charts = new DynamicList<ChartModelBase>();

        public ChartViewModel(string symbol, IShell shell, TraderClientModel clientModel, AlgoEnvironment algoEnv)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.clientModel = clientModel;
            this.algoEnv = algoEnv;
            this.shell = shell;

            ChartWindowId = "Chart" + ++idSeed;

            SymbolModel smb = (SymbolModel)clientModel.Symbols[symbol];

            Precision = smb.Descriptor.Precision;
            UpdateLabelFormat();

            this.barChart = new BarChartModel(smb, algoEnv, clientModel);
            this.tickChart = new TickChartModel(smb, algoEnv, clientModel);
            this.UiLock = new UiLock();

            var allIndicators = charts.SelectMany(c => c.Indicators);
            var dataSeries = charts.SelectMany(c => c.DataSeriesCollection);
            var indicatorViewModels = allIndicators.Chain().Select(i => new IndicatorViewModel(Chart, i, ChartWindowId, smb));
            var overlayIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasOverlayOutputs);
            var overlaySeries = overlayIndicators.Chain().SelectMany(i => i.Series);
            var allSeries = Dynamic.CombineChained(dataSeries, overlaySeries);
            var paneIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasPaneOutputs);
            var panes = paneIndicators.Chain().SelectMany(i => i.Panes);

            Series = allSeries.AsObservable();

            Indicators = indicatorViewModels.AsObservable();
            Panes = panes.AsObservable();
            Bots = bots.AsObservable();

            periodActivatos.Add("MN1", () => ActivateBarChart(TimeFrames.MN, "MMMM yyyy"));
            periodActivatos.Add("W1", () => ActivateBarChart(TimeFrames.W, "d MMMM yyyy"));
            periodActivatos.Add("D1", () => ActivateBarChart(TimeFrames.D, "d MMMM yyyy"));
            periodActivatos.Add("H4", () => ActivateBarChart(TimeFrames.H4, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("H1", () => ActivateBarChart(TimeFrames.H1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M30", () => ActivateBarChart(TimeFrames.M30, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M15", () => ActivateBarChart(TimeFrames.M15, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M5", () => ActivateBarChart(TimeFrames.M5, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M1", () => ActivateBarChart(TimeFrames.M1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("S10", () => ActivateBarChart(TimeFrames.S10, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add("S1", () => ActivateBarChart(TimeFrames.S1, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add("Ticks", () => ActivateTickChart());

            SelectedPeriod = periodActivatos.ElementAt(8);
            
            ViewPort = new CustomViewPortManager();

            CloseCommand = new GenericCommand(o => TryClose());
        }

        #region Bindable Properties

        private readonly Dictionary<string, System.Action> periodActivatos = new Dictionary<string, System.Action>();
        private KeyValuePair<string, System.Action> selectedPeriod;

        public string ChartWindowId { get; private set; }

        public Dictionary<string, System.Action> AvailablePeriods { get { return periodActivatos; } }

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

        public KeyValuePair<string, System.Action> SelectedPeriod
        {
            get { return selectedPeriod; }
            set
            {
                selectedPeriod = value;
                NotifyOfPropertyChange("SelectedPeriod");
                selectedPeriod.Value();
            }
        }

        public IViewportManager ViewPort { get; private set; }

        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; private set; }
        public IReadOnlyList<IndicatorPaneViewModel> Panes { get; private set; }
        public IReadOnlyList<IndicatorViewModel> Indicators { get; private set; }
        public IReadOnlyList<BotControlViewModel> Bots { get; private set; }
        public GenericCommand CloseCommand { get; private set; }

        public bool HasIndicators { get { return Indicators.Count > 0; } }

        public int Precision { get; private set; }
        public string YAxisLabelFormat { get; private set; }

        #endregion

        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            shell.ToolWndManager.CloseWindow(this);
        }

        public void OpenOrder()
        {
            shell.OrderCommands.OpenMarkerOrder(Symbol);
        }

        #region Algo

        public void OpenPlugin(object descriptorObj)
        {
            OpenAlgoSetup((PluginCatalogItem)descriptorObj);
        }

        private void OpenAlgoSetup(PluginCatalogItem item)
        {
            try
            {
                var model = new PluginSetupViewModel(algoEnv.Repo, item, Chart);
                if (!model.SetupCanBeSkipped)
                    shell.ToolWndManager.OpenWindow("AlgoSetupWindow", model, true);
                else
                    AttachPlugin(model);

                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void AttachPlugin(PluginSetupViewModel setupModel)
        {
            var pluginType = setupModel.Setup.Descriptor.AlgoLogicType;

            if (pluginType == AlgoTypes.Indicator)
            {
                Chart.AddIndicator(setupModel.Setup);
            }
            else if (pluginType == AlgoTypes.Robot)
            {
                var bot = new TradeBotModel2(setupModel.Setup, Chart);
                //bot.TimeFrame = Chart.TimeFrame;
                //bot.MainSymbol = Chart.SymbolCode;
                //bot.TimelineStart = Chart.TimelineStart;
                //bot.TimelineEnd = DateTime.Now + TimeSpan.FromDays(100);
                var viewModel = new BotControlViewModel(bot, shell.ToolWndManager);
                viewModel.Closed += BotClosed;
                bots.Add(viewModel);
                //bot.StateChanged += Bot_StateChanged;
            }
        }

        void AlgoSetupClosed(PluginSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
                AttachPlugin(setupModel);
        }

        private void BotClosed(BotControlViewModel sender)
        {
            bots.Remove(sender);
            sender.Dispose();
            sender.Closed -= BotClosed;
            //sender.Model.StateChanged -= Bot_StateChanged;
        }

        #endregion

        public void Drop(object o)
        {
            var algo = o as AlgoItemViewModel;
            if (algo != null)
            {
                var pluginType = algo.PluginItem.Descriptor.AlgoLogicType;
                if (pluginType == AlgoTypes.Indicator || pluginType == AlgoTypes.Robot)
                    OpenAlgoSetup(algo.PluginItem);
            }
        }

        private void ActivateBarChart(TimeFrames timeFrame, string dateLabelFormat)
        {
            barChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = barChart;
            barChart.Activate(timeFrame);
        }

        private void ActivateTickChart()
        {
            this.Chart = tickChart;
            tickChart.Activate();
            //barChart.Deactivate();
        }

        private void Chart_ParamsLocked()
        {
            shell.ConnectionLock.Lock();
            UiLock.Lock();
        }

        private void Chart_ParamsUnlocked()
        {
            shell.ConnectionLock.Release();
            UiLock.Release();
        }

        private void Indicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            NotifyOfPropertyChange("HasIndicators");
            Precision = Indicators.Where(i => i.Model.HasOverlayOutputs).Max(i => i.Precision);
            UpdateLabelFormat();
        }

        private void InitChart()
        {
            charts.Clear();
            charts.Add(Chart);


            Chart.ParamsLocked += Chart_ParamsLocked;
            Chart.ParamsUnlocked += Chart_ParamsUnlocked;
            Chart.Indicators.Updated += Indicators_Updated;
        }

        private void DeinitChart()
        {
            Chart.ParamsLocked -= Chart_ParamsLocked;
            Chart.ParamsUnlocked -= Chart_ParamsUnlocked;
            Chart.Indicators.Updated -= Indicators_Updated;
        }

        public bool CanDrop(object o)
        {
            return o is AlgoItemViewModel;
        }

        public class CustomViewPortManager : DefaultViewportManager
        {
            protected override IRange OnCalculateNewXRange(IAxis xAxis)
            {
                return base.OnCalculateNewXRange(xAxis);
            }

            protected override IRange OnCalculateNewYRange(IAxis yAxis, RenderPassInfo renderPassInfo)
            {
                return base.OnCalculateNewYRange(yAxis, renderPassInfo);
            }
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat = $"n{Precision}";
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }
    }
}