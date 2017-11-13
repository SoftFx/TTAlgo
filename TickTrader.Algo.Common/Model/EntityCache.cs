using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class EntityCache : EntityBase
    {
        private IFeedServerApi _feedApi;
        private ITradeServerApi _tradeApi;
        private Property<AccountEntity> _accProperty;
        private DynamicDictionary<string, SymbolEntity> _symbols;
        private DynamicDictionary<string, CurrencyEntity> _currencies;
        private DynamicDictionary<string, OrderEntity> _orders;
        private DynamicDictionary<string, PositionEntity> _positions;

        public EntityCache()
        {
            _accProperty = AddProperty<AccountEntity>();
            _currencies = new DynamicDictionary<string, CurrencyEntity>();
            _symbols = new DynamicDictionary<string, SymbolEntity>();
            _orders = new DynamicDictionary<string, OrderEntity>();
            _positions = new DynamicDictionary<string, PositionEntity>();
        }

        public Var<AccountEntity> AccountInfo => _accProperty.Var;
        public IDynamicDictionarySource<string, SymbolEntity> Symbols => _symbols;
        public IDynamicDictionarySource<string, CurrencyEntity> Currencies => _currencies;
        public IDynamicDictionarySource<string, OrderEntity> TradeRecords => _orders;
        public IDynamicDictionarySource<string, PositionEntity> Positions => _positions;

        internal async Task Load(ITradeServerApi tradeApi, IFeedServerApi feedApi)
        {
            _tradeApi = tradeApi;
            _feedApi = feedApi;

            var getInfoTask = tradeApi.GetAccountInfo();
            var getSymbolsTask = feedApi.GetSymbols();
            var getCurrenciesTask = feedApi.GetCurrencies();
            var getOrdersTask = tradeApi.GetTradeRecords();
            //var getPositionsTask = tradeApi.GetPositions();

            //await Task.WhenAll(getInfoTask, getSymbolsTask, getCurrenciesTask, getOrdersTask, getPositionsTask);
            await Task.WhenAll(getInfoTask, getSymbolsTask, getCurrenciesTask, getOrdersTask);

            _accProperty.Value = getInfoTask.Result;

            _currencies.Clear();
            foreach (var c in getCurrenciesTask.Result)
                _currencies.Add(c.Name, c);

            _symbols.Clear();
            foreach (var s in getSymbolsTask.Result)
                _symbols.Add(s.Name, s);

            _orders.Clear();
            foreach (var o in getOrdersTask.Result)
                _orders.Add(o.OrderId, o);

            _positions.Clear();
            //foreach (var p in getPositionsTask.Result)
                //_positions.Add(p.Symbol, p);
        }

        internal void Close()
        {
            if (_tradeApi != null)
            {
                _accProperty.Value = null;
                _currencies.Clear();
                _symbols.Clear();

                _tradeApi = null;
                _feedApi = null;
            }
        }
    }
}
