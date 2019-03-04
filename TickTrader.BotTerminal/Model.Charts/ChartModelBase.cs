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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.BotTerminal.Lib;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Common.Model;
using System.Collections.Specialized;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using Machinarium.Var;
using SM = Machinarium.State;

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
        private bool isCrosshairEnabled;
        private string dateAxisLabelFormat;
        private List<QuoteEntity> updateQueue;
        private IFeedSubscription subscription;
        private Property<QuoteEntity> _currentRateProp = new Property<QuoteEntity>();

        public ChartModelBase(SymbolModel symbol, AlgoEnvironment algoEnv)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            AlgoEnv = algoEnv;
            this.Model = symbol;

            AvailableIndicators = AlgoEnv.LocalAgentVM.Plugins.Where(p => p.Descriptor.Type == AlgoTypes.Indicator).AsObservable();
            AvailableBotTraders = AlgoEnv.LocalAgentVM.Plugins.Where(p => p.Descriptor.Type == AlgoTypes.Robot).AsObservable();

            AvailableIndicators.CollectionChanged += AvailableIndicators_CollectionChanged;
            AvailableBotTraders.CollectionChanged += AvailableBotTraders_CollectionChanged;

            this.isConnected = ClientModel.IsConnected.Value;
            ClientModel.Connected += Connection_Connected;
            ClientModel.Disconnected += Connection_Disconnected;
            ClientModel.Deinitializing += Client_Deinitializing;

            subscription = ClientModel.Distributor.Subscribe(symbol.Name);
            subscription.NewQuote += OnRateUpdate;

            _currentRateProp.Value = symbol.LastQuote;

            stateController.AddTransition(States.Idle, () => isConnected && !_isDisposed, States.LoadingData);
            stateController.AddTransition(States.LoadingData, Events.Loaded, States.Online);
            stateController.AddTransition(States.LoadingData, Events.LoadFailed, States.Faulted);
            stateController.AddTransition(States.Online, () => isUpdateRequired || !isConnected || _isDisposed, States.Stopping);
            stateController.AddTransition(States.Faulted, () => isUpdateRequired || !isConnected || _isDisposed, States.Stopping);
            //stateController.AddTransition(States.Stopping, () => isUpdateRequired && isConnected, States.LoadingData);
            stateController.AddTransition(States.Stopping, Events.Stopped, States.Idle);
            stateController.AddTransition(States.Stopping, () => isConnected && !_isDisposed, States.LoadingData);

            stateController.OnEnter(States.LoadingData, () => Update(CancellationToken.None));
            stateController.OnEnter(States.Online, StartIndicators);
            stateController.OnEnter(States.Stopping, StopIndicators);

            stateController.StateChanged += (o, n) => logger.Debug("Chart [" + Model.Name + "] " + o + " => " + n);
        }

        protected LocalAlgoAgent Agent => AlgoEnv.LocalAgent;
        protected SymbolModel Model { get; private set; }
        protected TraderClientModel ClientModel => Agent.ClientModel;
        protected AlgoEnvironment AlgoEnv { get; }
        protected ConnectionModel.Handler Connection { get { return ClientModel.Connection; } }
        protected VarList<IRenderableSeriesViewModel> SeriesCollection { get { return seriesCollection; } }

        public abstract Api.TimeFrames TimeFrame { get; }
        public abstract ITimeVectorRef TimeSyncRef { get; }
        public IVarList<IRenderableSeriesViewModel> DataSeriesCollection { get { return seriesCollection; } }
        public IObservableList<AlgoPluginViewModel> AvailableIndicators { get; private set; }
        public bool HasAvailableIndicators => AvailableIndicators.Count() > 0;
        public IObservableList<AlgoPluginViewModel> AvailableBotTraders { get; private set; }
        public bool HasAvailableBotTraders => AvailableBotTraders.Count() > 0;
        public IVarList<IndicatorModel> Indicators { get { return indicators; } }
        public IEnumerable<SelectableChartTypes> ChartTypes { get { return supportedChartTypes; } }
        public string SymbolCode { get { return Model.Name; } }
        public Var<QuoteEntity> CurrentRate => _currentRateProp.Var;
        public bool IsIndicatorsOnline => isIndicatorsOnline;

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

        public void AddIndicator(PluginConfig config)
        {
            var indicator = CreateIndicator(config);
            indicators.Add(indicator);
            Agent.IdProvider.RegisterIndicator(indicator);
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
        protected abstract void ApplyUpdate(QuoteEntity update);

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
                updateQueue = new List<QuoteEntity>();
                await StopEvent.InvokeAsync(this);
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
            Navigator.Init(count, startDate, endDate);
        }

        protected void ExtendBoundaries(int count, DateTime endDate)
        {
            TimelineEnd = endDate;
            Navigator.Extend(count, endDate);
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
                    subscription.Dispose();

                    logger.Debug("Chart[" + Model.Name + "] disposed!");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Dispose failed: " + ex.Message);
                }
            }
        }

        protected virtual void OnRateUpdate(QuoteEntity tick)
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
            var axis =  Navigator.CreateAxis();
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

        public virtual void InitializePlugin(PluginExecutor plugin)
        {
            plugin.InvokeStrategy = new PriorityInvokeStartegy();
            plugin.AccInfoProvider = new PluginTradeInfoProvider(ClientModel.Cache, new DispatcherSync());
        }

        public virtual void UpdatePlugin(PluginExecutor plugin)
        {
            plugin.TimeFrame = TimeFrame;
            plugin.MainSymbolCode = SymbolCode;
            plugin.InitTimeSpanBuffering(TimelineStart, DateTime.Now + TimeSpan.FromDays(100));
        }

        bool IExecStateObservable.IsStarted { get { return isIndicatorsOnline; } }

        public event System.Action ParamsChanged = delegate { };
        public event System.Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return CompletedTask.Default; };
        public event System.Action Connected;
        public event System.Action Disconnected;

        #endregion


        #region IAlgoSetupContext

        Api.TimeFrames IAlgoSetupContext.DefaultTimeFrame => TimeFrame;

        ISymbolInfo IAlgoSetupContext.DefaultSymbol => new SymbolToken(SymbolCode);

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

        #endregion
    }
}
