using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class CommonCdProxy : CrossDomainObject, IAccountInfoProvider, IPluginMetadata, ITradeHistoryProvider
    {
        private IAccountInfoProvider _acc;
        private IPluginMetadata _meta;
        private ITradeHistoryProvider _tHistory;

        public CommonCdProxy(IAccountInfoProvider acc, IPluginMetadata meta, ITradeHistoryProvider tHistory)
        {
            _acc = acc;
            _meta = meta;
            _tHistory = tHistory;
        }

        #region ITradeHistoryProvider

        public IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(ThQueryOptions options)
        {
            return _tHistory.GetTradeHistory(options);
        }

        public IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime from, DateTime to, ThQueryOptions options)
        {
            return _tHistory.GetTradeHistory(from, to, options);
        }

        public IAsyncCrossDomainEnumerator<TradeReportEntity> GetTradeHistory(DateTime to, ThQueryOptions options)
        {
            return _tHistory.GetTradeHistory(to, options);
        }

        #endregion

        #region IPluginMetadata

        public IEnumerable<CurrencyEntity> GetCurrencyMetadata()
        {
            return _meta.GetCurrencyMetadata();
        }

        public IEnumerable<SymbolEntity> GetSymbolMetadata()
        {
            return _meta.GetSymbolMetadata();
        }

        #endregion

        #region IAccountInfoProvider

        public AccountEntity AccountInfo => _acc.AccountInfo;

        public event Action<OrderExecReport> OrderUpdated;
        public event Action<BalanceOperationReport> BalanceUpdated;

        public List<OrderEntity> GetOrders()
        {
            return _acc.GetOrders();
        }

        public IEnumerable<PositionExecReport> GetPositions()
        {
            return _acc.GetPositions();
        }

        public void SyncInvoke(Action action)
        {
            _acc.SyncInvoke(action);
        }

        #endregion
    }
}
