using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    public enum SelectableChartTypes { Candle, OHLC, Line, Mountain, DigitalLine, DigitalMountain, Scatter }
    public enum TimelineTypes { Real, Uniform }

    internal abstract class ChartModelBase : PropertyChangedBase
    {
        private BindableCollection<IRenderableSeries> series = new BindableCollection<IRenderableSeries>();
        private IndicatorsCollection indicators;
        private AlgoRepositoryModel repository;
        private SelectableChartTypes chartType;
        private bool isLoading;
        private readonly TriggeredActivity updateActivity;
        private readonly List<SelectableChartTypes> supportedChartTypes = new List<SelectableChartTypes>();
        private ChartNavigator navigator;
        private TimelineTypes timelineType;

        public ChartModelBase(SymbolModel symbol, FeedModel feed)
        {
            this.Feed = feed;
            this.Model = symbol;
            this.indicators = new IndicatorsCollection(series);
            this.updateActivity = new TriggeredActivity(Update);

            SwitchNavigator(TimelineTypes.Uniform);
        }

        protected SymbolModel Model { get; private set; }
        protected FeedModel Feed { get; private set; }
        protected ConnectionModel Connection { get { return Feed.Connection; } }

        public ObservableCollection<IRenderableSeries> Series { get { return series; } }
        public BindableCollection<IndicatorBuilderModel> Indicators { get { return indicators.Values; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public string Symbol { get { return Model.Name; } }

        protected void Activate()
        {
            if (Feed.Connection.State.Current == ConnectionModel.States.Online)
                updateActivity.Trigger();
        }

        protected void ReserveTopSeries(int count)
        {
            for (int i = 0; i < count; i++)
                Series.Add(null);
        }

        public void Deactivate()
        {
            updateActivity.Cancel();
        }

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (this.isLoading != value)
                {
                    this.isLoading = value;
                    NotifyOfPropertyChange("IsLoading");
                }
            }
        }

        public TimelineTypes TimelineType
        {
            get { return timelineType; }
            set
            {
                this.timelineType = value;
                NotifyOfPropertyChange("TimelineType");
                SwitchNavigator(value);
            }
        }

        public ChartNavigator Navigator
        {
            get { return navigator; }
            private set
            {
                navigator = value;
                NotifyOfPropertyChange("Navigator");
            }
        }

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

        protected abstract Task LoadData(CancellationToken cToken);
        protected abstract void UpdateSeriesStyle();

        protected void Support(SelectableChartTypes chartType)
        {
            this.supportedChartTypes.Add(chartType);
        }

        private async Task Update(CancellationToken cToken)
        {
            this.IsLoading = true;

            try
            {
                await LoadData(cToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ChartModelBase.Update() ERROR " + ex.ToString());
            }

            this.IsLoading = false;
        }

        //public BindableCollection<AlgoRepositoryItem> AvailableIndicators { get { return repository.Indicators; } }

        private void SwitchNavigator(TimelineTypes timelineType)
        {
            switch (timelineType)
            {
                case TimelineTypes.Real:
                    Navigator = new NonUniformChartNavigator();
                    break;
                case TimelineTypes.Uniform:
                    Navigator = new UniformChartNavigator();
                    break;
                default:
                    throw new Exception("Unsupported: " + timelineType);
            }
        }
    }
}
