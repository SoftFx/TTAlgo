using ActorSharp;
using ActorSharp.Lib;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ClientModel : ActorPart
    {
        private ConnectionModel _connection;
        //private DatatSnapshot _snapshot = new DatatSnapshot();
        private EntityCache _cache = new EntityCache(AccountModelOptions.None);
        private Dictionary<ActorRef, Channel<EntityCacheUpdate>> _tradeListeners = new Dictionary<ActorRef, Channel<EntityCacheUpdate>>();
        private Dictionary<ActorRef, Channel<QuoteEntity>> _feedListeners = new Dictionary<ActorRef, Channel<QuoteEntity>>();
        private InvokeHandler _syncUpdater;
        //private ActorEvent<TradeDataHandler> _tradeListeners = new ActorEvent<TradeDataHandler>();

        //protected QuoteDistributor Distributor { get; }

        //protected ITradeServerApi TradeProxy => Connection.TradeProxy;
        //protected IFeedServerApi FeedProxy => Connection.FeedProxy;

        private void Init(ConnectionOptions options)
        {
            _connection = new ConnectionModel(new ConnectionOptions());
            //Distributor = _symbols.Distributor;

            _connection.Initalizing += async (s,t) =>
            {
                _syncUpdater = new InvokeHandler(this);
                _connection.FeedProxy.Tick += _syncUpdater.OnUpdate;
                _connection.TradeProxy.ExecutionReport += _syncUpdater.OnUpdate;
                _connection.TradeProxy.PositionReport += _syncUpdater.OnUpdate;
                _connection.TradeProxy.TradeTransactionReport += _syncUpdater.OnUpdate;
                _connection.TradeProxy.BalanceOperation += _syncUpdater.OnUpdate;
            };

            _connection.Deinitalizing += async (s,t) =>
            {
                _syncUpdater.Close();
                _connection.TradeProxy.ExecutionReport -= _syncUpdater.OnUpdate;
                _connection.TradeProxy.PositionReport -= _syncUpdater.OnUpdate;
                _connection.TradeProxy.TradeTransactionReport -= _syncUpdater.OnUpdate;
                _connection.TradeProxy.BalanceOperation -= _syncUpdater.OnUpdate;
                _connection.FeedProxy.Tick -= _syncUpdater.OnUpdate;
                _syncUpdater = null;
            };
        }

        //public event Action<PositionEntity> PositionReportReceived;
        //public event Action<ExecutionReport> ExecutionReportReceived;
        ////public event Action<AccountInfoEventArgs> AccountInfoReceived;
        //public event Action<TradeReportEntity> TradeTransactionReceived;
        //public event Action<BalanceOperationReport> BalanceReceived;
        //public event Action<QuoteEntity> TickReceived;

        public class ControlHandler : Data
        {
            public ControlHandler(ConnectionOptions options) : base(SpawnLocal<ClientModel>())
            {
                Actor.Send(a => a.Init(options));
            }
        }

        public class Data : Handler<ClientModel>
        {
            public Data(Ref<ClientModel> actorRef, AccountModelOptions options = AccountModelOptions.None) : base(actorRef)
            {
                Cache = new EntityCache(options);
            }

            public ConnectionModel.Handler Connection { get; private set; }
            public Ref<Data> Ref { get; private set; }
            public EntityCache Cache { get; private set; }
            public IVarSet<string, SymbolModel> Symbols => Cache.Symbols;
            public IVarSet<string, CurrencyEntity> Currencies => Cache.Currencies;
            
            protected override void ActorInit()
            {
                Ref = this.GetRef();
            }

            public async Task Start()
            {
                var updateStream = Channel.NewInput<EntityCacheUpdate>();
                Connection = new ConnectionModel.Handler(await Actor.Call(a => a._connection.Ref));
                await Connection.Start();
                //var snapshot = await Actor.OpenChannel(updateStream, (a,c) => a.SyncListener(Ref, c));

                //ApplyUpdatesLoop(updateStream);
            }

            public async Task Stop()
            {
                await Actor.Call(a => a.UnsyncListener(Ref));
                await Connection.Stop();
            }

            private async void ApplyUpdatesLoop(Channel<EntityCacheUpdate> updateStream)
            {
                while (await updateStream.ReadNext())
                    updateStream.Current.Apply(Cache);
            }
        }

        private class InvokeHandler : BlockingHandler<ClientModel>
        {
            private BlockingChannel<object> _tradeChannel;
            private BlockingChannel<QuoteEntity> _feedChannel;

            public InvokeHandler(ClientModel client) : base(client.GetRef())
            {
                _tradeChannel =  OpenInputChannel<object>(100, (a, c) => a.UpdateTradeLoop(c));
                _feedChannel = OpenInputChannel<QuoteEntity>(100, (a, c) => a.UpdateQuoteLoop(c));
            }

            public void OnUpdate(PositionEntity position)
            {
                _tradeChannel.Write(position);
            }

            public void OnUpdate(ExecutionReport position)
            {
                _tradeChannel.Write(position);
            }

            public void OnUpdate(TradeReportEntity position)
            {
                _tradeChannel.Write(position);
            }

            public void OnUpdate(BalanceOperationReport position)
            {
                _tradeChannel.Write(position);
            }

            public void OnUpdate(QuoteEntity tick)
            {
                _feedChannel.Write(tick);
            }

            public void Close()
            {
                _tradeChannel.Close(true);
                _feedChannel.Close(true);
            }
        }

        private async Task SyncListener(ActorRef handleRef, Channel<EntityCacheUpdate> channel)
        {
            await channel.Write(_cache.GetSnapshotUpdate());
            await channel.ConfirmRead();
            _tradeListeners.Add(handleRef, channel);
        }

        private async Task UnsyncListener(ActorRef handlerRef)
        {
            var channel = _tradeListeners.GetOrDefault(handlerRef);
            if (channel != null)
            {
                _tradeListeners.Remove(handlerRef);
                channel.Clear();
                await channel.ConfirmRead();
                await channel.Close();
            }
        }

        private async Task LoadData()
        {
            var tradeApi = _connection.TradeProxy;
            var feedApi = _connection.FeedProxy;

            var getInfoTask = tradeApi.GetAccountInfo();
            var getSymbolsTask = feedApi.GetSymbols();
            var getCurrenciesTask = feedApi.GetCurrencies();
            var getOrdersTask = tradeApi.GetTradeRecords();

            await Task.WhenAll(getInfoTask, getSymbolsTask, getCurrenciesTask, getOrdersTask);

            var info = getInfoTask.Result;

            //_currencies.Clear();
            //foreach (var c in getCurrenciesTask.Result)
            //    _currencies.Add(c.Name, c);

            //_symbols.Clear();
            //foreach (var s in getSymbolsTask.Result)
            //    _symbols.Add(s.Name, s);

            //_orders.Clear();
            //foreach (var o in getOrdersTask.Result)
            //    _orders.Add(o.OrderId, o);

            //_positions.Clear();
            //if (_accProperty.Value.Type == Api.AccountTypes.Net)
            //{
            //    var fkdPositions = await tradeApi.GetPositions();
            //    foreach (var p in fkdPositions)
            //        _positions.Add(p.Symbol, p);
            //}
        }

        private async Task UnloadData()
        {
        }

        //public async Task Init()
        //{
        //    await Cache.Load(TradeProxy, FeedProxy);
        //    await _symbols.Initialize();
        //}

        //public async Task Deinit()
        //{
        //    await _symbols.Deinit();
        //    Cache.Close();
        //}

        private async void UpdateTradeLoop(Channel<object> updateChannel)
        {
            while (await updateChannel.ReadNext())
            {
                var update = UpdateCache(updateChannel.Current);

                if (update != null)
                {
                    foreach (var listener in _tradeListeners.Values)
                        await listener.Write(update);
                }
            }
        }

        private EntityCacheUpdate UpdateCache(object item)
        {
            return null;
        }

        private async void UpdateQuoteLoop(Channel<QuoteEntity> updateChannel)
        {
            while (await updateChannel.ReadNext())
            {
                var quote = updateChannel.Current;

                foreach (var listener in _feedListeners.Values)
                    await listener.Write(quote);
            }
        }

        //private class DatatSnapshot
        //{
        //    public DatatSnapshot()
        //    {
        //        Symbols = new Dictionary<string, SymbolModel>();
        //        Currencies = new Dictionary<string, CurrencyEntity>();
        //        Orders = new Dictionary<string, OrderEntity>();
        //        Positions = new Dictionary<string, PositionEntity>();
        //    }

        //    public DatatSnapshot(DatatSnapshot original)
        //    {
        //        Symbols = new Dictionary<string, SymbolModel>(original.Symbols);
        //        Currencies = new Dictionary<string, CurrencyEntity>(original.Currencies);
        //        Orders = new Dictionary<string, OrderEntity>(original.Orders);
        //        Positions = new Dictionary<string, PositionEntity>(original.Positions);
        //    }

        //    public Dictionary<string, SymbolModel> Symbols { get; }
        //    public Dictionary<string, CurrencyEntity> Currencies { get; }
        //    public Dictionary<string, OrderEntity> Orders { get; }
        //    public Dictionary<string, PositionEntity> Positions { get; }

        //    public DatatSnapshot Clone()
        //    {
        //        return new DatatSnapshot(this);
        //    }
        //}
    }
}
