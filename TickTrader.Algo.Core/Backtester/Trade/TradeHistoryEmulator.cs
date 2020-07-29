using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using Bo = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Core
{
    internal class TradeHistoryEmulator : CrossDomainObject,  TradeHistory
    {
        private List<TradeReportAdapter> _history = new List<TradeReportAdapter>();
        private TimeKeyGenerator _idGenerator = new TimeKeyGenerator();

        public TradeReportAdapter Create(DateTime time, SymbolAccessor smb, Domain.TradeReportInfo.Types.ReportType action, Domain.TradeReportInfo.Types.Reason reason)
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

        public IPagedEnumerator<Domain.TradeReportInfo> Marshal()
        {
            const int pageSize = 4000;

            return _history.Select(r => r.Entity).GetCrossDomainEnumerator(pageSize);
        }

        #region TradeHistory implementation

        IEnumerable<TradeReport> TradeHistory.Get(ThQueryOptions options)
        {
            return QueryAll(options);
        }

        IAsyncEnumerator<TradeReport> TradeHistory.GetAsync(ThQueryOptions options)
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

        IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime from, DateTime to, ThQueryOptions options)
        {
            return QueryRange(from, to, options).SimulateAsync();
        }

        IAsyncEnumerator<TradeReport> TradeHistory.GetRangeAsync(DateTime to, ThQueryOptions options)
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
                var startIndex = _history.BinarySearchBy(r => r.ReportTime, from, BinarySearchTypes.NearestHigher);

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
                return src.Where(r => r.Entity.ReportType != Domain.TradeReportInfo.Types.ReportType.OrderCanceled && r.Entity.ReportType != Domain.TradeReportInfo.Types.ReportType.OrderExpired);
            return src;
        }
    }
}
