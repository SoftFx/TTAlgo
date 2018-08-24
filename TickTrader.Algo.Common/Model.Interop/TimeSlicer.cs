using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public static class TimeSlicer
    {
        public static TimeSlicer<BarEntity> GetBarSlicer(int pageSize, DateTime? from = null, DateTime? to = null)
        {
            return new TimeSlicer<BarEntity>(pageSize, from, to, b => b.OpenTime, b => b.CloseTime);
        }

        public static TimeSlicer<QuoteEntity> GetQuoteSlicer(int pageSize, DateTime? from = null, DateTime? to = null)
        {
            return new TimeSlicer<QuoteEntity>(pageSize, from, to, b => b.Time);
        }
    }

    public class TimeSlicer<T>
    {
        private DateTime? _from;
        private DateTime? _to;
        //private DateTime? _i;
        private Func<T, DateTime> _getTime;
        private Func<T, DateTime> _getEndTime;
        private List<T> _cachedPage = new List<T>();
        private int _pageSize;
        private DateTime _lastItemTime;
        private int _count;
        
        public TimeSlicer(int pageSize, DateTime? from, DateTime? to, Func<T, DateTime> getItemTime, Func<T, DateTime> getItemEndTime = null)
        {
            _pageSize = pageSize;
            _from = from;
            _to = to;
            _getTime = getItemTime;
            _getEndTime = getItemEndTime;
        }

        public bool Write(T item)
        {
            var itemTime = _getTime(item);
            bool dupTime = _lastItemTime == itemTime;
            _lastItemTime = itemTime;

            _cachedPage.Add(item);

            // items with same time coordinate should be in same page
            // so page size may be slightly larger than requested
            return _cachedPage.Count >= _pageSize + 1 && !dupTime;
        }

        public Slice<T> CompleteSlice(bool last)
        {
            var sliceFrom = _count == 0 && _from != null ? _from.Value : _getTime(_cachedPage.First());
            _count++;

            if (!last)
            {
                var lastItem = RemoveLastItem();
                var sliceTo = _getTime(lastItem);
                var content = _cachedPage.ToArray();
                _cachedPage.Clear();
                _cachedPage.Add(lastItem);
                return new Slice<T>(sliceFrom, sliceTo, content);
            }
            else
            {
                if (_cachedPage.Count == 0)
                    return null;

                DateTime sliceTo;

                if (_to != null)
                    sliceTo = _to.Value;
                else if (_getEndTime != null)
                    sliceTo = _getEndTime(_cachedPage.Last());
                else
                    sliceTo = _getTime(_cachedPage.Last()) + TimeSpan.FromTicks(1); // clumsy hack

                var content = _cachedPage.ToArray();
                _cachedPage.Clear();

                return new Slice<T>(sliceFrom, sliceTo, content);
            }
        }

        private T RemoveLastItem()
        {
            var lastIndex = _cachedPage.Count - 1;
            var lastItem = _cachedPage[lastIndex];
            _cachedPage.RemoveAt(lastIndex);
            return lastItem;
        }
    }
}
