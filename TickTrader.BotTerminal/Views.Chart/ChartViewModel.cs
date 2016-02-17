using Caliburn.Micro;
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
using System.Windows.Data;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Conductor<Screen>
    {
        private readonly FeedModel feed;
        private AlgoCatalog catalog;
        private IWindowManager wndManager;
        private ChartModelBase activeChart;
        private BarChartModel barChart;
        private TickChartModel tickChart;

        public ChartViewModel(string symbol, FeedModel feed, AlgoCatalog catalog, IWindowManager wndManager)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.feed = feed;
            this.catalog = catalog;
            this.wndManager = wndManager;

            SymbolModel smb = feed.Symbols[symbol];

            this.barChart = new BarChartModel(smb, catalog, feed);
            this.tickChart = new TickChartModel(smb, catalog, feed);

            Panes = new ObservableCollection<IndicatorPaneViewModel>();
            Series = new ObservableCollection<IRenderableSeries>();

            //repository.Removed += repository_Removed;
            //repository.Replaced += repository_Replaced;

            periodActivatos.Add("MN1", () => ActivateBarChart(BarPeriod.MN1));
            periodActivatos.Add("W1", () => ActivateBarChart(BarPeriod.W1));
            periodActivatos.Add("D1", () => ActivateBarChart(BarPeriod.D1));
            periodActivatos.Add("H4", () => ActivateBarChart(BarPeriod.H4));
            periodActivatos.Add("H1", () => ActivateBarChart(BarPeriod.H1));
            periodActivatos.Add("M30", () => ActivateBarChart(BarPeriod.M30));
            periodActivatos.Add("M15", () => ActivateBarChart(BarPeriod.M15));
            periodActivatos.Add("M5", () => ActivateBarChart(BarPeriod.M5));
            periodActivatos.Add("M1", () => ActivateBarChart(BarPeriod.M1));
            periodActivatos.Add("S10", () => ActivateBarChart(BarPeriod.S10));
            periodActivatos.Add("S1", () => ActivateBarChart(BarPeriod.S1));
            periodActivatos.Add("Ticks", () => ActivateTickChart());

            SelectedPeriod = periodActivatos.ElementAt(5);

            //IndicatorPanes.Add(new IndicatorPaneViewModel());
            //IndicatorPanes.Add(new IndicatorPaneViewModel());
            //IndicatorPanes.Add(new IndicatorPaneViewModel());
        }

        #region Bindable Properties

        private readonly Dictionary<string, System.Action> periodActivatos = new Dictionary<string, System.Action>();
        private KeyValuePair<string, System.Action> selectedPeriod;

        public Dictionary<string, System.Action> AvailablePeriods { get { return periodActivatos; } }

        public ChartModelBase Chart
        {
            get { return activeChart; }
            private set
            {
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

        public ObservableCollection<IRenderableSeries> Series { get; set; }
        public ObservableCollection<IndicatorPaneViewModel> Panes { get; set; }

        #endregion

        public string Symbol { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            if (this.ActiveItem != null)
                this.ActiveItem.TryClose();
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

        private void ActivateBarChart(BarPeriod period)
        {
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
            AddSeriesRange(barChart.Series);

            foreach (var i in barChart.Indicators.Values)
                AddIndicator(i);

            barChart.Indicators.Added += AddIndicator;
            barChart.Indicators.Removed += RemoveIndicator;
            barChart.Indicators.Replaced += ReplaceIndicator;
        }

        private void DisposeChart()
        {
            Panes.Clear();
            Series.Clear();

            if (barChart != null)
            {
                barChart.Indicators.Added -= AddIndicator;
                barChart.Indicators.Removed -= RemoveIndicator;
                barChart.Indicators.Replaced -= ReplaceIndicator;
            }
        }

        private void AddSeriesRange(IEnumerable<IRenderableSeries> range)
        {
            foreach (var s in range)
                Series.Add(s);
        }

        private void AddIndicator(IndicatorModel i)
        {
            if (i.IsOverlay)
                AddOutputs(i);
            else
                Panes.Add(new IndicatorPaneViewModel(i, Chart));
        }

        private void RemoveIndicator(IndicatorModel i)
        {
            int paneIndex = Panes.IndexOf(p => p.IndicatorId == i.Id);
            if (paneIndex > 0)
                Panes.RemoveAt(paneIndex);

            RemoveOutputs(i);
        }

        private void ReplaceIndicator(IndicatorModel i)
        {
            if (Panes.Any(p => p.IndicatorId == i.Id) && !i.IsOverlay)
            {
                int index = Panes.IndexOf(p => p.IndicatorId == i.Id);
                Panes[index] = new IndicatorPaneViewModel(i, this.Chart);
            }
            else
            {
                RemoveIndicator(i);
                AddIndicator(i);
            }
        }

        private void AddOutputs(IndicatorModel indicator)
        {
            foreach (var output in indicator.SeriesCollection)
            {
                FastLineRenderableSeries chartSeries = new FastLineRenderableSeries();
                chartSeries.DataSeries = output;
                Series.Add(chartSeries);
            }
        }

        private void RemoveOutputs(IndicatorModel indicator)
        {
            foreach (var output in indicator.SeriesCollection)
            {
                var seriesIndex = Series.IndexOf(s => s.DataSeries == output);
                if (seriesIndex > 0)
                    Series.RemoveAt(seriesIndex);
            }
        }
    }
}