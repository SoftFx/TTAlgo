using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class TradeHistoryEmulator : IHistoryProvider
    {
        private List<TradeReportAdapter> _history = new List<TradeReportAdapter>();
        private TimeKeyGenerator _idGenerator = new TimeKeyGenerator();

        public TradeReportAdapter Create(DateTime time, SymbolInfo smb, Domain.TradeReportInfo.Types.ReportType action, Domain.TradeReportInfo.Types.Reason reason)
        {
            return TradeReportAdapter.Create(_idGenerator.NextKey(time), smb, action, reason);
        }

        public void Add(TradeReportAdapter report)
        {
            _history.Add(report);
            OnReportAdded?.Invoke(report);
        }

        public int Count => _history.Count;

        public event Action<TradeReportAdapter> OnReportAdded;

        public void Reset()
        {
            _idGenerator.Reset();
            _history = new List<TradeReportAdapter>();
        }

        public IEnumerable<TradeReportInfo> LocalGetReports() => _history.Select(r => r.Info);

        #region TradeHistory implementation

        IEnumerable<TradeReport> TradeHistory.Get(ThQueryOptions options)
        {
            return QueryAll(options);
        }

        Api.IAsyncEnumerator<TradeReport> TradeHistory.GetAsync(ThQueryOptions options)
        {
            return QueryAll(options).SimulateAsync();
        }

        IEnumerator<TradeReport> IEnumerable<TradeReport>.GetEnumerator()
        {
            return QueryAll(ThQueryOptions.Backwards).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return QueryAll(ThQueryOptions.Backwards).GetEnumerator();
        }

        IEnumerable<TradeReport> TradeHistory.GetRange(DateTime from, DateTime to, ThQueryOptions options)
        {
            return QueryRange(from, to, options);
        }

        IEnumerable<TradeReport> TradeHistory.GetRange(DateTime to, ThQueryOptions options)
        {
            return QueryRange(DateTime.MinValue, to, options);
        }

        Api.IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options)
        {
            return QueryRange(from, to, options).SimulateAsync();
        }

        Api.IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime to, ThQueryOptions options)
        {
            return QueryRange(DateTime.MinValue, to, options).SimulateAsync();
        }

        #endregion

        private IEnumerable<TradeReport> QueryAll(ThQueryOptions options)
        {
            return SkipCancelReports(IterateAll(options), options);
        }

        private IEnumerable<TradeReport> QueryRange(DateTime from, DateTime to, ThQueryOptions options)
        {
            return SkipCancelReports(Iterate(from, to, options), options);
        }

        private IEnumerable<TradeReportAdapter> IterateAll(ThQueryOptions options)
        {
            if (options.HasFlag(ThQueryOptions.Backwards))
                return _history.IterateBackwards();
            else
                return _history;
        }

        private IEnumerable<TradeReportAdapter> Iterate(DateTime from, DateTime to, ThQueryOptions options)
        {
            if (options.HasFlag(ThQueryOptions.Backwards))
            {
                var startIndex = _history.BinarySearchBy(r => r.ReportTime, to, BinarySearchTypes.NearestLower);

                for (int i = startIndex; i >= 0; i--)
                {
                    var item = _history[i];

                    if (item.ReportTime < from)
                        break;

                    yield return _history[i];
                }
            }
            else
            {
                var startIndex = Math.Max(_history.BinarySearchBy(r => r.ReportTime, from, BinarySearchTypes.NearestHigher), 0);

                for (int i = startIndex; i < _history.Count; i++)
                {
                    var item = _history[i];

                    if (item.ReportTime > to)
                        break;

                    yield return _history[i];
                }
            }
        }

        private static IEnumerable<TradeReport> SkipCancelReports(IEnumerable<TradeReportAdapter> src, ThQueryOptions options)
        {
            if (options.HasFlag(ThQueryOptions.SkipCanceled))
                return src.Where(r => r.Info.ReportType != Domain.TradeReportInfo.Types.ReportType.OrderCanceled && r.Info.ReportType != Domain.TradeReportInfo.Types.ReportType.OrderExpired);
            return src;
        }

        IEnumerable<TriggerReport> TriggerHistory.Get(ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TriggerReport> TriggerHistory.GetRange(DateTime from, DateTime to, ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TriggerReport> TriggerHistory.GetRange(DateTime to, ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetAsync(ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        Api.IAsyncEnumerator<TriggerReport> TriggerHistory.GetRangeAsync(DateTime to, ThQueryOptions options)
        {
            throw new NotImplementedException();
        }

        IEnumerator<TriggerReport> IEnumerable<TriggerReport>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
