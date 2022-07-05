using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.BotTerminal.Lib;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;
using System.Collections.Specialized;
using Machinarium.Var;
using SM = Machinarium.State;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server;
using TickTrader.Algo.Package;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Account;
using TickTrader.Algo.Core.Subscriptions;

namespace TickTrader.BotTerminal
{
    public enum SelectableChartTypes { Candle, OHLC, Line, Mountain, DigitalLine, DigitalMountain, Scatter }
    public enum TimelineTypes { Real, Uniform }

    internal abstract class ChartModelBase : PropertyChangedBase, IDisposable, IAlgoPluginHost, IAlgoSetupContext
    {
        private Logger logger;
        private enum States { Idle, LoadingData, Online, Stopping, Closed, Faulted }
        private enum Events { Loaded, LoadFailed, Stopped }

        private SM.StateMachine<States> stateController = new SM.StateMachine<States>(new DispatcherStateMachineSync());
        private VarList<IRenderableSeriesViewModel> seriesCollection = new VarList<IRenderableSeriesViewModel>();
        private VarList<IndicatorModel> indicators = new VarList<IndicatorModel>();
        private SelectableChartTypes chartType;
        private bool isIndicatorsOnline;
        private bool isLoading;
        private bool isUpdateRequired;
        private bool isConnected;
        private bool _isDisposed;
        private readonly List<SelectableChartTypes> supportedChartTypes = new List<SelectableChartTypes>();
        private ChartNavigator navigator;
        private long indicatorNextId = 1;
        private Property<AxisBase> _timeAxis = new Property<AxisBase>();
        private string dateAxisLabelFormat;
        private List<QuoteInfo> updateQueue;
        private IDisposable _subscription;
        private Property<IRateInfo> _currentRateProp = new Property<IRateInfo>();
        private Feed.Types.Timeframe _timeframe;

        public ChartModelBase(SymbolInfo symbol, AlgoEnvironment algoEnv)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            AlgoEnv = algoEnv;
            this.Model = symbol;

            AvailableIndicators = AlgoEnv.LocalAgentVM.Plugins.Where(p => p.Descriptor.IsIndicator).Chain().AsObservable();
            AvailableBotTraders = AlgoEnv.LocalAgentVM.Plugins.Where(p => p.Descriptor.IsTradeBot).Chain().AsObservable();

            AvailableIndicators.CollectionChanged += AvailableIndicators_CollectionChanged;
            AvailableBotTraders.CollectionChanged += AvailableBotTraders_CollectionChanged;

            this.isConnected = ClientModel.IsConnected.Value;
            ClientModel.Connected += Connection_Connected;
            ClientModel.Disconnected += Connection_Disconnected;
            ClientModel.Deinitializing += Client_Deinitializing;

            _subscription = ClientModel.Distributor.AddListener(OnRateUpdate, symbol.Name, 1);
            //subscription.NewQuote += ;

            _currentRateProp.Value = (IRateInfo)symbol.LastQuote;

            Func<bool> isReadyToStart = () => isConnected && !_isDisposed;
            Func<bool> isNotReadyToStart = () => !isReadyToStart();

            stateController.AddTransition(States.Idle, isReadyToStart, States.LoadingData);
            stateController.AddTransition(States.LoadingData, Events.Loaded, States.Online);
            stateController.AddTransition(States.LoadingData, Events.LoadFailed, States.Faulted);
            stateController.AddTransition(States.Online, () => isUpdateRequired || !isConnected || _isDisposed, States.Stopping);
            stateController.AddTransition(States.Faulted, () => isUpdateRequired || !isConnected || _isDisposed, States.Stopping);
            //stateController.AddTransition(States.Stopping, () => isUpdateRequired && isConnected, States.LoadingData);
            stateController.AddTransition(States.Stopping, Events.Stopped, isNotReadyToStart, States.Idle);
            stateController.AddTransition(States.Stopping, Events.Stopped, isReadyToStart, States.LoadingData);

