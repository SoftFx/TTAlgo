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
        private IndicatorContext pluginProxy;
        private Dictionary<string, IDataBuffer> inputBuffers = new Dictionary<string, IDataBuffer>();
        //private Dictionary<string, IDataBuffer> outputBuffers = new Dictionary<string, IDataBuffer>();
        private MarketDataImpl marketData;
        private bool isInitialized;
        private bool isAccountInitialized;
        private bool isSymbolsInitialized;

        public IndicatorBuilder(AlgoPluginDescriptor descriptor)
        {
            Descriptor = descriptor;
            pluginProxy = PluginContext.CreateIndicator(descriptor.Id, this);
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
                pluginProxy.InvokeCalculate();
            }
        }

        public void RebuildLast()
        {
            LazyInit();
            pluginProxy.InvokeCalculate();
        }

        public void Reset()
        {
            pluginProxy.Coordinator.Reset();
        }

        private void LazyInit()
        {
            if (isInitialized)
                return;

            pluginProxy.InvokeInit();

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

    public class AccountEntity : AccountDataProvider
    {
        public AccountEntity()
        {
            Orders = new OrdersCollection();
        }

        public OrdersCollection Orders { get; private set; }
        public double Balance { get; set; }

        OrderList AccountDataProvider.Orders { get { return Orders.OrderListImpl; } }
        PositionList AccountDataProvider.Positions { get { throw new NotImplementedException(); } }
    }

    public class OrdersCollection
    {
        private OrdersFixture fixture = new OrdersFixture();

        internal OrderList OrderListImpl { get { return fixture; } }
        internal bool IsEnabled { get { return true; } }

        public void Add(OrderEntity entity)
        {
            fixture.Add(entity, IsEnabled);
        }

        public void Replace(OrderEntity entity)
        {
            fixture.Replace(entity, IsEnabled);
        }

        public void Remove(long orderId)
        {
            fixture.Remove(orderId, IsEnabled);
        }

        internal class OrdersFixture : OrderList
        {
            private Dictionary<long, OrderEntity> orders = new Dictionary<long, OrderEntity>();

            public Order this[long id]
            {
                get
                {
                    OrderEntity entity;
                    if (!orders.TryGetValue(id, out entity))
                        return null;
                    return entity;
                }
            }

            public void Add(OrderEntity entity, bool fireEvent)
            {
                orders.Add(entity.Id, entity);
                Opened(new OrderOpenedEventArgsImpl(entity));
            }

            public void Replace(OrderEntity entity, bool fireEvent)
            {
                var oldOrder = orders[entity.Id];
                orders[entity.Id] = entity;
                Modified(new OrderModifiedEventArgsImpl(entity, oldOrder));
            }

            public void Remove(long orderId, bool fireEvent)
            {
                var removedOrder = orders[orderId];
                Closed(new OrderClosedEventArgsImpl(removedOrder));
            }

            public event Action<OrderClosedEventArgs> Closed = delegate { };
            public event Action<OrderModifiedEventArgs> Modified = delegate { };
            public event Action<OrderOpenedEventArgs> Opened = delegate { };

            public IEnumerator<Order> GetEnumerator()
            {
                return this.orders.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.orders.Values.GetEnumerator();
            }
        }
    }

    public class OrderOpenedEventArgsImpl : OrderOpenedEventArgs
    {
        public OrderOpenedEventArgsImpl(Order order)
        {
            this.Order = order;
        }

        public Order Order { get; private set; }
    }

    public class OrderModifiedEventArgsImpl : OrderModifiedEventArgs
    {
        public OrderModifiedEventArgsImpl(Order newOrder, Order oldOrder)
        {
            this.NewOrder = newOrder;
            this.OldOrder = oldOrder;
        }

        public Order NewOrder { get; private set; }
        public Order OldOrder { get; private set; }
    }

    public class OrderClosedEventArgsImpl : OrderClosedEventArgs
    {
        public OrderClosedEventArgsImpl(Order order)
        {
            this.Order = order;
        }

        public Order Order { get; private set; }
    }

    public class OrderEntity : Order
    {
        public OrderEntity(long orderId)
        {
            this.Id = orderId;
        }

        public long Id { get; private set; }
        public decimal RemainingAmount { get; set; }
        public string Symbol { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderTypes Type { get; set; }
    }

    public interface IDataBuffer : IEnumerable
    {
        void Append(object item);
        object this[int index] { get; set; }
    }

    public interface IDataBuffer<T> : IEnumerable<T>
    {
        void Append(T item);
        T this[int index] { get; set; }
    }

    public interface IReaonlyDataBuffer : IEnumerable
    {
        object this[int index] { get; }
    }

    public interface IReaonlyDataBuffer<T> : IEnumerable<T>
    {
        T this[int index] { get; }
    }

    //public interface InputBuffersCollection<T> : IReadOnlyDictionary<string, InputBuffer<T>>
    //{
    //}

    //public interface OutputBuffersCollection<T> : IReadOnlyDictionary<string, InputBuffer<T>>
    //{
    //}
}
