using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using Bo = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class TradeHistoryEmulator :  TradeHistory
    {
        private List<TradeReportAdapter> _history = new List<TradeReportAdapter>();
        private TimeKeyGenerator _idGenerator = new TimeKeyGenerator();

        public TradeReportAdapter Create(DateTime time, SymbolAccessor smb, TradeExecActions action, Bo.TradeTransReasons reason)
        {
            var report =  TradeReportAdapter.Create(_idGenerator.NextKey(time), smb, action, reason);
            _history.Add(report);
            return report;
        }

        public void Reset()
        {
            _idGenerator.Reset();
            _history = new List<TradeReportAdapter>();
        }

        #region TradeHistory implementation

        IEnumerable<TradeReport> TradeHistory.Get(bool skipCancelOrders)
        {
            return GetInternal(skipCancelOrders);
        }

        IAsyncEnumerator<TradeReport> TradeHistory.GetAsync(bool skipCancelOrders)
        {
            return GetInternal(skipCancelOrders).SimulateAsync();
        }

        IEnumerator<TradeReport> IEnumerable<TradeReport>.GetEnumerator()
        {
            return GetInternal(false).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetInternal(false).GetEnumerator();
        }

        IEnumerable<TradeReport> TradeHistory.GetRange(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return GetRangeInternal(from, to, skipCancelOrders);
        }

        IEnumerable<TradeReport> TradeHistory.GetRange(DateTime to, bool skipCancelOrders)
        {
            return GetRangeInternal(DateTime.MinValue, to, skipCancelOrders);
        }

        IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return GetRangeInternal(from, to, skipCancelOrders).SimulateAsync();
        }

        IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime to, bool skipCancelOrders)
        {
            return GetRangeInternal(DateTime.MinValue, to, skipCancelOrders).SimulateAsync();
        }

        #endregion

        private IEnumerable<TradeReport> GetInternal(bool skipCancelOrders)
        {
            if (skipCancelOrders)
                return SkipCancelReports(_history);
            return _history;
        }

        private IEnumerable<TradeReport> GetRangeInternal(DateTime from, DateTime to, bool skipCancelOrders)
        {
            var startIndex = _history.BinarySearchBy(r => r.ReportTime, from, BinarySearchTypes.NearestHigher);

            if (startIndex < 0)
                yield break;

            for (int i = startIndex; i < _history.Count; i++)
            {
                var item = _history[i];

                if (item.ReportTime > to)
                    break;

                yield return _history[i];
            }
        }

        private static IEnumerable<TradeReport> SkipCancelReports(IEnumerable<TradeReportAdapter> src)
        {
            return src.Where(r => r.Entity.TradeTransactionReportType != TradeExecActions.OrderCanceled);
        }
    }
}