            stateController.OnEnter(States.LoadingData, () => Update(CancellationToken.None));
            stateController.OnEnter(States.Online, StartIndicators);
            stateController.OnEnter(States.Stopping, StopIndicators);

            stateController.StateChanged += (o, n) => logger.Debug("Chart [" + Model.Name + "] " + o + " => " + n);
        }

        protected LocalAlgoAgent Agent => AlgoEnv.LocalAgent;
        protected SymbolInfo Model { get; private set; }
        protected TraderClientModel ClientModel => Agent.ClientModel;
        protected AlgoEnvironment AlgoEnv { get; }
        protected ConnectionModel.Handler Connection { get { return ClientModel.Connection; } }
        protected VarList<IRenderableSeriesViewModel> SeriesCollection { get { return seriesCollection; } }

        public Feed.Types.Timeframe TimeFrame
        {
            get => _timeframe;
            set
            {
                if (_timeframe != value)
                {
                    _timeframe = value;
                    TimeframeChanged?.Invoke();
                }
            }
        }

        public abstract ITimeVectorRef TimeSyncRef { get; }
        public IVarList<IRenderableSeriesViewModel> DataSeriesCollection { get { return seriesCollection; } }
        public IObservableList<AlgoPluginViewModel> AvailableIndicators { get; private set; }
        public bool HasAvailableIndicators => AvailableIndicators.Count() > 0;
        public IObservableList<AlgoPluginViewModel> AvailableBotTraders { get; private set; }
        public bool HasAvailableBotTraders => AvailableBotTraders.Count() > 0;
        public IVarList<IndicatorModel> Indicators { get { return indicators; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public string SymbolCode { get { return Model.Name; } }
        public Var<IRateInfo> CurrentRate => _currentRateProp.Var;
        public bool IsIndicatorsOnline => isIndicatorsOnline;

        public event System.Action TimeframeChanged;

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
                _timeAxis.Value = value.CreateAxis();
                CreateXAxisBinging(TimeAxis.Value);
            }
        }

        public void CreateXAxisBinging(AxisBase timeAxis)
        {
            Binding cursorTextFormatBinding = new Binding(nameof(DateAxisLabelFormat));
            cursorTextFormatBinding.Source = this;
            cursorTextFormatBinding.Mode = BindingMode.TwoWay;
            timeAxis.SetBinding(AxisBase.CursorTextFormattingProperty, cursorTextFormatBinding);
        }

        public Var<AxisBase> TimeAxis => _timeAxis.Var;

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

        public void AddIndicator(PluginConfig config)
        {
            var indicator = CreateIndicator(config);
            indicators.Add(indicator);
            Agent.IdProvider.RegisterPluginId(indicator.InstanceId);
        }

        public void RemoveIndicator(IndicatorModel i)
        {
            if (indicators.Remove(i))
            {
                Agent.IdProvider.UnregisterPlugin(i.InstanceId);
                i.Dispose();
            }
        }

        protected long GetNextIndicatorId()
        {
            return indicatorNextId++;
        }

