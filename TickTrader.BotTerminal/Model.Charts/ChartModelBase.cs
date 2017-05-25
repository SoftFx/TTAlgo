﻿using Caliburn.Micro;
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
using SoftFX.Extended;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using SciChart.Charting.Model.ChartSeries;
using TickTrader.Algo.Common.Model;
using System.Collections.Specialized;

namespace TickTrader.BotTerminal
{
    public enum SelectableChartTypes { Candle, OHLC, Line, Mountain, DigitalLine, DigitalMountain, Scatter }
    public enum TimelineTypes { Real, Uniform }

    internal abstract class ChartModelBase : PropertyChangedBase, IAlgoSetupFactory, IDisposable, IAlgoPluginHost
    {
        private Logger logger;
        private enum States { Idle, LoadingData, Online, Stopping, Closed }
        private enum Events { Loaded, Stopped }

        private StateMachine<States> stateController = new StateMachine<States>(new DispatcherStateMachineSync());
        private DynamicList<IRenderableSeriesViewModel> seriesCollection = new DynamicList<IRenderableSeriesViewModel>();
        private DynamicList<IndicatorModel> indicators = new DynamicList<IndicatorModel>();
        private readonly AlgoEnvironment algoEnv;
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
        private IFeedSubscription subscription;

        public ChartModelBase(SymbolModel symbol, AlgoEnvironment algoEnv, TraderClientModel client)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.ClientModel = client;
            this.Model = symbol;
            this.algoEnv = algoEnv;
            this.Journal = algoEnv.BotJournal;

            AvailableIndicators = algoEnv.Repo.Indicators.OrderBy((k, v) => v.DisplayName).Chain().AsObservable();
            AvailableBotTraders = algoEnv.Repo.BotTraders.OrderBy((k, v) => v.DisplayName).Chain().AsObservable();

            AvailableIndicators.CollectionChanged += AvailableIndicators_CollectionChanged;
            AvailableBotTraders.CollectionChanged += AvailableBotTraders_CollectionChanged;

            this.isConnected = client.IsConnected;
            client.Connected += Connection_Connected;
            client.Disconnected += Connection_Disconnected;

            subscription = symbol.Subscribe();
            subscription.NewQuote += OnRateUpdate;

            CurrentAsk = symbol.CurrentAsk;
            CurrentBid = symbol.CurrentBid;

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
        protected TraderClientModel ClientModel { get; private set; }
        protected AlgoEnvironment AlgoEnv => algoEnv;
        protected ConnectionModel Connection { get { return ClientModel.Connection; } }
        protected DynamicList<IRenderableSeriesViewModel> SeriesCollection { get { return seriesCollection; } }

        public abstract Api.TimeFrames TimeFrame { get; }
        public IDynamicListSource<IRenderableSeriesViewModel> DataSeriesCollection { get { return seriesCollection; } }
        public IObservableListSource<PluginCatalogItem> AvailableIndicators { get; private set; }
        public bool HasAvailableIndicators => AvailableIndicators.Count() > 0;
        public IObservableListSource<PluginCatalogItem> AvailableBotTraders { get; private set; }
        public bool HasAvailableBotTraders => AvailableBotTraders.Count() > 0;
        public IDynamicListSource<IndicatorModel> Indicators { get { return indicators; } }
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

                Binding cursorTextFormatBinding = new Binding(nameof(DateAxisLabelFormat));
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

        public void RemoveIndicator(IndicatorModel i)
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
        protected abstract IndicatorModel CreateIndicator(PluginSetup setup);
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

        private void Connection_Disconnected()
        {
            stateController.ModifyConditions(() => isConnected = false);
        }

        private void Connection_Connected()
        {
            stateController.ModifyConditions(() => isConnected = true);
        }

        private void AvailableIndicators_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange("HasAvailableIndicators");
        }

        private void AvailableBotTraders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange("HasAvailableBotTraders");
        }

        public void Dispose()
        {
            AvailableIndicators.CollectionChanged -= AvailableIndicators_CollectionChanged;
            AvailableBotTraders.CollectionChanged -= AvailableBotTraders_CollectionChanged;
            AvailableIndicators.Dispose();
            AvailableBotTraders.Dispose();
            subscription.Dispose();
        }

        protected virtual void OnRateUpdate(Quote tick)
        {
            if (stateController.Current == States.LoadingData)
                updateQueue.Add(tick);
            else if (stateController.Current == States.Online)
                ApplyUpdate(tick);

            this.CurrentAsk = tick.GetNullableAsk();
            this.CurrentBid = tick.GetNullableBid();
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

        #region IAlgoPluginHost

        void IAlgoPluginHost.Lock()
        {
            ParamsLocked();
        }

        void IAlgoPluginHost.Unlock()
        {
            ParamsUnlocked();
        }

        ITradeApi IAlgoPluginHost.GetTradeApi()
        {
            return ClientModel.TradeApi;
        }

        public virtual void InitializePlugin(PluginExecutor plugin, string uniqueBotName)
        {
            plugin.InvokeStrategy = new PriorityInvokeStartegy();
            plugin.AccInfoProvider = ClientModel.Account;
            if (string.IsNullOrEmpty(uniqueBotName))
            {
                plugin.WorkingFolder = EnvService.Instance.AlgoWorkingFolder;
                plugin.BotWorkingFolder = EnvService.Instance.AlgoWorkingFolder;
            }
            else
            {
                plugin.WorkingFolder = Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(uniqueBotName));
                plugin.BotWorkingFolder = plugin.WorkingFolder;
                EnvService.Instance.EnsureFolder(plugin.BotWorkingFolder);
            }
        }

        public virtual void UpdatePlugin(PluginExecutor plugin)
        {
            plugin.TimeFrame = TimeFrame;
            plugin.MainSymbolCode = SymbolCode;
            plugin.TimePeriodStart = TimelineStart;
            plugin.TimePeriodEnd = DateTime.Now + TimeSpan.FromDays(100);
        }

        bool IAlgoPluginHost.IsStarted { get { return isIndicatorsOnline; } }

        public event System.Action ParamsChanged = delegate { };
        public event System.Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return CompletedTask.Default; };

        #endregion
    }
}
