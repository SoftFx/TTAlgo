using Caliburn.Micro;
using Machinarium.State;
using Machinarium.Qnil;
using NLog;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.GuiModel;
using TickTrader.BotTerminal.Lib;
using SoftFX.Extended;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;

namespace TickTrader.BotTerminal
{
    public enum SelectableChartTypes { Candle, OHLC, Line, Mountain, DigitalLine, DigitalMountain, Scatter }
    public enum TimelineTypes { Real, Uniform }

    internal abstract class ChartModelBase : PropertyChangedBase, IAlgoSetupFactory, IDisposable, IRateUpdatesListener, IAlgoPluginHost
    {
        private Logger logger;
        private enum States { Idle, LoadingData, Online, Stopping, Closed }
        private enum Events { Loaded, Stopped }

        private StateMachine<States> stateController = new StateMachine<States>(new DispatcherStateMachineSync());
        private DynamicList<IRenderableSeriesViewModel> seriesCollection = new DynamicList<IRenderableSeriesViewModel>();
        private DynamicList<IndicatorModel2> indicators = new DynamicList<IndicatorModel2>();
        private PluginCatalog catalog;
        private SelectableChartTypes chartType;
        private bool isIndicatorsOnline;
        private bool isLoading;
        private bool isUpdateRequired;
        private bool isCloseRequested;
        private bool isConnected;
        private readonly List<SelectableChartTypes> supportedChartTypes = new List<SelectableChartTypes>();
        private ChartNavigator navigator;
        private TimelineTypes timelineType;
        private long indicatorNextId = 1;
        private AxisBase timeAxis;
        private bool isCrosshairEnabled;
        private string dateAxisLabelFormat;
        private List<Quote> updateQueue;

        public ChartModelBase(SymbolModel symbol, PluginCatalog catalog, FeedModel feed, BotJournal journal)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.Feed = feed;
            this.Model = symbol;
            this.catalog = catalog;
            this.Journal = journal;

            this.AvailableIndicators = catalog.Indicators.OrderBy((k, v) => v.DisplayName).Chain().AsObservable();

            this.isConnected = feed.Connection.State.Current == ConnectionModel.States.Online;
            feed.Connection.Connected += Connection_Connected;
            feed.Connection.Disconnected += Connection_Disconnected1;

            symbol.Subscribe(this);

            CurrentAsk = symbol.LastQuote?.Ask;
            CurrentBid = symbol.LastQuote?.Bid;

            stateController.AddTransition(States.Idle, () => isUpdateRequired && isConnected, States.LoadingData);
            stateController.AddTransition(States.LoadingData, Events.Loaded, States.Online);
            stateController.AddTransition(States.Online, () => isUpdateRequired && isConnected, States.Stopping);
            //stateController.AddTransition(States.Stopping, () => isUpdateRequired && isConnected, States.LoadingData);
            stateController.AddTransition(States.Stopping, Events.Stopped, States.Idle);

            stateController.OnEnter(States.LoadingData, ()=> Update(CancellationToken.None));
            stateController.OnEnter(States.Online, StartIndicators);
            stateController.OnEnter(States.Stopping, StopIndicators);

            stateController.StateChanged += (o, n) => logger.Debug("Chart [" + Model.Name + "] " + o + " => " + n);
        }

        protected SymbolModel Model { get; private set; }
        protected FeedModel Feed { get; private set; }
        protected ConnectionModel Connection { get { return Feed.Connection; } }
        protected DynamicList<IRenderableSeriesViewModel> SeriesCollection { get { return seriesCollection; } }