        protected abstract void ClearData();
        protected abstract void UpdateSeries();
        protected abstract Task LoadData(CancellationToken cToken);
        protected abstract IndicatorModel CreateIndicator(PluginConfig config);
        protected abstract void ApplyUpdate(QuoteInfo update);

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
                updateQueue = new List<QuoteInfo>();
                await LoadData(cToken);
                ApplyQueue();
                stateController.PushEvent(Events.Loaded);
            }
            catch (Exception ex)
            {
                var fex = ex.FlattenAsPossible();

                if (fex is InteropException)
                    logger.Info("Chart[" + Model.Name + "] : load failed due disconnection.");
                else
                    logger.Error("Update ERROR " + ex);
                stateController.PushEvent(Events.LoadFailed);
            }

            this.IsLoading = false;
        }

        protected void InitBoundaries(int count, DateTime startDate, DateTime endDate)
        {
            TimelineStart = startDate;
            TimelineEnd = endDate;
            //Navigator.Init(count, startDate, endDate);
            Navigator.OnDataLoaded(count, startDate, endDate);
        }

        protected void ExtendBoundaries(int count, DateTime endDate)
        {
            TimelineEnd = endDate;
            //Navigator.Extend(count, endDate);
        }

        private void Connection_Disconnected()
        {
            Disconnected?.Invoke();
        }

        private void Connection_Connected()
        {
            stateController.ModifyConditions(() => isConnected = true);
            Connected?.Invoke();
        }

        private Task Client_Deinitializing(object sender, CancellationToken cancelToken)
        {
            return stateController.ModifyConditionsAndWait(() => isConnected = false, States.Idle);
        }

        private void AvailableIndicators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange("HasAvailableIndicators");
        }

        private void AvailableBotTraders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange("HasAvailableBotTraders");
        }

        public async void Dispose()
        {
            if (!_isDisposed)
            {
                try
                {
                    await stateController.ModifyConditionsAndWait(() => _isDisposed = true, States.Idle);

                    AvailableIndicators.CollectionChanged -= AvailableIndicators_CollectionChanged;
                    AvailableBotTraders.CollectionChanged -= AvailableBotTraders_CollectionChanged;
                    AvailableIndicators.Dispose();
                    AvailableBotTraders.Dispose();

                    ClientModel.Connected -= Connection_Connected;
                    ClientModel.Disconnected -= Connection_Disconnected;
                    ClientModel.Deinitializing -= Client_Deinitializing;
                    _subscription.Dispose();

                    logger.Debug("Chart[" + Model.Name + "] disposed!");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Dispose failed: " + ex.Message);
                }
            }
        }

        protected virtual void OnRateUpdate(QuoteInfo tick)
        {
            if (stateController.Current == States.LoadingData)
                updateQueue.Add(tick);
            else if (stateController.Current == States.Online)
                ApplyUpdate(tick);

            _currentRateProp.Value = tick;
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

        #region IAlgoPluginHost

        AxisBase IPluginDataChartModel.CreateXAxis()
        {
            var axis = Navigator.CreateAxis();
            CreateXAxisBinging(axis);
            return axis;
        }

        void IAlgoPluginHost.Lock()
        {
            ParamsLocked();
        }

        void IAlgoPluginHost.Unlock()
        {
            ParamsUnlocked();
        }

        void IAlgoPluginHost.EnqueueStartAction(System.Action action)
        {
            ((IAlgoPluginHost)AlgoEnv.LocalAgent).EnqueueStartAction(action);
        }

        ITradeExecutor IAlgoPluginHost.GetTradeApi()
        {
            return ClientModel.TradeApi;
        }

        ITradeHistoryProvider IAlgoPluginHost.GetTradeHistoryApi()
        {
            return ClientModel.TradeHistory.AlgoAdapter;
        }

        string IAlgoPluginHost.GetConnectionInfo()
        {
            return $"account {ClientModel.Connection.CurrentLogin} on {ClientModel.Connection.CurrentServer} using {ClientModel.Connection.CurrentProtocol}";
        }

        public virtual void InitializePlugin(ExecutorConfig config)
        {
            config.InitPriorityInvokeStrategy();
            config.InitTimeSpanBuffering(TimelineStart, DateTime.Now + TimeSpan.FromDays(100));
            //runtime.AccInfoProvider = new PluginTradeInfoProvider(ClientModel.Cache, new DispatcherSync());
        }

        bool IExecStateObservable.IsStarted { get { return isIndicatorsOnline; } }

        public event System.Action ParamsChanged = delegate { };
        public event System.Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return Task.CompletedTask; };
        public event System.Action Connected;
        public event System.Action Disconnected;

        #endregion

        #region IAlgoSetupContext

        Feed.Types.Timeframe IAlgoSetupContext.DefaultTimeFrame => TimeFrame;

        ISetupSymbolInfo IAlgoSetupContext.DefaultSymbol => new SymbolToken(SymbolCode);

        MappingKey IAlgoSetupContext.DefaultMapping => MappingDefaults.DefaultBarToBarMapping.Key;

        #endregion
    }
}
