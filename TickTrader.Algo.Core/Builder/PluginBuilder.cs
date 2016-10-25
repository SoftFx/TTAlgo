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
        private Dictionary<string, IDataBuffer> inputBuffers = new Dictionary<string, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;
        private StatusApiImpl statusApi = new StatusApiImpl();
        private PluginLoggerAdapter logAdapter = new PluginLoggerAdapter();
        private SynchronizationContextAdapter syncContext = new SynchronizationContextAdapter();

        internal PluginBuilder(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            marketData = new MarketDataImpl(this, this);
            Account = new AccountEntity(this);
            Symbols = new SymbolsCollection(this);

            PluginProxy = PluginAdapter.Create(descriptor, this);

            //OnException = ex => Logger.OnError("Exception: " + ex.Message, ex.ToString());
        }

        internal PluginAdapter PluginProxy { get; private set; }

        public string MainSymbol { get; set; }
        public SymbolsCollection Symbols { get; private set; }
        public int DataSize { get { return PluginProxy.Coordinator.VirtualPos; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AccountEntity Account { get; private set; }
        public Action AccountDataRequested { get; set; }
        public Action SymbolDataRequested { get; set; }
        public ITradeApi TradeApi { get; set; }
        public IPluginLogger Logger { get { return logAdapter.Logger; } set { logAdapter.Logger = value; } }
        public Action<string, int> OnSubscribe { get; set; }
        public Action<string> OnUnsubscribe { get; set; }
        public Action<Exception> OnException { get; set; }
        public Action<SendOrPostCallback, object> OnAsyncAction { get { return syncContext.OnAsyncAction; } set { syncContext.OnAsyncAction = value; } }
        public Action OnExit { get; set; }
        public string Status { get { return statusApi.Status; } }
        public string DataFolder { get; set; }

        public Action<string> StatusUpdated { get { return statusApi.Updated; } set { statusApi.Updated = value; } }

        public IReadOnlyDictionary<string, IDataBuffer> DataBuffers { get { return inputBuffers; } }

        public InputBuffer<T> GetBuffer<T>(string bufferId)
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

        public void MapBarInput<TVal>(string inputName, string bufferId, Func<BarEntity, TVal> selector)
        {
            MapInput(inputName, bufferId, selector);
        }

        public void MapBarInput(string inputName, string bufferId)
        {
            MapInput<BarEntity, Api.Bar>(inputName, bufferId, b => b);
        }

        public void MapInput<TSrc, TVal>(string inputName, string bufferId, Func<TSrc, TVal> selector)
        {
            var buffer = GetBuffer<TSrc>(bufferId);
            var input = PluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer<TSrc, TVal>(buffer, selector);
        }

        public void MapInput<T>(string inputName, string bufferId)
        {
            var buffer = GetBuffer<T>(bufferId);
            var input = PluginProxy.GetInput<T>(inputName);

            input.Buffer = buffer;
        }

        //protected void InvokeCalculate(bool isUpdate)
        //{

        //}

        private void OnPluginException(Exception ex)
        {
            Logger.OnError(ex);
            OnException?.Invoke(ex);
        }

        #region IPluginContext

        FeedProvider IPluginContext.GetFeed()
        {
            return marketData;
        }

        AccountDataProvider IPluginContext.GetAccountDataProvider()
        {
            if (!isAccountInitialized)
            {
                isAccountInitialized = true;
                AccountDataRequested?.Invoke();
            }
            return Account;
        }

        SymbolProvider IPluginContext.GetSymbolProvider()
        {
            if (!isSymbolsInitialized)
            {
                isSymbolsInitialized = true;
                SymbolDataRequested?.Invoke();
                Symbols.MainSymbolCode = MainSymbol;
            }
            return Symbols.SymbolProviderImpl;
        }

        IPluginMonitor IPluginContext.GetPluginLogger()
        {
            return logAdapter;
        }

        TradeCommands IPluginContext.GetTradeApi()
        {
            if (TradeApi == null)
                return new NullTradeApi();
            return new TradeApiAdapter(TradeApi, Symbols.SymbolProviderImpl, Account, logAdapter);
        }

        StatusApi IPluginContext.GetStatusApi()
        {
            return statusApi;
        }

        EnvironmentInfo IPluginContext.GetEnvironment()
        {
            return this;
        }

        IHelperApi IPluginContext.GetHelper()
        {
            return this;
        }

        void IPluginContext.OnExit()
        {
            Logger.OnExit();
            OnExit?.Invoke();
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
            OnBeforeInvoke();
            var oldContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(syncContext);
            try
            {
                invokeAction();
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
            SynchronizationContext.SetSynchronizationContext(oldContext);
            OnAfterInvoke();
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

        internal void InvokeOnQuote(QuoteEntity quote)
        {
            InvokePluginMethod(() => PluginProxy.InvokeOnQuote(quote));
        }

        #endregion

        internal void StartBatch()
        {
            PluginProxy.Coordinator.FireBeginBatch();
        }

        internal void StopBatch()
        {
            PluginProxy.Coordinator.FireEndBatch();
        }

        internal void IncreaseVirtualPosition()
        {
            PluginProxy.Coordinator.MoveNext();
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

            void StatusApi.Write(string str, object[] strParams)
            {
                statusBuilder.AppendFormat(str, strParams);
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
