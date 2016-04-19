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
    public abstract class PluginExecutor : IPluginDataProvider
    {
        private Dictionary<string, IDataBuffer> inputBuffers = new Dictionary<string, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isInitialized;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;

        public PluginExecutor(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            marketData = new MarketDataImpl(this);
            Account = new AccountEntity();
            Symbols = new SymbolsCollection();

            //PluginFactory2 factory = new PluginFactory2(descriptor.AlgoClassType, this);
        }

        internal abstract PluginAdapter PluginProxy { get; }

        public string MainSymbol { get; set; }
        public SymbolsCollection Symbols { get; private set; }
        public int DataSize { get { return PluginProxy.Coordinator.VirtualPos; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AccountEntity Account { get; private set; }
        public Action AccountDataRequested { get; set; }
        public Action SymbolDataRequested { get; set; }
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

        protected void LazyInit()
        {
            if (isInitialized)
                return;

            PluginProxy.Init();

            isInitialized = true;
        }

        MarketDataProvider IPluginDataProvider.GetMarketDataProvider()
        {
            return marketData;
        }

        AccountDataProvider IPluginDataProvider.GetAccountDataProvider()
        {
            if (!isAccountInitialized)
            {
                isAccountInitialized = true;
                if (AccountDataRequested != null)
                    AccountDataRequested();
            }
            return Account;
        }

        SymbolProvider IPluginDataProvider.GetSymbolProvider()
        {
            if (!isSymbolsInitialized)
            {
                isSymbolsInitialized = true;
                if (SymbolDataRequested != null)
                    SymbolDataRequested();
                Symbols.MainSymbolCode = MainSymbol;
            }
            return Symbols.SymbolProviderImpl;
        }

        private class MarketDataImpl : MarketDataProvider
        {
            private PluginExecutor builder;
            private BarSeriesProxy mainBars;

            public MarketDataImpl(PluginExecutor builder)
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

            public Leve2Series Level2
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public QuoteSeries Quotes
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

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

            public Leve2Series GetLevel2(string symbolCode)
            {
                throw new NotImplementedException();
            }

            public Leve2Series GetLevel2(string symbolCode, DateTime from, DateTime to)
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
