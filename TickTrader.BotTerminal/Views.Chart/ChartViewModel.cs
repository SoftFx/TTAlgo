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

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Conductor<Screen>, IDropHandler
    {
        private static int idSeed;

        private readonly FeedModel feed;
        private AlgoCatalog catalog;
        private IWindowManager wndManager;
        private ChartModelBase activeChart;
        private BarChartModel barChart;
        private TickChartModel tickChart;
        private DynamicList<IndicatorModel> indicators = new DynamicList<IndicatorModel>();
        private DynamicList<IRenderableSeriesViewModel> mainSeries = new DynamicList<IRenderableSeriesViewModel>();

        public ChartViewModel(string symbol, FeedModel feed, AlgoCatalog catalog, IWindowManager wndManager)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.feed = feed;
            this.catalog = catalog;
            this.wndManager = wndManager;

            ChartWindowId = "Chart" + ++idSeed;

            SymbolModel smb = feed.Symbols[symbol];

            UpdateLabelFormat(smb);

            this.barChart = new BarChartModel(smb, catalog, feed);
            this.tickChart = new TickChartModel(smb, catalog, feed);

            //OverlaySeries = new DynamicList<IRenderableSeriesViewModel>();

            var indicatorViewModels = indicators.Select(i => new IndicatorViewModel(i));
            var overlayIndicators = indicatorViewModels.Where(i => i.Model.IsOverlay);
            var paneIndicator = indicatorViewModels.Where(i => !i.Model.IsOverlay);
            var panes = paneIndicator.Select(i => new IndicatorPaneViewModel(i, Chart, ChartWindowId));
            var overlaySeries = overlayIndicators.SelectMany(i => i.Series);
            var allSeries = DLinq.Combine(mainSeries, overlaySeries);

            Indicators = indicatorViewModels.AsObservable();
            Panes = panes.AsObservable();

            // SciChart does not support anything except ObservableCollection. Booo!
            Series = new ObservableCollection<IRenderableSeriesViewModel>();
            allSeries.ConnectTo(Series); // Do not use ConnectTo method except a case of ugly hacks

            ///Series = mainSeries new ObservableComposer<IRenderableSeriesViewModel>();

            //Indicators = new ObservableCollection<IndicatorModel>();

            indicators.Updated += (a) => NotifyOfPropertyChange("HasIndicators");

            //repository.Removed += repository_Removed;
            //repository.Replaced += repository_Replaced;

            periodActivatos.Add("MN1", () => ActivateBarChart(BarPeriod.MN1, "MMMM yyyy"));
            periodActivatos.Add("W1", () => ActivateBarChart(BarPeriod.W1, "d MMMM yyyy"));
            periodActivatos.Add("D1", () => ActivateBarChart(BarPeriod.D1, "d MMMM yyyy"));
            periodActivatos.Add("H4", () => ActivateBarChart(BarPeriod.H4, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("H1", () => ActivateBarChart(BarPeriod.H1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M30", () => ActivateBarChart(BarPeriod.M30, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M15", () => ActivateBarChart(BarPeriod.M15, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M5", () => ActivateBarChart(BarPeriod.M5, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M1", () => ActivateBarChart(BarPeriod.M1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("S10", () => ActivateBarChart(BarPeriod.S10, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add("S1", () => ActivateBarChart(BarPeriod.S1, "d MMMM yyyy HH:mm:ss"));
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
                    DisposeChart();
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

        public bool HasIndicators { get { return Indicators.Count > 0; } }

        public string YAxisLabelFormat { get; private set; }

        #endregion

        public string Symbol { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            if (this.ActiveItem != null)
                this.ActiveItem.TryClose();
        }

        public void OpenOrder()
        {
            try
            {
                var openOrderModel = new OpenOrderDialogViewModel();
                wndManager.ShowWindow(openOrderModel);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        public void OpenIndicator(object descriptorObj)
        {
            try
            {
                var model = new IndicatorSetupViewModel(catalog, (AlgoCatalogItem)descriptorObj, Chart);
                wndManager.ShowWindow(model);
                ActivateItem(model);

                model.Closed += model_Closed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        void model_Closed(IndicatorSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= model_Closed;
        }

        private void ActivateBarChart(BarPeriod period, string dateLabelFormat)
        {
            barChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = barChart;
            barChart.Activate(period);
            //tickChart.Deactivate();
        }

        private void ActivateTickChart()
        {
            this.Chart = tickChart;
            tickChart.Activate();
            //barChart.Deactivate();
        }

        private void InitChart()
        {
            RefreshMainSeries();

            foreach (var i in barChart.Indicators.Values)
                AddIndicator(i);

            Chart.ChartTypeChanged += RefreshMainSeries;
            Chart.Indicators.Added += AddIndicator;
            Chart.Indicators.Removed += RemoveIndicator;
            //barChart.Indicators.Replaced += ReplaceIndicator;
        }

        private void RefreshMainSeries()
        {
            mainSeries.Clear();
            foreach (var series in Chart.DataSeriesCollection)
            {
                var viewModel = SeriesViewModel.Create(Chart, series);
                if (viewModel != null)
                    mainSeries.Add(viewModel);
            }
        }

        private void DisposeChart()
        {
            indicators.Values.Clear();
            mainSeries.Values.Clear();

            if (barChart != null)
            {
                Chart.ChartTypeChanged -= RefreshMainSeries;
                Chart.Indicators.Added -= AddIndicator;
                Chart.Indicators.Removed -= RemoveIndicator;
                //barChart.Indicators.Replaced -= ReplaceIndicator;
            }
        }

        private void AddIndicator(IndicatorModel i)
        {
            indicators.Values.Add(i);
        }

        private void RemoveIndicator(IndicatorModel i)
        {
            indicators.Values.Remove(i);
        }

        public void Drop(object o)
        {
            var algo = o as FakeAlgo;
            if (algo != null)
                MessageBox.Show(string.Format("Algo name: {0} Group name: {1}", algo.Name, algo.Group), "Drop Handler");
        }

        public bool CanDrop(object o)
        {
            return o is FakeAlgo;
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