        public abstract Api.TimeFrames TimeFrame { get; }
        public IDynamicListSource<IRenderableSeriesViewModel> DataSeriesCollection { get { return seriesCollection; } }
        public IObservableListSource<PluginCatalogItem> AvailableIndicators { get; private set; }
        public IDynamicListSource<IndicatorModel2> Indicators { get { return indicators; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public string SymbolCode { get { return Model.Name; } }
        public BotJournal Journal { get; private set; }
        public double? CurrentAsk { get; private set; }
        public double? CurrentBid { get; private set; }

        protected void Activate()
        {
            stateController.ModifyConditions(() => isUpdateRequired = true);
        }

        //protected void AddSeries(IRenderableSeriesViewModel series)
        //{
        //    seriesCollection.Add(series);
        //}

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
                    NotifyOfPropertyChange(nameof(IsLoading));
                    NotifyOfPropertyChange(nameof(IsReady));
                }
            }
        }

        public bool IsReady { get { return !IsLoading; } }

        public ChartNavigator Navigator
        {
            get { return navigator; }
            protected set
            {
                navigator = value;
                TimeAxis = value.CreateAxis();

                Binding cursorTextFormatBinding = new Binding("DateAxisLabelFormat");
                cursorTextFormatBinding.Source = this;
                cursorTextFormatBinding.Mode = BindingMode.TwoWay;
                TimeAxis.SetBinding(AxisBase.CursorTextFormattingProperty, cursorTextFormatBinding);
            }
        }

        public AxisBase TimeAxis
        {
            get { return timeAxis; }
            set
            {
                timeAxis = value;
                NotifyOfPropertyChange("TimeAxis");
            }
        }

        public SelectableChartTypes SelectedChartType
        {
            get { return chartType; }
            set
            {
                chartType = value;
                UpdateSeries();
                ChartTypeChanged();
                NotifyOfPropertyChange(nameof(SelectedChartType));
            }
        }

        public bool IsCrosshairEnabled
        {
            get { return isCrosshairEnabled; }
            set
            {
                this.isCrosshairEnabled = value;
                NotifyOfPropertyChange("IsCrosshairEnabled");
            }
        }

        public string DateAxisLabelFormat
        {
            get { return dateAxisLabelFormat; }
            set
            {
                this.dateAxisLabelFormat = value;
                NotifyOfPropertyChange("DateAxisLabelFormat");
            }
        }

        public int Depth { get { return 0; } }
        public DateTime TimelineStart { get; private set; }
        public DateTime TimelineEnd { get; private set; }

        public event System.Action ChartTypeChanged = delegate { };
        public event System.Action DepthChanged = delegate { };
        public event System.Action ParamsLocked = delegate { };
        public event System.Action ParamsUnlocked = delegate { };

        public void AddIndicator(PluginSetup setup)
        {
            var indicator = CreateIndicator(setup);
            indicators.Add(indicator);
        }

        public void RemoveIndicator(IndicatorModel2 i)
        {
            if (indicators.Remove(i))
                i.Dispose();
        }

        protected long GetNextIndicatorId()
        {
            return indicatorNextId++;
        }

        protected abstract PluginSetup CreateSetup(AlgoPluginRef catalogItem);

        protected abstract void ClearData();
        protected abstract void UpdateSeries();
        protected abstract Task LoadData(CancellationToken cToken);
        protected abstract IndicatorModel2 CreateIndicator(PluginSetup setup);
        protected abstract void ApplyUpdate(Quote update);

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
                updateQueue = new List<Quote>();
                await StopEvent.InvokeAsync(this);
                await LoadData(cToken);
                ApplyQueue();
                StartEvent();
            }
            catch (Exception ex)
            {
                logger.Error("Update ERROR " + ex);
            }

            stateController.PushEvent(Events.Loaded);

            this.IsLoading = false;
        }

        protected void InitBoundaries(int count, DateTime startDate, DateTime endDate)
        {
            TimelineStart = startDate;
            TimelineEnd = endDate;
            Navigator.Init(count, startDate, endDate);
        }

        protected void ExtendBoundaries(int count, DateTime endDate)
        {
            TimelineEnd = endDate;
            Navigator.Extend(count, endDate);
        }

        private void Connection_Disconnected1()
        {
            stateController.ModifyConditions(() => isConnected = false);
        }

        private void Connection_Connected()
        {
            stateController.ModifyConditions(() => isConnected = true);
        }

        public void Dispose()
        {
            AvailableIndicators.Dispose();

            Model.Unsubscribe(this);
        }

        public virtual void OnRateUpdate(Quote tick)
        {
            if (stateController.Current == States.LoadingData)
                updateQueue.Add(tick);
            else if (stateController.Current == States.Online)
                ApplyUpdate(tick);

            this.CurrentAsk = tick.Ask;
            this.CurrentBid = tick.Bid;
            NotifyOfPropertyChange(nameof(CurrentAsk));
            NotifyOfPropertyChange(nameof(CurrentBid));
        }

        private void ApplyQueue()
        {
            updateQueue.ForEach(ApplyUpdate);
            updateQueue = null;
        }

        private void StartIndicators()
        {
            isIndicatorsOnline = true;
            StartEvent();
        }

        private async void StopIndicators()
        {
            isIndicatorsOnline = false;
            await StopEvent.InvokeAsync(this);
            stateController.PushEvent(Events.Stopped);
        }

        PluginSetup IAlgoSetupFactory.CreateSetup(AlgoPluginRef catalogItem)
        {
            return CreateSetup(catalogItem);
        }

        protected virtual IPluginFeedProvider CreateProvider()
        {
            return new PluginFeedProvider(Feed.Symbols, Feed.History);
        }

        #region IAlgoPluginHost

        void IAlgoPluginHost.Lock()
        {
            ParamsLocked();
        }

        void IAlgoPluginHost.Unlock()
        {
            ParamsUnlocked();
        }

        IPluginFeedProvider IAlgoPluginHost.GetProvider()
        {
            return CreateProvider();
        }

        protected abstract FeedStrategy GetFeedStrategy();

        FeedStrategy IAlgoPluginHost.GetFeedStrategy()
        {
            return GetFeedStrategy();
        }

        bool IAlgoPluginHost.IsStarted { get { return isIndicatorsOnline; } }

        public event System.Action ParamsChanged = delegate { };
        public event System.Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return CompletedTask.Default; };

        #endregion
    }
}
