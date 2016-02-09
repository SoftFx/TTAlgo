using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Model.DataSeries;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Conductor<Screen>
    {
        private static readonly BarPeriod[] PredefinedPeriods = new BarPeriod[]
        {
            BarPeriod.MN1,
            BarPeriod.W1,
            BarPeriod.D1,
            BarPeriod.H4,
            BarPeriod.H1,
            BarPeriod.M30,
            BarPeriod.M15,
            BarPeriod.M5,
            BarPeriod.M1,
            BarPeriod.S10,
            BarPeriod.S1
        };

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
                activeChart = value;
                NotifyOfPropertyChange();
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
    }
}