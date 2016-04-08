using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    /// <summary>
    /// Note: This class is not thread safe and designed to be used from one single thread
    /// </summary>
    public class IndicatorBuilder : IPluginDataProvider
    {
        private IndicatorAdapter pluginProxy;
        private Dictionary<string, IDataBuffer> inputBuffers = new Dictionary<string, IDataBuffer>();
        //private Dictionary<string, IDataBuffer> outputBuffers = new Dictionary<string, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isInitialized;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;

        public IndicatorBuilder(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            pluginProxy = PluginAdapter.CreateIndicator(descriptor.Id, this);
            marketData = new MarketDataImpl(this);
            Account = new AccountEntity();

            //PluginFactory2 factory = new PluginFactory2(descriptor.AlgoClassType, this);
        }

        public string MainSymbol { get; set; }
        public int DataSize { get { return pluginProxy.Coordinator.VirtualPos; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AccountEntity Account { get; private set; }
        public Action AccountDataRequested { get; set; }
        public Action SymbolDataRequested { get; set; }

        public InputBuffer<T> GetBuffer<T>(string bufferId)
        {
            if (inputBuffers.ContainsKey(bufferId))
                return (InputBuffer<T>)inputBuffers[bufferId];

            InputBuffer<T> buffer = new InputBuffer<T>(pluginProxy.Coordinator);
            inputBuffers.Add(bufferId, buffer);
            return buffer;
        }

        public void SetParameter(string paramName, object value)
        {
            pluginProxy.SetParameter(paramName, value);
        }

        public InputBuffer<BarEntity> GetBarSeries(string bufferId)
        {
            return GetBuffer<BarEntity>(bufferId);
        }

        public IReaonlyDataBuffer GetOutput(string outputName)
        {
            var outputProxy = pluginProxy.GetOutput(outputName);
            return (IReaonlyDataBuffer)outputProxy.Buffer;
        }

        public OutputBuffer<T> GetOutput<T>(string outputName)
        {
            var outputProxy = pluginProxy.GetOutput(outputName);
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
            var input = pluginProxy.GetInput<TVal>(inputName);

            input.Buffer = new ProxyBuffer<TSrc, TVal>(buffer, selector);
        }

        public void MapInput<T>(string inputName, string bufferId)
        {
            var buffer = GetBuffer<T>(bufferId);
            var input = pluginProxy.GetInput<T>(inputName);

            input.Buffer = buffer;
        }

        public void BuildNext(int count = 1)
        {
            BuildNext(count, CancellationToken.None);
        }

        public void BuildNext(int count, CancellationToken cToken)
        {
            LazyInit();
            for (int i = 0; i < count; i++)
            {
                if (cToken.IsCancellationRequested)
                    return;
                pluginProxy.Coordinator.MoveNext();
                pluginProxy.Calculate(false);
            }
        }

        public void RebuildLast()
        {
            LazyInit();
            pluginProxy.Calculate(true);
        }

        public void Reset()
        {
            pluginProxy.Coordinator.Reset();
        }

        private void LazyInit()
        {
            if (isInitialized)
                return;

            pluginProxy.Init();

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
            }
            throw new NotImplementedException(); 
        }

        private class MarketDataImpl : MarketDataProvider
        {
            private IndicatorBuilder builder;
            private BarSeriesProxy mainBars; 

            public MarketDataImpl(IndicatorBuilder builder)
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
