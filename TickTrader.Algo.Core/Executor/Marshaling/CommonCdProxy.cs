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

            _acc.OrderUpdated += Acc_OrderUpdated;
            _acc.BalanceUpdated += Acc_BalanceUpdated;
            _acc.PositionUpdated += Acc_PositionUpdated;
        }

        public override void Dispose()
        {
            base.Dispose();

            _acc.OrderUpdated -= Acc_OrderUpdated;
            _acc.BalanceUpdated -= Acc_BalanceUpdated;
            _acc.PositionUpdated -= Acc_PositionUpdated;
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

        public IEnumerable<Domain.SymbolInfo> GetSymbolMetadata()
        {
            return _meta.GetSymbolMetadata();
        }

        #endregion

        #region IAccountInfoProvider

        public event Action<Domain.OrderExecReport> OrderUpdated;
        public event Action<Domain.BalanceOperation> BalanceUpdated;
        public event Action<Domain.PositionExecReport> PositionUpdated;

        public Domain.AccountInfo GetAccountInfo()
        {
            return _acc.GetAccountInfo();
        }

        public List<Domain.OrderInfo> GetOrders()
        {
            return _acc.GetOrders();
        }

        public List<Domain.PositionInfo> GetPositions()
        {
            return _acc.GetPositions();
        }

        public void SyncInvoke(Action action)
        {
            _acc.SyncInvoke(action);
        }

        private void Acc_BalanceUpdated(Domain.BalanceOperation rep)
        {
            BalanceUpdated?.Invoke(rep);
        }

        private void Acc_OrderUpdated(Domain.OrderExecReport rep)
        {
            OrderUpdated?.Invoke(rep);
        }

        private void Acc_PositionUpdated(Domain.PositionExecReport rep)
        {
            PositionUpdated?.Invoke(rep);
        }

        #endregion
    }
}
