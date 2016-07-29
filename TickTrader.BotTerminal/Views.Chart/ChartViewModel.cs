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

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Screen, IDropHandler
    {
        private static int idSeed;
        private Logger logger;
        private readonly FeedModel feed;
        private PluginCatalog catalog;
        private BotJournal journal;
        private ChartModelBase activeChart;
        private BarChartModel barChart;
        private TickChartModel tickChart;
        private IShell shell;
        private IDynamicListSource<IRenderableSeriesViewModel> allSeries;
        private IDynamicListSource<IndicatorPaneViewModel> panes;
        //private DynamicList<IDynamicListSource<IndicatorModel2>> indicatorCollections = new DynamicList<IDynamicListSource<IndicatorModel2>>();
        private DynamicList<BotControlViewModel> bots = new DynamicList<BotControlViewModel>();
        //private DynamicList<IRenderableSeriesViewModel> mainSeries = new DynamicList<IRenderableSeriesViewModel>();
        //private DynamicList<IDynamicListSource

        public ChartViewModel(string symbol, IShell shell, FeedModel feed, PluginCatalog catalog, BotJournal journal)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.feed = feed;
            this.catalog = catalog;
            this.shell = shell;
            this.journal = journal;

            ChartWindowId = "Chart" + ++idSeed;

            SymbolModel smb = feed.Symbols[symbol];

            UpdateLabelFormat(smb);

            this.barChart = new BarChartModel(smb, catalog, feed, journal);
            this.tickChart = new TickChartModel(smb, catalog, feed, journal);
            this.UiLock = new UiLock();

            //OverlaySeries = new DynamicList<IRenderableSeriesViewModel>();

            //var indicators = indicatorCollections.SelectMany(c => c);
            //var indicatorViewModels = indicators.Select(i => new IndicatorViewModel(Chart, i));
            //var overlayIndicators = indicatorViewModels.Where(i => i.Model.IsOverlay);
            //var paneIndicator = indicatorViewModels.Where(i => !i.Model.IsOverlay);
            //var panes = paneIndicator.Select(i => new IndicatorPaneViewModel(i, Chart, ChartWindowId));
            //var overlaySeries = overlayIndicators.SelectMany(i => i.Series);
            //var allSeries = Dynamic.Combine(mainSeries, overlaySeries);

            //Indicators = indicatorViewModels.AsObservable();
            Bots = bots.AsObservable();
            //Panes = panes.AsObservable();

            // SciChart does not support anything except ObservableCollection. Booo!
            //Series = new ObservableCollection<IRenderableSeriesViewModel>();
            //allSeries.ConnectTo(Series); // Do not use ConnectTo method except in case of ugly hacks

            ///Series = mainSeries new ObservableComposer<IRenderableSeriesViewModel>();

            //Indicators = new ObservableCollection<IndicatorModel>();

            //indicators.Updated += (a) => NotifyOfPropertyChange("HasIndicators");

            //repository.Removed += repository_Removed;
            //repository.Replaced += repository_Replaced;

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

            //IndicatorPanes.Add(new IndicatorPaneViewModel());
            //IndicatorPanes.Add(new IndicatorPaneViewModel());
            //IndicatorPanes.Add(new IndicatorPaneViewModel());

            ViewPort = new CustomViewPortManager();
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

        public ObservableCollection<IRenderableSeriesViewModel> Series { get; private set; }
        public IObservableListSource<IndicatorPaneViewModel> Panes { get; private set; }
        public IObservableListSource<IndicatorViewModel> Indicators { get; private set; }
        public IObservableListSource<BotControlViewModel> Bots { get; private set; }

        public bool HasIndicators { get { return Indicators.Count > 0; } }

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

        public void OpenIndicator(object descriptorObj)
        {
            OpenAlgoSetup((PluginCatalogItem)descriptorObj);
        }

        private void OpenAlgoSetup(PluginCatalogItem item)
        {
            try
            {
                var model = new PluginSetupViewModel(catalog, item, Chart);
                if (!model.IsEmpty)
                    shell.ToolWndManager.OpenWindow("AlgoSetupWindow", model, true);
                else
                    OpenPlugin(model);

                model.Closed += AlgoSetup_Closed;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void OpenPlugin(PluginSetupViewModel setupModel)
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
                viewModel.Closed += Bot_Closed;
                bots.Add(viewModel);
                //bot.StateChanged += Bot_StateChanged;
            }
        }

        void AlgoSetup_Closed(PluginSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetup_Closed;
            if (dlgResult)
                OpenPlugin(setupModel);
        }

        //private void AddIndicator(IndicatorModel2 i)
        //{
        //    indicatorCollections.Values.Add(i);
        //}

        //private void RemoveIndicator(IndicatorModel2 i)
        //{
        //    indicatorCollections.Values.Remove(i);
        //}

        //private void Bot_StateChanged(TradeBotModel bot)
        //{
        //    if (!bot.IsStarted && bot.IsBusy) // strating
        //    {
        //        shell.ConnectionLock.Lock();
        //        UiLock.Lock();
        //    }
        //    else if (!bot.IsStarted && !bot.IsBusy) // stopped
        //    {
        //        shell.ConnectionLock.Release();
        //        UiLock.Release();
        //    }
        //}

        private void Bot_Closed(BotControlViewModel sender)
        {
            bots.Remove(sender);
            sender.Dispose();
            sender.Closed -= Bot_Closed;
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
            //tickChart.Deactivate();
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

        private void Indicators_Updated(ListUpdateArgs<IndicatorModel2> args)
        {
            NotifyOfPropertyChange("HasIndicators");
        }

        private void InitChart()
        {
            //indicatorCollections.Add(Chart.Indicators);

            var indicatorViewModels = Chart.Indicators.Select(i => new IndicatorViewModel(Chart, i));
            var overlayIndicators = indicatorViewModels.Chain().Where(i => i.Model.IsOverlay);
            var overlaySeries = overlayIndicators.Chain().SelectMany(i => i.Series);
            allSeries = Dynamic.CombineChained(Chart.DataSeriesCollection, overlaySeries);
            Series = new ObservableCollection<IRenderableSeriesViewModel>();
            allSeries.ConnectTo(Series); // Do not use ConnectTo method except in case of ugly hacks
            NotifyOfPropertyChange(nameof(Series));

            Indicators = indicatorViewModels.AsObservable();

            var paneIndicators = indicatorViewModels.Chain().Where(i => !i.Model.IsOverlay);
            panes = paneIndicators.Chain().Select(i => new IndicatorPaneViewModel(i, Chart, ChartWindowId));
            Panes = panes.AsObservable();
            NotifyOfPropertyChange(nameof(Panes));
            

            //var indicatorViewModels = indicators.Select(i => new IndicatorViewModel(Chart, i));
            //var overlaySeries = Chart.Indicators.Where(i => i.IsOverlay).Select(i => i.s


            //foreach (var i in barChart.Indicators.Values)
            //    AddIndicator(i);


            Chart.ParamsLocked += Chart_ParamsLocked;
            Chart.ParamsUnlocked += Chart_ParamsUnlocked;
            Chart.Indicators.Updated += Indicators_Updated;
            //Chart.ChartTypeChanged += RefreshMainSeries;
            //Chart.Indicators.Added += AddIndicator;
            //Chart.Indicators.Removed += RemoveIndicator;
            //barChart.Indicators.Replaced += ReplaceIndicator;
        }

        private void DeinitChart()
        {
            //indicatorCollections.Values.Clear();

            allSeries.Dispose();
            panes.Dispose();

            Chart.ParamsLocked -= Chart_ParamsLocked;
            Chart.ParamsUnlocked -= Chart_ParamsUnlocked;
            Chart.Indicators.Updated -= Indicators_Updated;
            //Chart.ChartTypeChanged -= RefreshMainSeries;

            //foreach (var bot in bots.Values)
            //    bot.Dispose();

            //if (barChart != null)
            //{
            //    Chart.ChartTypeChanged -= RefreshMainSeries;
            //    Chart.Indicators.Added -= AddIndicator;
            //    Chart.Indicators.Removed -= RemoveIndicator;
            //    //barChart.Indicators.Replaced -= ReplaceIndicator;
            //}
        }

        //private void RefreshMainSeries()
        //{
        //    mainSeries.Clear();
        //    foreach (var series in Chart.DataSeriesCollection)
        //    {
        //        var viewModel = SeriesViewModel.Create(Chart, series);
        //        if (viewModel != null)
        //            mainSeries.Add(viewModel);
        //    }
        //}

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

        private void UpdateLabelFormat(SymbolModel smb)
        {
            var digits = smb.Descriptor.Precision;

            StringBuilder formatBuilder = new StringBuilder();
            formatBuilder.Append("0.");

            for (int i = 0; i < digits; i++)
                formatBuilder.Append("0");

            YAxisLabelFormat = formatBuilder.ToString();
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }
    }
}