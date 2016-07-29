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
    public class PluginBuilder : IPluginContext, IPluginInvoker, IPluginSubscriptionHandler
    {
        private static readonly NullLogger nullLogger = new NullLogger();

        private Dictionary<string, IDataBuffer> inputBuffers = new Dictionary<string, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;

        internal PluginBuilder(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            marketData = new MarketDataImpl(this);
            Account = new AccountEntity();
            Symbols = new SymbolsCollection(this);
            Logger = nullLogger;

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
        public IPluginLogger Logger { get; set; }
        public Action<string, int> OnSubscribe { get; set; }
        public Action<string> OnUnsubscribe { get; set; }
        public Action<Exception> OnException { get; set; }
        public Action OnExit { get; set; }

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

        internal void InvokeUpdateNotification(Quote quote)
        {
            try
            {
                PluginProxy.InvokeOnQuote(quote);
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
        }

        protected void InvokeCalculate(bool isUpdate)
        {
            try
            {
                PluginProxy.InvokeCalculate(isUpdate);
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
        }

        private void OnPluginException(Exception ex)
        {
            OnException?.Invoke(ex);
        }

        MarketDataProvider IPluginContext.GetMarketDataProvider()
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
            return new PluginLoggerAdapter(Logger);
        }

        ITradeCommands IPluginContext.GetTradeApi()
        {
            if (TradeApi == null)
                return new NullTradeApi();
            return new TradeApiAdapter(TradeApi);
        }

        void IPluginInvoker.InvokeCalculate(bool isUpdate)
        {
            InvokeCalculate(isUpdate);
        }

        void IPluginInvoker.InvokeOnStart()
        {
            try
            {
                PluginProxy.InvokeOnStart();
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
            Logger.OnPrint("Bot started");
        }

        void IPluginInvoker.InvokeOnStop()
        {
            try
            {
                PluginProxy.InvokeOnStop();
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
            Logger.OnPrint("Bot stopped");
        }

        void IPluginInvoker.StartBatch()
        {
            PluginProxy.Coordinator.FireBeginBatch();
        }

        void IPluginInvoker.StopBatch()
        {
            PluginProxy.Coordinator.FireEndBatch();
        }

        void IPluginInvoker.IncreaseVirtualPosition()
        {
            PluginProxy.Coordinator.MoveNext();
        }

        void IPluginInvoker.InvokeInit()
        {
            try
            {
                PluginProxy.InvokeInit();
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
            Logger.OnInitialized();
        }

        void IPluginInvoker.InvokeOnQuote(QuoteEntity quote)
        {
            try
            {
                PluginProxy.InvokeOnQuote(quote);
            }
            catch (Exception ex)
            {
                OnPluginException(ex);
            }
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

        void IPluginContext.OnExit()
        {
            Logger.OnExit();
            OnExit?.Invoke();
        }

        private class MarketDataImpl : MarketDataProvider
        {
            private PluginBuilder builder;
            private BarSeriesProxy mainBars;

            public MarketDataImpl(PluginBuilder builder)
            {
                this.builder = builder;
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

            //public QuoteSeries Level2
            //{
            //    get
            //    {
            //        throw new NotImplementedException();
            //    }
            //}

            //public QuoteSeries Quotes
            //{
            //    get
            //    {
            //        throw new NotImplementedException();
            //    }
            //}

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
        }
    }
}
