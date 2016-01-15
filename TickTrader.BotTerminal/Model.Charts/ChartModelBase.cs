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
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    public enum SelectableChartTypes { Candle, OHLC, Line, Mountain, DigitalLine, DigitalMountain, Scatter }
    public enum TimelineTypes { Real, Uniform }

    internal abstract class ChartModelBase : PropertyChangedBase, IDisposable
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

        public ChartModelBase(SymbolModel symbol, AlgoRepositoryModel repository, FeedModel feed)
        {
            this.Feed = feed;
            this.Model = symbol;
            this.repository = repository;
            this.indicators = new IndicatorsCollection(series);
            this.updateActivity = new TriggeredActivity(Update);

            TimelineType = TimelineTypes.Uniform;

            this.AvailableIndicators = new BindableCollection<AlgoRepositoryItem>();

            foreach (var indicator in repository.Indicators)
            {
                if (IsIndicatorSupported(indicator.Descriptor))
                    AvailableIndicators.Add(indicator);
            }

            repository.Added += Repository_Added;
            repository.Removed += Repository_Removed;
            repository.Replaced += Repository_Replaced;
        }

        protected SymbolModel Model { get; private set; }
        protected FeedModel Feed { get; private set; }
        protected ConnectionModel Connection { get { return Feed.Connection; } }

        public ObservableCollection<IRenderableSeries> Series { get { return series; } }
        public BindableCollection<AlgoRepositoryItem> AvailableIndicators { get; private set; }
        public BindableCollection<IndicatorBuilderModel> Indicators { get { return indicators.Values; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public IEnumerable<TimelineTypes> AvailableTimelines { get { return new TimelineTypes[] { TimelineTypes.Uniform, TimelineTypes.Real }; } }
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

        public abstract IndicatorSetup CreateSetup(AlgoRepositoryItem item);

        protected abstract void ClearData();
        protected abstract Task LoadData(CancellationToken cToken);
        protected abstract void UpdateSeriesStyle();
        protected abstract bool IsIndicatorSupported(AlgoInfo descriptor);

        protected void Support(SelectableChartTypes chartType)
        {
            this.supportedChartTypes.Add(chartType);
        }

        private async Task Update(CancellationToken cToken)
        {
            this.IsLoading = true;

            try
            {
                ClearData();
                await LoadData(cToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ChartModelBase.Update() ERROR " + ex.ToString());
            }

            this.IsLoading = false;
        }

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

        private void Repository_Replaced(AlgoRepositoryItem obj)
        {
        }

        private void Repository_Removed(AlgoRepositoryItem obj)
        {
        }

        private void Repository_Added(AlgoRepositoryItem obj)
        {
        }

        public void Dispose()
        {
            repository.Added -= Repository_Added;
            repository.Removed -= Repository_Removed;
            repository.Replaced -= Repository_Replaced;
        }
    }
}
