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
    class ChartViewModel : Screen
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

        public enum SelectableChartTypes { Candle, OHLC, Line, Mountain }

        private readonly ConnectionModel connection;
        private readonly TriggeredActivity updateActivity;
        private AlgoRepositoryModel repository;

        public ChartViewModel(string symbol, ConnectionModel connection, AlgoRepositoryModel repository)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.connection = connection;
            this.repository = repository;

            this.updateActivity = new TriggeredActivity(Update);

            UpdateSeriesStyle();

            connection.Connected += connection_Connected;
            connection.Disconnected += connection_Disconnected;

            if (connection.State.Current == ConnectionModel.States.Online)
                updateActivity.Trigger();
        }

        #region Bindable Properties

        private bool isBusy;
        private IndexRange visibleRange = new IndexRange(0, 10);
        private OhlcDataSeries<DateTime, double> data;
        private BarPeriod selectedPeriod = BarPeriod.M30;
        private SelectableChartTypes chartType = SelectableChartTypes.Candle;
        private ObservableCollection<IRenderableSeries> series = new ObservableCollection<IRenderableSeries>();

        public BarPeriod[] AvailablePeriods { get { return PredefinedPeriods; } }

        public IndexRange VisibleRange
        {
            get { return visibleRange; }
            set
            {
                visibleRange = value;
                NotifyOfPropertyChange("VisibleRange");
            }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (this.isBusy != value)
                {
                    this.isBusy = value;
                    NotifyOfPropertyChange("IsBusy");
                }
            }
        }

        public OhlcDataSeries<DateTime, double> Data
        {
            get { return data; }
            set
            {
                data = value;
                series[0].DataSeries = value;
                NotifyOfPropertyChange("Data");
            }
        }

        public IRenderableSeries MainSeries
        {
            get { return series[0]; }
            set
            {
                if (series.Count > 0)
                    series[0] = value;
                else
                    series.Add(value);
            }
        }

        public BarPeriod SelectedPeriod
        {
            get { return selectedPeriod; }
            set
            {
                selectedPeriod = value;
                NotifyOfPropertyChange("SelectedPeriod");
                updateActivity.Trigger(true);
            }
        }

        public ObservableCollection<IRenderableSeries> Series { get { return series; } }

        public Array ChartTypes { get { return Enum.GetValues(typeof(SelectableChartTypes)); } }

        public SelectableChartTypes SelectedChartType
        {
            get { return chartType; }
            set
            {
                chartType = value;
                NotifyOfPropertyChange("SelectedChartType");
                UpdateSeriesStyle();
            }
        }

        public BindableCollection<AlgoRepositoryItem> RepositoryIndicators { get { return repository.Indicators; } }

        public BindableCollection<IndicatorModel> Indicators { get; private set; }

        #endregion

        public void OpenIndicator(object descriptorObj)
        {
            AlgoRepositoryItem metadata = (AlgoRepositoryItem)descriptorObj;
            Indicators.Add(new IndicatorModel(metadata, null));
        }

        private Task connection_Disconnected(object sender)
        {
            return updateActivity.Abort();
        }

        private void connection_Connected()
        {
            updateActivity.Trigger(true);
        }

        private async Task Update(CancellationToken cToken)
        {
            this.Data = null;
            this.IsBusy = true;

            try
            {
                var response = await Task.Factory.StartNew(
                    () => connection.FeedProxy.Server.GetHistoryBars(
                        Symbol, DateTime.Now + TimeSpan.FromDays(1),
                        -4000, SoftFX.Extended.PriceType.Ask, SelectedPeriod));

                cToken.ThrowIfCancellationRequested();

                var newData = new OhlcDataSeries<DateTime, double>();

                foreach (var bar in response.Bars.Reverse())
                    newData.Append(bar.From, bar.Open, bar.High, bar.Low, bar.Close);

                
                this.Data = newData;
                if (newData.Count > 0)
                {
                    this.VisibleRange.Max = newData.Count - 1;
                    this.VisibleRange.Min = Math.Max(0, newData.Count - 101);
                }
            }
            catch (Exception ex)
            {
            }

            this.IsBusy = false;
        }

        public string Symbol { get; private set; }

        private void UpdateSeriesStyle()
        {
            switch (SelectedChartType)
            {
                case SelectableChartTypes.Candle:
                    MainSeries = new FastCandlestickRenderableSeries();
                    break;
                case SelectableChartTypes.Line:
                    MainSeries = new FastLineRenderableSeries();
                    break;
                case SelectableChartTypes.OHLC:
                    MainSeries = new FastOhlcRenderableSeries();
                    break;
                case SelectableChartTypes.Mountain:
                    MainSeries = new FastMountainRenderableSeries();
                    break;
            }

            MainSeries.DataSeries = Data;
        }
    }
}