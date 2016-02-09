using Abt.Controls.SciChart;
using Abt.Controls.SciChart.Visuals.RenderableSeries;
using Caliburn.Micro;
using StateMachinarium;
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

    internal abstract class ChartModelBase : PropertyChangedBase, IIndicatorHost, IDisposable
    {
        private enum States { Idle, UpdatingData, Closed }
        private enum Events { DoneUpdating }

        private StateMachine<States> stateController = new StateMachine<States>(new DispatcherStateMachineSync());
        private BindableCollection<IRenderableSeries> series = new BindableCollection<IRenderableSeries>();
        private IndicatorsCollection indicators;
        private AlgoCatalog catalog;
        private SelectableChartTypes chartType;
        private bool isLoading;
        private bool isUpdateRequired;
        private bool isCloseRequested;
        private bool isOnline;
        private readonly List<SelectableChartTypes> supportedChartTypes = new List<SelectableChartTypes>();
        private ChartNavigator navigator;
        private TimelineTypes timelineType;
        private long indicatorNextId = 1;

        public ChartModelBase(SymbolModel symbol, AlgoCatalog catalog, FeedModel feed)
        {
            this.Feed = feed;
            this.Model = symbol;
            this.catalog = catalog;
            this.indicators = new IndicatorsCollection(series);

            TimelineType = TimelineTypes.Uniform;

            this.AvailableIndicators = new BindableCollection<AlgoCatalogItem>();

            this.isOnline = feed.Connection.State.Current == ConnectionModel.States.Online;
            feed.Connection.Connected += Connection_Connected;
            feed.Connection.Disconnected += Connection_Disconnected;

            foreach (var indicator in catalog.Indicators)
            {
                if (IsIndicatorSupported(indicator.Descriptor))
                    AvailableIndicators.Add(indicator);
            }

            catalog.Added += Repository_Added;
            catalog.Removed += Repository_Removed;
            catalog.Replaced += Repository_Replaced;

            stateController.AddTransition(States.Idle, () => isUpdateRequired && isOnline, States.UpdatingData);
            stateController.AddTransition(States.Idle, () => isCloseRequested, States.Closed);
            stateController.AddTransition(States.UpdatingData, Events.DoneUpdating, States.Idle);

            stateController.OnEnter(States.UpdatingData, ()=> Update(CancellationToken.None));

            stateController.StateChanged += (o, n) => System.Diagnostics.Debug.WriteLine("Chart [" + Model.Name + "] " + o + " => " + n);
        }

        protected SymbolModel Model { get; private set; }
        protected FeedModel Feed { get; private set; }
        protected ConnectionModel Connection { get { return Feed.Connection; } }

        public ObservableCollection<IRenderableSeries> Series { get { return series; } }
        public BindableCollection<AlgoCatalogItem> AvailableIndicators { get; private set; }
        public BindableCollection<IndicatorModel> Indicators { get { return indicators.Values; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public IEnumerable<TimelineTypes> AvailableTimelines { get { return new TimelineTypes[] { TimelineTypes.Uniform, TimelineTypes.Real }; } }
        public string Symbol { get { return Model.Name; } }

        protected void Activate()
        {
            stateController.ModifyConditions(() => isUpdateRequired = true);
        }

        protected void ReserveTopSeries(int count)
        {
            for (int i = 0; i < count; i++)
                Series.Add(null);
        }

        public void Deactivate()
        {
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

        public IIndicatorConfig CreateIndicatorConfig(AlgoCatalogItem item)
        {
            return CreateInidactorConfig(item);
        }

        public void AddOrUpdateIndicator(IIndicatorConfig config)
        {
            indicators.AddOrReplace(new IndicatorModel(config));
        }

        protected long GetNextIndicatorId()
        {
            return indicatorNextId++;
        }

        protected abstract void ClearData();
        protected abstract Task LoadData(CancellationToken cToken);
        protected abstract void UpdateSeriesStyle();
        protected abstract bool IsIndicatorSupported(AlgoInfo descriptor);
        protected abstract IIndicatorConfig CreateInidactorConfig(AlgoCatalogItem repItem);

        protected void Support(SelectableChartTypes chartType)
        {
            this.supportedChartTypes.Add(chartType);
        }

        private async void Update(CancellationToken cToken)
        {
            this.IsLoading = true;

            try
            {
                isUpdateRequired = false;
                ClearData();
                await indicators.Stop();
                await LoadData(cToken);
                indicators.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ChartModelBase.Update() ERROR " + ex.ToString());
            }

            stateController.PushEvent(Events.DoneUpdating);

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

        private void Repository_Replaced(AlgoCatalogItem obj)
        {
        }

        private void Repository_Removed(AlgoCatalogItem obj)
        {
        }

        private void Repository_Added(AlgoCatalogItem obj)
        {
        }

        private Task Connection_Disconnected(object sender)
        {
            stateController.ModifyConditions(() => isOnline = false);
            return Task.FromResult(new object());
        }

        private void Connection_Connected()
        {
            stateController.ModifyConditions(() => isOnline = true);
        }

        public void Dispose()
        {
            catalog.Added -= Repository_Added;
            catalog.Removed -= Repository_Removed;
            catalog.Replaced -= Repository_Replaced;
        }
    }
}
