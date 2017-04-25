using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class PluginBuilder : IPluginContext, IPluginSubscriptionHandler, EnvironmentInfo, IHelperApi
    {
        private Dictionary<object, IDataBuffer> inputBuffers = new Dictionary<object, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;
        private bool isCurrenciesInitialized;
        private volatile bool isStopped;
        private StatusApiImpl statusApi = new StatusApiImpl();
        private PluginLoggerAdapter logAdapter = new PluginLoggerAdapter();
        private SynchronizationContextAdapter syncContext = new SynchronizationContextAdapter();

        internal PluginBuilder(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            marketData = new MarketDataImpl(this, this);
            Account = new AccountEntity(this);
            Symbols = new SymbolsCollection(this);
            Currencies = new CurrenciesCollection();

            PluginProxy = PluginAdapter.Create(descriptor, this);

            Diagnostics = Null.Diagnostics;

            syncContext.OnAsyncAction = OnPluginThread;

            //OnException = ex => Logger.OnError("Exception: " + ex.Message, ex.ToString());
        }

        internal PluginAdapter PluginProxy { get; private set; }

        public string MainSymbol { get; set; }
        public SymbolsCollection Symbols { get; private set; }
        public CurrenciesCollection Currencies { get; private set; }
        public int DataSize { get { return PluginProxy.Coordinator.VirtualPos; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AccountEntity Account { get; private set; }
        public Action AccountDataRequested { get; set; }
        public Action SymbolDataRequested { get; set; }
        public Action CurrencyDataRequested { get; set; }
        public DiagnosticInfo Diagnostics { get; set; }
        public ITradeApi TradeApi { get; set; }
        public IPluginLogger Logger { get { return logAdapter.Logger; } set { logAdapter.Logger = value; } }
        public Action<string, int> OnSubscribe { get; set; }
        public Action<string> OnUnsubscribe { get; set; }
        public Action<Exception> OnException { get; set; }
        public Action<Action> OnAsyncAction { get; set; }
        public Action OnExit { get; set; }
        public string Status { get { return statusApi.Status; } }
        public string DataFolder { get; set; }
        public string BotDataFolder { get; set; }

        public Action<string> StatusUpdated { get { return statusApi.Updated; } set { statusApi.Updated = value; } }

        public IReadOnlyDictionary<object, IDataBuffer> DataBuffers { get { return inputBuffers; } }

        public InputBuffer<T> GetBuffer<T>(string bufferId)
        {
            if (inputBuffers.ContainsKey(bufferId))
                return (InputBuffer<T>)inputBuffers[bufferId];

            InputBuffer<T> buffer = new InputBuffer<T>(PluginProxy.Coordinator);
            inputBuffers.Add(bufferId, buffer);
            return buffer;
        }

        public InputBuffer<T> GetBuffer<T>(object bufferId)
        {
            if (inputBuffers.ContainsKey(bufferId))
                return (InputBuffer<T>)inputBuffers[bufferId];

            InputBuffer<T> buffer = new InputBuffer<T>(PluginProxy.Coordinator);
            inputBuffers.Add(bufferId, buffer);
            return buffer;
        }

        public void SetParameter(string paramName, object value)
        {
            PluginProxy.SetParameter(paramName, value);
        }

        public InputBuffer<BarEntity> GetBarBuffer(string bufferId)
        {
            return GetBuffer<BarEntity>(bufferId);
        }

        public InputBuffer<BarEntity> GetBarBuffer(object bufferId)
        {
            return GetBuffer<BarEntity>(bufferId);
        }

        public IReaonlyDataBuffer GetOutput(string outputName)
        {
            var outputProxy = PluginProxy.GetOutput(outputName);
            return (IReaonlyDataBuffer)outputProxy.Buffer;
        }

        public OutputBuffer<T> GetOutput<T>(string outputName)
        {
            var outputProxy = PluginProxy.GetOutput(outputName);
            return (OutputBuffer<T>)outputProxy.Buffer;
        }

        public void MapBarInput<TVal>(string inputName, object bufferId, Func<BarEntity, TVal> selector)
        {
            MapInput(inputName, bufferId, selector);
        }

        public void MapBarInput(string inputName, object bufferId)
        {
            MapInput<BarEntity, Api.Bar>(inputName, bufferId, b => b);
        }

        public void MapInput<TSrc, TVal>(string inputName, object bufferId, Func<TSrc, TVal> selector)
        {
            var buffer = GetBuffer<TSrc>(bufferId);
            var input = PluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer<TSrc, TVal>(buffer, selector);
        }

        public void MapInput<TSrc, TVal>(string inputName, object bufferId1, object bufferId2, Func<TSrc, TSrc, TVal> selector)
        {
            var buffer1 = GetBuffer<TSrc>(bufferId1);
            var buffer2 = GetBuffer<TSrc>(bufferId2);
            var input = PluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer2<TSrc, TVal>(buffer1, buffer2, selector);
        }

        public void MapInput<T>(string inputName, object bufferId)
        {
            var buffer = GetBuffer<T>(bufferId);
            var input = PluginProxy.GetInput<T>(inputName);

            input.Buffer = buffer;
        }

        public void SetStopped()
        {
            isStopped = true;
        }

        //protected void InvokeCalculate(bool isUpdate)
        //{

        //}

        private void OnPluginException(Exception ex)
        {
            Logger.OnError(ex);
            OnException?.Invoke(ex);
        }

        private void OnPluginThread(SendOrPostCallback callback, object state)
        {
            OnAsyncAction(() => callback(state));
        }

        private Task OnPluginThreadAsync(Action action)
        {
            TaskCompletionSource<object> waithandler = new TaskCompletionSource<object>();
            OnAsyncAction(() =>
            {
                try
                {
                    action();
                    waithandler.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    waithandler.SetException(ex);
                }
            });
            return waithandler.Task;
        }

        #region IPluginContext

        FeedProvider IPluginContext.Feed => marketData;

        AccountDataProvider IPluginContext.AccountData
        {
            get
            {
                if (!isAccountInitialized)
                {
                    isAccountInitialized = true;
                    AccountDataRequested?.Invoke();
                }
                return Account;
            }
        }

        SymbolProvider IPluginContext.Symbols
        {
            get
            {
                if (!isSymbolsInitialized)
                {
                    isSymbolsInitialized = true;
                    SymbolDataRequested?.Invoke();
                    Symbols.MainSymbolCode = MainSymbol;
                }
                return Symbols.SymbolProviderImpl;
            }
        }

        CurrencyList IPluginContext.Currencies
        {
            get
            {
                if (!isCurrenciesInitialized)
                {
                    isCurrenciesInitialized = true;
                    CurrencyDataRequested?.Invoke();
                }
                return Currencies.CurrencyListImp;
            }
        }

        TradeCommands IPluginContext.TradeApi
        {
            get
            {
                if (TradeApi == null)
                    return new NullTradeApi();
                return new TradeApiAdapter(TradeApi, Symbols.SymbolProviderImpl, Account, logAdapter);
            }
        }

        IPluginMonitor IPluginContext.Logger => logAdapter;
        StatusApi IPluginContext.StatusApi => statusApi;
        EnvironmentInfo IPluginContext.Environment => this;
        IHelperApi IPluginContext.Helper => this;
        bool IPluginContext.IsStopped => isStopped;

        void IPluginContext.OnExit()
        {
            Logger.OnExit();
            OnExit?.Invoke();
        }

        void IPluginContext.OnPluginThread(Action action)
        {
            if (AlgoPlugin.staticContext != null) // we are already on plugin thread!
                action();
            else
                OnPluginThreadAsync(action).Wait();
        }

        void IPluginContext.BeginOnPluginThread(Action action)
        {
            OnAsyncAction(action);
        }

        Task IPluginContext.OnPluginThreadAsync(Action action)
        {
            return OnPluginThreadAsync(action);
        }

        #endregion IPluginContext

        #region Invoke

        private void OnBeforeInvoke()
        {
        }

        private void OnAfterInvoke()
        {
            statusApi.Apply();
        }

        internal void InvokePluginMethod(Action invokeAction)
        {
            var ex = InvokeMethod(invokeAction);
            if (ex != null)
                OnPluginException(ex);
        }

        private Exception InvokeMethod(Action invokeAction)
        {
            Exception pluginException = null;

            OnBeforeInvoke();
            var oldContext = SynchronizationContext.Current;
            AlgoPlugin.staticContext = this;
            SynchronizationContext.SetSynchronizationContext(syncContext);
            try
            {
                invokeAction();
            }
            catch (Exception ex)
            {
                pluginException = ex;
            }
            AlgoPlugin.staticContext = null;
            SynchronizationContext.SetSynchronizationContext(oldContext);
            OnAfterInvoke();

            return pluginException;
        }

        internal void InvokeCalculate(bool isUpdate)
        {
            InvokePluginMethod(() => PluginProxy.InvokeCalculate(isUpdate));
        }

        internal void InvokeOnStart()
        {
            InvokePluginMethod(PluginProxy.InvokeOnStart);
            Logger.OnStart();
        }

        internal void InvokeOnStop()
        {
            InvokePluginMethod(PluginProxy.InvokeOnStop);
            Logger.OnStop();
        }

        internal void InvokeInit()
        {
            InvokePluginMethod(PluginProxy.InvokeInit);
            Logger.OnInitialized();
        }

        internal void InvokeOnQuote(Quote quote)
        {
            InvokePluginMethod(() => PluginProxy.InvokeOnQuote(quote));
        }

        internal void InvokeAsyncAction(Action asyncAction)
        {
            InvokePluginMethod(asyncAction);
        }

        internal Task InvokeAsyncStop()
        {
            Task result = null;
            InvokePluginMethod(() => result = PluginProxy.InvokeAsyncStop());
            return result;
        }

        #endregion

        internal void StartBatch()
        {
            PluginProxy.Coordinator.BeginBatch();
        }

        internal void StopBatch()
        {
            PluginProxy.Coordinator.EndBatch();
        }

        internal void IncreaseVirtualPosition()
        {
            PluginProxy.Coordinator.Extend();
        }

        internal void TruncateBuffers(int bySize)
        {
            PluginProxy.Coordinator.Truncate(bySize);
        }

        void IPluginSubscriptionHandler.Subscribe(string smbCode, int depth)
        {
            if (OnSubscribe != null)
                OnSubscribe(smbCode, depth);
        }

        void IPluginSubscriptionHandler.Unsubscribe(string smbCode)
        {
            if (OnUnsubscribe != null)
                OnUnsubscribe(smbCode);
        }

        #region IHelperApi

        string IHelperApi.FormatPrice(double price, int digits)
        {
            return price.ToString("F" + digits);
        }

        string IHelperApi.FormatPrice(double price, Symbol symbolInfo)
        {
            return price.ToString("F" + symbolInfo.Digits);
        }

        Quote IHelperApi.CreateQuote(string symbol, DateTime time, IEnumerable<BookEntry> bids, IEnumerable<BookEntry> asks)
        {
            QuoteEntity entity = new QuoteEntity();
            entity.Symbol = symbol;
            entity.Time = time;

            if (bids != null)
            {
                entity.BidList = bids.ToArray();
                entity.Bid = bids.Max(e => e.Price);
            }
            else
            {
                entity.BidList = new BookEntry[0];
                entity.Bid = double.NaN;
            }

            if (asks != null)
            {
                entity.AskList = asks.ToArray();
                entity.Ask = asks.Min(e => e.Price);
            }
            else
            {
                entity.AskList = new BookEntry[0];
                entity.Bid = double.NaN;
            }

            return entity;
        }

        BookEntry IHelperApi.CreateBookEntry(double price, double volume)
        {
            return new BookEntryEntity() { Price = price, Volume = volume };
        }

        IEnumerable<BookEntry> IHelperApi.CreateBook(IEnumerable<double> prices, IEnumerable<double> volumes)
        {
            if (prices == null || volumes == null)
                return new BookEntry[0];

            return prices.Zip(volumes, (p, v) => new BookEntryEntity() { Price = p, Volume = v });
        }

        double IHelperApi.RoundVolumeDown(double volume, Symbol symbolInfo)
        {
            double step = symbolInfo.TradeVolumeStep;
            double steps = System.Math.Truncate(volume / step);

            if (steps < symbolInfo.MinTradeVolume)
                return 0;

            return steps * step;
        }

        double IHelperApi.RoundVolumeUp(double volume, Symbol symbolInfo)
        {
            double step = symbolInfo.TradeVolumeStep;
            double steps = System.Math.Ceiling(volume / step);

            if (steps < symbolInfo.MinTradeVolume)
                return symbolInfo.MinTradeVolume;

            return steps * step;
        }

        #endregion

        private class MarketDataImpl : FeedProvider, CustomFeedProvider
        {
            private PluginBuilder builder;
            private BarSeriesProxy mainBars;
            private IPluginSubscriptionHandler subscribeHandler;

            public MarketDataImpl(PluginBuilder builder, IPluginSubscriptionHandler subscribeHandler)
            {
                this.builder = builder;
                this.subscribeHandler = subscribeHandler;
            }

            public BarSeries Bars
            {
                get
                {
                    if (mainBars == null)
                    {
                        mainBars = new BarSeriesProxy();
                        if (!string.IsNullOrEmpty(builder.MainSymbol))
                        {
                            IDataBuffer mainBuffer;
                            if (builder.inputBuffers.TryGetValue(builder.MainSymbol, out mainBuffer))
                            {
                                if (mainBuffer is InputBuffer<BarEntity>)
                                    mainBars.Buffer = new ProxyBuffer<BarEntity, Api.Bar>(b => b) { SrcBuffer = (InputBuffer<BarEntity>)mainBuffer };
                                else if (mainBuffer is InputBuffer<QuoteEntity>)
                                    mainBars.Buffer = new QuoteToBarAdapter((InputBuffer<QuoteEntity>)mainBuffer);
                            }
                        }

                        if (mainBars.Buffer == null)
                            mainBars.Buffer = new EmptyBuffer<Bar>();
                    }

                    return mainBars;
                }
            }

            public CustomFeedProvider CustomCommds { get { return this; } }

            public BarSeries GetBars(string symbolCode)
            {
                throw new NotImplementedException();
            }

            public BarSeries GetBars(string symbolCode, TimeFrames timeFrame)
            {
                throw new NotImplementedException();
            }

            public BarSeries GetBars(string symbolCode, TimeFrames timeFrame, DateTime from, DateTime to)
            {
                throw new NotImplementedException();
            }

            public QuoteSeries GetLevel2(string symbolCode)
            {
                throw new NotImplementedException();
            }

            public QuoteSeries GetLevel2(string symbolCode, DateTime from, DateTime to)
            {
                throw new NotImplementedException();
            }

            public QuoteSeries GetQuotes(string symbolCode)
            {
                throw new NotImplementedException();
            }

            public QuoteSeries GetQuotes(string symbolCode, DateTime from, DateTime to)
            {
                throw new NotImplementedException();
            }

            public void Subscribe(string symbol, int depth = 1)
            {
                subscribeHandler.Subscribe(symbol, depth);
            }

            public void Unsubscribe(string symbol)
            {
                subscribeHandler.Unsubscribe(symbol);
            }
        }

        private class StatusApiImpl : StatusApi
        {
            private StringBuilder statusBuilder = new StringBuilder();
            private bool hasChanges;
            
            public string Status { get; private set; }
            public Action<string> Updated { get; set; }

            public void Apply()
            {
                if (hasChanges)
                {
                    hasChanges = false;
                    Status = statusBuilder.ToString();
                    Updated?.Invoke(Status);
                    statusBuilder.Clear();
                }
            }

            void StatusApi.Flush()
            {
                Apply();
            }

            void StatusApi.Write(string str)
            {
                statusBuilder.Append(str);
                hasChanges = true;
            }

            void StatusApi.Write(string str, object[] strParams)
            {
                statusBuilder.AppendFormat(str, strParams);
                hasChanges = true;
            }

            void StatusApi.WriteLine(string str)
            {
                statusBuilder.Append(str).AppendLine();
                hasChanges = true;
            }

            void StatusApi.WriteLine(string str, object[] strParams)
            {
                statusBuilder.AppendFormat(str, strParams).AppendLine();
                hasChanges = true;
            }

            public void WriteLine()
            {
                statusBuilder.AppendLine();
                hasChanges = true;
            }

            public void Clear()
            {
                statusBuilder.Clear();
                hasChanges = true;
            }
        }
    }
}
